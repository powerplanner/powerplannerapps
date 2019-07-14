using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;
using PCLStorage;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public static class AccountsManager
    {
        private class CachedAccountEntry
        {
            private WeakReference<AccountDataItem> _account;

            public AccountDataItem Account
            {
                get
                {
                    if (_account == null)
                        return null;

                    AccountDataItem a;

                    if (_account.TryGetTarget(out a))
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

            public CachedAccountEntry(Guid localAccountId)
            {
                //Semaphore = new Semaphore(0, 1, "AccountDataItem-" + localAccountId);
            }
        }

        public static event EventHandler<Guid> OnAccountDeleted;

        /// <summary>
        /// This event currently isn't supported
        /// </summary>
        public static event EventHandler<AccountDataItem> OnAccountAdded;

        private static readonly Dictionary<Guid, CachedAccountEntry> _cachedAccounts = new Dictionary<Guid, CachedAccountEntry>();

        internal static void ClearCachedAccounts()
        {
            _cachedAccounts.Clear();
        }

        private static CachedAccountEntry GetCachedEntry(Guid localAccountId)
        {
            lock (_cachedAccounts)
            {
                CachedAccountEntry entry;

                _cachedAccounts.TryGetValue(localAccountId, out entry);

                return entry;
            }
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

        private static async Task<AccountDataItem> GetOrLoadHelper(Guid localAccountId)
        {
            Debug.WriteLine("GetOrLoad Account: " + localAccountId);

            CachedAccountEntry entry;

            lock (_cachedAccounts)
            {
                if (!_cachedAccounts.TryGetValue(localAccountId, out entry))
                {
                    entry = new CachedAccountEntry(localAccountId);
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
                Guid localAccountId;

                if (Guid.TryParse(accountFolder.Name, out localAccountId))
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
            using (Stream s = await file.OpenAsync(FileAccess.Read))
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

            bool needsSyncSettings = false;

            // Upgrade account data
            if (account.AccountDataVersion < 2)
            {
                // We introduced syncing selected semester in this version.
                // So if we upgraded, we need to make sure we force sync
                if (account.CurrentSemesterId != Guid.Empty)
                {
                    account.NeedsToSyncSettings = true;
                    needsSyncSettings = true;
                }
            }

            if (account.AccountDataVersion < AccountDataItem.CURRENT_ACCOUNT_DATA_VERSION)
            {
                account.AccountDataVersion = AccountDataItem.CURRENT_ACCOUNT_DATA_VERSION;
                var dontWait = Save(account); // Don't wait, otherwise we would get in a dead lock
            }

            if (needsSyncSettings)
            {
                var dontWait = SyncLayer.Sync.SyncSettings(account);
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
                        using (Stream s = await tempAccountFile.OpenAsync(FileAccess.ReadAndWrite))
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

            if (OnAccountDeleted != null)
                OnAccountDeleted(null, localAccountId);

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

        public class UsernameWasEmptyException : AccountException { }

        public class UsernameInvalidException : AccountException { }

        public class UsernameExistsLocallyException : AccountException
        {
        }

        public abstract class AccountException : Exception { }

        /// <summary>
        /// Throws error if no username was entered, invalid characters were used, or if the username is already taken. Otherwise returns null.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new UsernameWasEmptyException();

            if (!Credentials.IsUsernameOkay(username))
                throw new UsernameInvalidException();

            var accounts = await GetAllAccounts();

            if (accounts.Any(i => i.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)))
                throw new UsernameExistsLocallyException();
        }

        /// <summary>
        /// Thread safe. Throws various exceptions if cannot be created.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <param name="accountId"></param>
        /// <param name="deviceId"></param>
        /// <param name="rememberUsername"></param>
        /// <param name="rememberPassword"></param>
        /// <param name="autoLogin"></param>
        /// <returns></returns>
        public static async Task<AccountDataItem> CreateAccount(string username, string token, long accountId, int deviceId, bool rememberUsername, bool rememberPassword, bool autoLogin)
        {
            await ValidateUsername(username);

            // Initialize data
            AccountDataItem account = new AccountDataItem(Guid.NewGuid())
            {
                Username = username,
                Token = token,
                AccountId = accountId,
                DeviceId = deviceId,
                RememberUsername = rememberUsername,
                RememberPassword = rememberPassword,
                AutoLogin = autoLogin
            };

            // Place it in the cache
            lock (_cachedAccounts)
            {
                _cachedAccounts[account.LocalAccountId] = new CachedAccountEntry(account.LocalAccountId)
                {
                    Account = account
                };
            }

            try
            {
                // And save the account
                await Save(account);
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
