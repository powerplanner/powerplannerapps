using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public static class AccountsManager
    {
        public const string DefaultOfflineAccountUsername = "DefaultOfflineAccount";

        private class CachedAccountEntry
        {
            private WeakReference<AccountDataItem> _account;

            public AccountDataItem Account
            {
                get
                {
                    if (_account == null)
                        return null;

                    if (_account.TryGetTarget(out AccountDataItem a))
                        return a;

                    return null;
                }

                set
                {
                    if (value == null)
                        _account = null;
                    else
                        _account = new WeakReference<AccountDataItem>(value);
                }
            }
        }

        public static async System.Threading.Tasks.Task<AccountDataItem> CreateAndInitializeAccountAsync(string username, string localToken, string token, long accountId, int deviceId)
        {
            var account = await CreateAccountHelper.CreateAccountLocally(username, localToken, token, accountId, deviceId, needsInitialSync: false);

            if (account != null)
            {
                AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);

                // Add the default year/semester
                try
                {
                    DataItemYear year = new DataItemYear()
                    {
                        Identifier = Guid.NewGuid(),
                        Name = PowerPlannerResources.GetString("DummyFirstYear")
                    };

                    DataItemSemester semester = new DataItemSemester()
                    {
                        Identifier = Guid.NewGuid(),
                        UpperIdentifier = year.Identifier,
                        Name = PowerPlannerResources.GetString("DummyFirstSemester")
                    };

                    DataChanges changes = new DataChanges();
                    changes.Add(year);
                    changes.Add(semester);

                    await PowerPlannerApp.Current.SaveChanges(account, changes);
                    await account.SetCurrentSemesterAsync(semester.Identifier);
                    NavigationManager.MainMenuSelection = NavigationManager.MainMenuSelections.Schedule;

                    return account;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            return null;
        }

        public static event EventHandler<Guid> OnAccountDeleted;

        private static readonly Dictionary<Guid, CachedAccountEntry> _cachedAccounts = new Dictionary<Guid, CachedAccountEntry>();

        internal static void ClearCachedAccounts()
        {
            _cachedAccounts.Clear();
        }

        /// <summary>
        /// Thread safe. If account is already loaded and cached, returns that version. Otherwise loads and caches the account. If account doesn't exist, returns null.
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        public static async Task<AccountDataItem> GetOrLoad(Guid localAccountId)
        {
            var account = await GetOrLoadHelper(localAccountId);
            if (account != null)
            {
                TelemetryExtension.Current?.UpdateCurrentUser(account);
            }
            return account;
        }

        public static async Task<AccountDataItem> GetOrLoadOnlineAccount(long onlineAccountId)
        {
            lock (_cachedAccounts)
            {
                foreach (var cached in _cachedAccounts.Values)
                {
                    var account = cached.Account;
                    if (account != null)
                    {
                        if (account.AccountId == onlineAccountId)
                        {
                            return account;
                        }
                    }
                }
            }

            var allAccounts = await GetAllAccounts();
            return allAccounts.FirstOrDefault(i => i.AccountId == onlineAccountId);
        }

        private static async Task<AccountDataItem> GetOrLoadHelper(Guid localAccountId)
        {
            Debug.WriteLine("GetOrLoad Account: " + localAccountId);

            CachedAccountEntry entry;

            lock (_cachedAccounts)
            {
                if (!_cachedAccounts.TryGetValue(localAccountId, out entry))
                {
                    entry = new CachedAccountEntry();
                    _cachedAccounts[localAccountId] = entry;
                }

                // Cache entry exists
                else
                {
                    // But account still might have been disposed
                    var account = entry.Account;
                    if (account != null)
                    {
                        Debug.WriteLine("Returning cached account: " + localAccountId);
                        return account;
                    }
                }
            }

            Debug.WriteLine("GetOrLoad Account starting thread to enter data lock: " + localAccountId);

            // We'll have to enter a lock and then load it (if it's not already loaded at that point)
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                Debug.WriteLine("GetOrLoad Account acquiring lock: " + localAccountId);

                using (await Locks.LockAccounts())
                {
                    Debug.WriteLine("GetOrLoad Account acquired lock: " + localAccountId);

                    AccountDataItem account;

                    // Double check if account's already loaded (might have been loaded by the time we've entered the lock)
                    account = entry.Account;

                    if (account != null)
                    {
                        Debug.WriteLine("GetOrLoad Account, account already loaded: " + localAccountId);
                        return account;
                    }

                    Debug.WriteLine("Loading account: " + localAccountId);

                    account = await Load(localAccountId);

                    // If account doesn't exist
                    if (account == null)
                    {
                        Debug.WriteLine("Account didn't exist: " + localAccountId);
                        return null;
                    }

                    Debug.WriteLine("Loaded account: " + localAccountId);

                    entry.Account = account;

                    return account;
                }
            });
        }

        /// <summary>
        /// Thread safe
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AccountDataItem>> GetAllAccounts()
        {
            List<AccountDataItem> accounts = new List<AccountDataItem>();

            IList<IFolder> folders;
            folders = await System.Threading.Tasks.Task.Run(async delegate
            {
                using (await Locks.LockAccounts())
                {
                    return await FileHelper.GetAllAccountFolders();
                }
            });

            foreach (IFolder accountFolder in folders)
            {
                if (Guid.TryParse(accountFolder.Name, out Guid localAccountId))
                {
                    AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);

                    if (account != null)
                        accounts.Add(account);
                }
            }

            return accounts;
        }

        public static List<AccountDataItem> GetCurrentlyLoadedAccounts()
        {
            lock (_cachedAccounts)
            {
                List<AccountDataItem> answer = new List<AccountDataItem>();

                foreach (var a in _cachedAccounts.Values)
                {
                    AccountDataItem account = a.Account;

                    if (account != null)
                        answer.Add(account);
                }

                return answer;
            }
        }

        private static DataContractSerializer GetSerializer()
        {
            return new DataContractSerializer(typeof(AccountDataItem));
        }

        /// <summary>
        /// Not thread safe. Loads and returns the account if it exists, otherwise returns null
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        private static async Task<AccountDataItem> Load(Guid localAccountId)
        {
            var timeTracker = TimeTracker.Start();
            IFile file = await GetFile(localAccountId);
            timeTracker.End(3, "AccountsManager.Load GetFile");

            // If doesn't exist
            if (file == null)
                return null;

            // Otherwise 
            AccountDataItem account;
            timeTracker = TimeTracker.Start();
            using (Stream s = await file.OpenAsync(StorageEverywhere.FileAccess.Read))
            {
                timeTracker.End(3, "AccountsManager.Load open file stream");

                timeTracker = TimeTracker.Start();
                try
                {
                    account = (AccountDataItem)GetSerializer().ReadObject(s);
                }
                catch (Exception ex)
                {
                    // Sometimes the file becomes corrupt, nothing we can do, they'll have to log in again
                    TelemetryExtension.Current?.TrackEvent("Error_FailDeserializeAccount", new Dictionary<string, string>()
                    {
                        { "Exception", ex.ToString() }
                    });
                    return null;
                }
                timeTracker.End(3, "AccountsManager.Load deserializing stream");
            }

            account.LocalAccountId = localAccountId;

            SyncLayer.Sync.ChangedSetting? changedSettings = null;
            bool needsClassRemindersReset = false;

            // Upgrade account data
            if (account.AccountDataVersion < 2)
            {
                // We introduced syncing selected semester in this version.
                // So if we upgraded, we need to make sure we force sync
                if (account.CurrentSemesterId != Guid.Empty)
                {
                    account.NeedsToSyncSettings = true;

                    if (changedSettings == null)
                    {
                        changedSettings = SyncLayer.Sync.ChangedSetting.SelectedSemesterId;
                    }
                    else
                    {
                        changedSettings = changedSettings.Value | SyncLayer.Sync.ChangedSetting.SelectedSemesterId;
                    }
                }
            }

            if (account.AccountDataVersion < 3)
            {
                // We introduced auto-selecting the week that the schedule changes on,
                // so users in Spain will now have everything as Monday for first day
                DayOfWeek cultureFirstDayOfWeek = System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;
                if (account.WeekChangesOn == DayOfWeek.Sunday && account.WeekChangesOn != cultureFirstDayOfWeek)
                {
                    account.NeedsToSyncSettings = true;
                    account.SetWeekSimple(cultureFirstDayOfWeek, account.CurrentWeek);

                    if (changedSettings == null)
                    {
                        changedSettings = SyncLayer.Sync.ChangedSetting.WeekOneStartsOn;
                    }
                    else
                    {
                        changedSettings = changedSettings.Value | SyncLayer.Sync.ChangedSetting.WeekOneStartsOn;
                    }

                    // While we technically should update their reminders/schedule tile (since if
                    // they were using two-week schedules, that info changed), we'll skip doing that
                    // since it's extra complicated to load the data items here while also loading the
                    // account, and if they were using two week, they probably already have this set
                    // correctly anyways
                }
            }

            if (account.AccountDataVersion < 4)
            {
                // Set to the default timespan (otherwise this would be null when upgrading)
                account.ClassRemindersTimeSpan = AccountDataItem.DefaultClassRemindersTimeSpan;
                needsClassRemindersReset = true;
            }

            if (account.AccountDataVersion < AccountDataItem.CURRENT_ACCOUNT_DATA_VERSION)
            {
                account.AccountDataVersion = AccountDataItem.CURRENT_ACCOUNT_DATA_VERSION;
                _ = Save(account); // Don't wait, otherwise we would get in a dead lock
            }

            if (changedSettings != null)
            {
                _ = SyncLayer.Sync.SyncSettings(account, changedSettings.Value);
            }

            if (needsClassRemindersReset)
            {
                _ = ClassRemindersExtension.Current?.ResetAllRemindersAsync(account);
            }

            return account;
        }

        /// <summary>
        /// Thread safe, runs on own thread for data lock.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static async Task Save(AccountDataItem account)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Serialize on current thread
                GetSerializer().WriteObject(stream, account);
                stream.Position = 0;

                await Task.Run(async delegate
                {
                    using (await Locks.LockAccounts())
                    {
                        // Get the account folder
                        var timeTracker = TimeTracker.Start();
                        IFolder accountFolder = await FileHelper.GetOrCreateAccountFolder(account.LocalAccountId);
                        timeTracker.End(3, "AccountsManager.Save GetOrCreateAccountFolder");

                        // Create a temp file to write to
                        timeTracker = TimeTracker.Start();
                        IFile tempAccountFile = await accountFolder.CreateFileAsync("TempAccountData.dat", CreationCollisionOption.ReplaceExisting);
                        timeTracker.End(3, "AccountsManager.Save creating temp file");

                        // Write the data to the temp file
                        timeTracker = TimeTracker.Start();
                        using (Stream s = await tempAccountFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                        {
                            timeTracker.End(3, "AccountsManager.Save opening file stream");

                            timeTracker = TimeTracker.Start();
                            stream.CopyTo(s);
                            timeTracker.End(3, "AccountsManager.Save copying stream to file");
                        }

                        // Move the temp file to the actual file
                        timeTracker = TimeTracker.Start();
                        await tempAccountFile.RenameAsync(FileNames.ACCOUNT_FILE_NAME, NameCollisionOption.ReplaceExisting);
                        timeTracker.End(3, "AccountsManager.Save renaming temp file to final");
                    }
                });
            }
        }

        /// <summary>
        /// Thread safe, but will lock the thread
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        public static async Task Delete(Guid localAccountId)
        {
            await Task.Run(async delegate
            {
                using (await Locks.LockAccounts())
                {
                    using (await Locks.LockDataForWriteAsync("AccountsManager.Delete"))
                    {
                        IFolder accountFolder = await FileHelper.GetAccountFolder(localAccountId);

                        if (accountFolder != null)
                        {
                            AccountDataStore.Dispose(localAccountId);

                            try
                            {
                                await accountFolder.DeleteAsync();
                            }

                            catch { }
                        }
                    }

                    _cachedAccounts.Remove(localAccountId);
                }
            });

            OnAccountDeleted?.Invoke(null, localAccountId);

            // Clear reminders
            try
            {
                RemindersExtension.Current?.ClearReminders(localAccountId);
            }
            catch { }

            // Clear calendar integration
            try
            {
                if (AppointmentsExtension.Current != null)
                {
                    await AppointmentsExtension.Current.DeleteAsync(localAccountId);
                }
            }
            catch { }

            // Unpin tiles
            try
            {
                TilesExtension.Current?.UnpinAllTilesForAccount(localAccountId);
            }
            catch { }
        }

        public class UsernameWasEmptyException : AccountException
        {
            public override string FriendlyMessage => "You must provide a username!";
        }

        public class UsernameInvalidException : AccountException
        {
            public UsernameInvalidException(string friendlyMessage)
            {
                _friendlyMessage = friendlyMessage;
            }

            private string _friendlyMessage;
            public override string FriendlyMessage => _friendlyMessage;
        }

        public class UsernameExistsLocallyException : AccountException
        {
            public override string FriendlyMessage => "This username already exists on this device.";
        }

        public abstract class AccountException : Exception
        {
            public virtual string FriendlyMessage => "";
        }

        /// <summary>
        /// Throws error if no username was entered, invalid characters were used, or if the username is already taken. Otherwise returns null.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task ValidateUsernameAsync(string username)
        {
            var ex = await GetUsernameErrorAsync(username);
            if (ex != null)
            {
                throw ex;
            }
        }

        public static async Task<AccountException> GetUsernameErrorAsync(string username)
        {
            var invalidMessage = Credentials.GetUsernameError(username);
            if (invalidMessage != null)
            {
                return new UsernameInvalidException(invalidMessage);
            }

            var accounts = await GetAllAccounts();

            if (accounts.Any(i => i.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)))
                return new UsernameExistsLocallyException();

            return null;
        }

        /// <summary>
        /// Thread safe. Throws various exceptions if cannot be created.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="localToken"></param>
        /// <param name="token"></param>
        /// <param name="accountId"></param>
        /// <param name="deviceId"></param>
        /// <param name="rememberUsername"></param>
        /// <param name="rememberPassword"></param>
        /// <param name="autoLogin"></param>
        /// <returns></returns>
        public static async Task<AccountDataItem> CreateAccount(string username, string localToken, string token, long accountId, int deviceId, bool rememberUsername, bool rememberPassword, bool autoLogin, bool needsInitialSync)
        {
            await ValidateUsernameAsync(username);

            // Initialize data
            AccountDataItem account = new AccountDataItem(Guid.NewGuid())
            {
                Username = username,
                LocalToken = localToken,
                Token = token,
                AccountId = accountId,
                DeviceId = deviceId,
                RememberUsername = rememberUsername,
                RememberPassword = rememberPassword,
                AutoLogin = autoLogin,
                NeedsToSyncSettings = true, // Needs settings uploaded since we're setting WeekOneStartsOn
                WeekOneStartsOn = ToolsPortable.DateTools.Last(System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek, DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc)),
                NeedsInitialSync = needsInitialSync
            };

            // Place it in the cache
            lock (_cachedAccounts)
            {
                _cachedAccounts[account.LocalAccountId] = new CachedAccountEntry()
                {
                    Account = account
                };
            }

            try
            {
                // And save the account
                await Save(account);

                TelemetryExtension.Current?.UpdateCurrentUser(account);
            }

            catch
            {
                // If it failed to save we remove from the cache
                lock (_cachedAccounts)
                {
                    _cachedAccounts.Remove(account.LocalAccountId);
                }

                // And then we throw
                throw;
            }

            return account;
        }

        /// <summary>
        /// Not thread safe. Gets the file, or returns null if the file wasn't found
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        private static async Task<IFile> GetFile(Guid localAccountId)
        {
            try
            {
                return await FileSystem.Current.LocalStorage.GetFileByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId), FileNames.ACCOUNT_FILE_NAME);
            }
            
            catch
            {
                return null;
            }
        }

        public static Guid GetLastLoginLocalId()
        {
            return Settings.LastLoginLocalId;
        }

        public static async Task<AccountDataItem> GetLastLogin()
        {
            Guid localId = GetLastLoginLocalId();

            if (localId == Guid.Empty)
                return null;

            return await GetOrLoad(localId);
        }

        public static async void SetLastLoginIdentifier(Guid loginIdentifier)
        {
            Settings.LastLoginLocalId = loginIdentifier;

            try
            {
                if (TilesExtension.Current != null)
                {
                    await TilesExtension.Current.UpdatePrimaryTileNotificationsAsync();
                }
            }

            catch { }
        }
    }
}
