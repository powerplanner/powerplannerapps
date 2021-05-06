using Newtonsoft.Json;
using StorageEverywhere;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.NetworkInfo;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.SyncLayer
{
    public static class SyncExtensions
    {
        public static Func<string> GetPlatform;
        public static Func<string> GetAppName;
    }

    public class SyncResult
    {
        public string Error { get; set; }

        public List<SyncError> UpdateErrors { get; private set; } = new List<SyncError>();

        public SaveChangesTasks SaveChangesTask { get; internal set; }

        public Guid? SelectedSemesterId { get; internal set; }
    }

    public class SyncError
    {
        public string Name { get; private set; }
        public string Message { get; private set; }
        public DateTime Date { get; private set; }

        internal SyncError(string name, string message)
        {
            Name = name;
            Message = message;
            Date = DateTime.Now;
        }

        public override string ToString()
        {
            return Name + " - " + Date + "\n" + Message;
        }
    }

    public class SyncFinishedEventArgs
    {
        public AccountDataItem Account { get; private set; }
        public SyncResult Result { get; private set; }

        public SyncFinishedEventArgs(AccountDataItem account, SyncResult result)
        {
            Account = account;
            Result = result;
        }
    }

    public class SyncQueuedEventArgs
    {
        public AccountDataItem Account { get; private set; }
        public Task<SyncResult> ResultTask { get; private set; }

        public SyncQueuedEventArgs(AccountDataItem account, Task<SyncResult> resultTask)
        {
            Account = account;
            ResultTask = resultTask;
        }
    }

    public class UploadImageProgressEventArgs
    {
        public AccountDataItem Account { get; private set; }

        /// <summary>
        /// Percent between 0.0 and 1.0
        /// </summary>
        public double Progress { get; private set; }

        public UploadImageProgressEventArgs(AccountDataItem account, double progress)
        {
            Account = account;
            Progress = progress;
        }
    }

    public static class Sync
    {
        public static event EventHandler<SyncQueuedEventArgs> SyncQueued;
        public static event EventHandler<UploadImageProgressEventArgs> UploadImageProgress;


        private class AccountSyncRequest
        {
            public AccountDataItem Account { get; private set; }

            internal TaskCompletionSource<SyncResult> TaskCompletionSource { get; private set; } = new TaskCompletionSource<SyncResult>();

            private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public CancellationToken CancellationToken
            {
                get { return _cancellationTokenSource.Token; }
            }

            public void Cancel()
            {
                _cancellationTokenSource.Cancel();
                TaskCompletionSource.TrySetCanceled();
            }

            public AccountSyncRequest(AccountDataItem account)
            {
                Account = account;
            }
        }

        private static List<AccountSyncRequest> _queuedRequests = new List<AccountSyncRequest>();

        private static AccountSyncRequest _currTask;

        private static object _lock = new object();

        /// <summary>
        /// Places the account on the queue to be synced (will merge with existing queued if there are any), async task completes once the account is synced.
        /// Does NOT throw exceptions (except operation canceled), will always return a result.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static Task<SyncResult> SyncAccountAsync(AccountDataItem account)
        {
            try
            {
                if (!account.IsOnlineAccount)
                    return new Task<SyncResult>(NotOnlineAccount);

                var task = SyncAccountAsyncHelper(account);

                try
                {
                    if (SyncQueued != null)
                        SyncQueued(null, new SyncQueuedEventArgs(account, task));
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                return task;
            }

            catch (OperationCanceledException)
            {
                throw;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return new Task<SyncResult>(GenericFailure);
            }
        }

        /// <summary>
        /// Same as <see cref="SyncAccountAsync(AccountDataItem)"/> except it is non-blocking/non-async and works in the background
        /// </summary>
        /// <param name="account"></param>
        public static async void StartSyncAccount(AccountDataItem account)
        {
            try
            {
                await SyncAccountAsync(account);
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static SyncResult GenericFailure()
        {
            return new SyncResult()
            {
                Error = "Sync failed, error info sent to developer."
            };
        }

        private static SyncResult NotOnlineAccount()
        {
            return new SyncResult()
            {
                Error = "Not online account"
            };
        }

        public static bool CancelAll()
        {
            bool canceledCurrent = false;

            lock (_lock)
            {
                // Cancel queued syncs in reverse order
                foreach (var req in _queuedRequests.Reverse<AccountSyncRequest>())
                {
                    req.Cancel();
                }

                // Then clear all of them
                _queuedRequests.Clear();

                // Then cancel/clear the current sync
                if (_currTask != null)
                {
                    _currTask.Cancel();
                    _currTask = null;
                    canceledCurrent = true;
                }

                if (CancelImageUpload())
                    canceledCurrent = true;
            }

            return canceledCurrent;
        }
        
        public static bool CancelImageUpload()
        {
            bool canceledCurrent = false;
            lock (_lockUploadImages)
            {
                if (_isUploadingImage)
                    canceledCurrent = true;

                if (_uploadImagesCancellationTokenSource != null)
                {
                    _uploadImagesCancellationTokenSource.Cancel();
                    _uploadImagesCancellationTokenSource = null;
                }

                _queuedAccountToUploadImages = null;
            }

            return canceledCurrent;
        }

        private static Task<SyncResult> SyncAccountAsyncHelper(AccountDataItem account)
        {
            try
            {
                AccountSyncRequest requestToExecute = null;

                lock (_lock)
                {
                    // Make sure images stop uploading (the current one can finish concurrently, but after that it needs to be stopped)
                    lock (_lockUploadImages)
                    {
                        _queuedAccountToUploadImages = null;
                    }

                    AccountSyncRequest queuedRequest = _queuedRequests.FirstOrDefault(i => i.Account == account);

                    // If there's an existing in the queue, we'll use that
                    if (queuedRequest != null)
                        return queuedRequest.TaskCompletionSource.Task;

                    var request = new AccountSyncRequest(account);

                    // If something's syncing right now
                    if (_currTask != null)
                    {
                        // Queue it up
                        _queuedRequests.Add(request);
                        return request.TaskCompletionSource.Task;
                    }

                    // Otherwise nothing is pending/syncing, so we go
                    _currTask = request;
                    requestToExecute = request;
                }

                // We execute outside of the lock (we know that it'll be initialized if it got here)
                BeginExecuteSyncRequest(requestToExecute);
                return requestToExecute.TaskCompletionSource.Task;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return new Task<SyncResult>(GenericFailure);
            }
        }


        private static async void BeginExecuteSyncRequest(AccountSyncRequest request)
        {
            SyncResult result;

            try
            {
                result = await ExecuteSync(request);
            }

            catch (OperationCanceledException)
            {
                // TaskCompletionSource already set to canceled, and all requests are being cleared
                // So just do nothing
                return;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                result = GenericFailure();
            }

            // Trigger completed
            request.TaskCompletionSource.TrySetResult(result);


            bool shouldUploadImages = false;
            AccountSyncRequest nextRequest = null;

            lock (_lock)
            {
                // Handle switching to next task or image uploads
                _currTask = null;

                // If there's another sync request
                if (_queuedRequests.Count > 0)
                {
                    // Select it as the current and remove from queue
                    // It'll be executed later when I exit the lock
                    nextRequest = _queuedRequests.First();
                    _queuedRequests.RemoveAt(0);
                    _currTask = nextRequest;
                }

                // Otherwise no pending sync request
                else
                {
                    // If there was no error, we can queue image upload
                    if (result.Error == null)
                        shouldUploadImages = true;
                }
            }

            if (nextRequest != null)
                BeginExecuteSyncRequest(nextRequest);

            else if (shouldUploadImages)
                queueUploadImages(request.Account);
        }

        private static async Task<SyncResult> ExecuteSync(AccountSyncRequest request)
        {
            var account = request.Account;

            try
            {
                if (account.NeedsToSyncSettings)
                {
                    await SyncSettings(account);
                    account.NeedsToSyncSettings = false;
                }

                string pushChannelString = null;

                if (!account.IsPushDisabled && PushExtension.Current != null)
                {
                    try
                    {
                        pushChannelString = await PushExtension.Current.GetPushChannelUri();
                    }

                    catch { }

                    request.CancellationToken.ThrowIfCancellationRequested();
                }

                SyncRequest req;

                var accountDataStore = await AccountDataStore.Get(account.LocalAccountId);

                request.CancellationToken.ThrowIfCancellationRequested();

                var updatesAndDeletes = await accountDataStore.GetUpdatesAndDeletesAsync();
                bool isBatchingUpdates = updatesAndDeletes.Item3;

                request.CancellationToken.ThrowIfCancellationRequested();

                string pushChannelForAccount;

                pushChannelForAccount = pushChannelString;

                HashSet<ItemType> reSyncNeededFor = new HashSet<ItemType>();
                HashSet<MegaItemType> megaItemReSyncNeededFor = new HashSet<MegaItemType>();

                // This is when we added grade linking to task/event, so we need to pull down
                // the tasks/events, so that we get their updated grade values
                if (account.SyncedDataVersion < 2)
                {
                    // We'll also be pulling in the holidays
                    reSyncNeededFor.Add(ItemType.MegaItem);
                }

                else if (account.SyncedDataVersion < 3)
                {
                    megaItemReSyncNeededFor.Add(MegaItemType.Holiday);
                }

                // Added class start/end dates to classes
                if (account.SyncedDataVersion < 4)
                {
                    reSyncNeededFor.Add(ItemType.Class);
                }

                // Added GpaType and PassingGrade to classes
                if (account.SyncedDataVersion < 5)
                {
                    reSyncNeededFor.Add(ItemType.Class);
                }

                req = new SyncRequest()
                {
                    Updates = updatesAndDeletes.Item1,
                    Deletes = updatesAndDeletes.Item2,
                    CurrentChangeNumber = account.CurrentChangeNumber,
                    DeviceId = account.DeviceId,
                    Platform = SyncExtensions.GetPlatform(),
                    PushChannel = pushChannelForAccount,
                    AppName = SyncExtensions.GetAppName(),
                    AppVersion = Variables.VERSION.ToString(),
                    SyncVersion = 4,
                    // SyncVersion 2: if individual item update fails, it'll return error just for that item
                    // SyncVersion 3: MegaItem
                    // SyncVersion 4: Times added to mega items (anything below this version, when editing DateTime1 on MegaItem, the time component will be ignored)
                    MaxItemsToReturn = 200,
                    CurrentDefaultGradeScaleIndex = account.CurrentDefaultGradeScaleIndex
                };

                if (reSyncNeededFor.Count > 0)
                {
                    req.ReSyncNeededFor = reSyncNeededFor;
                }
                if (megaItemReSyncNeededFor.Count > 0)
                {
                    req.MegaItemReSyncNeededFor = megaItemReSyncNeededFor;
                }

                // Perform online sync
                SyncResponse response;
                SyncResult answer = new SyncResult();
                bool isMultiPart = false;
                int responseChangeNumber = 0;
                bool needsAnotherSync = false;
                IEnumerable<DeletedItem> deletedItems = null;
                UpdatedItems updatedItems = null;
                int partialSyncNumber = 0;
                IFolder partialSyncsFolder = null;
                List<IFile> partialSyncFiles = null;

                do
                {
                    try
                    {
                        response = await account.PostAuthenticatedAsync<SyncRequest, SyncResponse>(Website.URL + "syncmodern", req, request.CancellationToken);
                    }

                    catch (OperationCanceledException)
                    {
                        throw;
                    }

                    catch (Exception ex)
                    {
                        // Ignore typical issues, only capture unusal ones
                        if (!ExceptionHelper.IsHttpWebIssue(ex))
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                            Debug.WriteLine("Error syncing (WebException): " + ex.ToString());
                        }

                        return new SyncResult()
                        {
                            Error = "Offline."
                        };
                    }

                    if (response == null)
                    {
                        Debug.WriteLine("Sync response was null");

                        return new SyncResult()
                        {
                            Error = "Sync response was null"
                        };
                    }

                    if (response.Error != null)
                    {
                        // Skip logging the common ones where user simply changed their credentials
                        if (response.Error != SyncResponse.INCORRECT_PASSWORD
                            && response.Error != SyncResponse.USERNAME_CHANGED
                            && response.Error != SyncResponse.DEVICE_NOT_FOUND
                            && response.Error != SyncResponse.INCORRECT_CREDENTIALS
                            && !response.Error.StartsWith("Backend error."))
                        {
                            TelemetryExtension.Current?.TrackException(new Exception("SyncError: " + response.Error));
                        }
                        Debug.WriteLine("Sync error: " + response.Error);

                        return new SyncResult()
                        {
                            Error = response.Error
                        };
                    }

                    Debug.WriteLine("Sync response received. Updates: " + response.UpdatedItems.AsEnumerable().Count() + ". Deletes: " + response.DeletedItems.Count());

                    // Record errors about the items
                    if (response.UpdateErrors != null && response.UpdateErrors.Count > 0)
                    {
                        foreach (var updateError in response.UpdateErrors)
                            answer.UpdateErrors.Add(new SyncError("Update Item Error", updateError.ToString()));

                        Debug.WriteLine("Had " + response.UpdateErrors.Count + " UpdateErrors");
                    }

                    // Merge deletes
                    if (deletedItems == null)
                    {
                        deletedItems = response.DeletedItems;
                    }
                    else
                    {
                        deletedItems = deletedItems.Concat(response.DeletedItems);
                    }

                    // If it's the first sync, we log the change number
                    if (!isMultiPart)
                    {
                        responseChangeNumber = response.ChangeNumber;
                    }
                    else
                    {
                        // Otherwise, if the change number has changed since the first sync,
                        // we need to request another sync after we're done to grab any item that was
                        // inserted while we were syncing
                        if (response.ChangeNumber > responseChangeNumber)
                        {
                            needsAnotherSync = true;
                        }
                    }

                    // If it's multi-part sync and this is the FIRST of the parts
                    if (response.NextPage != null && !isMultiPart)
                    {
                        Debug.WriteLine("Using multi-part sync");

                        isMultiPart = true;

                        // Clear any updates/deletes so we don't send them again
                        req.Updates = null;
                        req.Deletes = null;

                        // Create the folder for partial syncs (deletes existing folder)
                        partialSyncsFolder = await FileHelper.CreatePartialSyncsFolder();
                        partialSyncFiles = new List<IFile>();
                    }

                    // If it's multi-part sync
                    if (response.NextPage != null)
                    {
                        // Pass the next page
                        req.Page = response.NextPage;
                    }

                    if (response.UpdatedItems != null)
                    {
                        // If we were in a multi-part sync, we need to save updated items to a file
                        if (isMultiPart && (response.UpdatedItems.MegaItems.Count > 0 || response.UpdatedItems.Grades.Count > 0))
                        {
                            var partialFile = await partialSyncsFolder.CreateFileAsync("Partial" + partialSyncNumber, CreationCollisionOption.ReplaceExisting);
                            partialSyncFiles.Add(partialFile);
                            var jsonSerializer = new JsonSerializer();
                            using (var stream = await partialFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                            {
                                using (var streamWriter = new StreamWriter(stream))
                                {
                                    jsonSerializer.Serialize(streamWriter, new UpdatedItems()
                                    {
                                        MegaItems = response.UpdatedItems.MegaItems,
                                        Grades = response.UpdatedItems.Grades
                                    });
                                }
                            }
                        }

                        // If first updated items we've received
                        if (updatedItems == null)
                        {
                            updatedItems = response.UpdatedItems;

                            // If it's multi-part, we need to strip out the mega items and grades
                            // (we've already saved them)
                            if (isMultiPart)
                            {
                                updatedItems.MegaItems.Clear();
                                updatedItems.Grades.Clear();
                            }
                        }
                        else
                        {
                            // Otherwise we merge and cache everything except MegaItems and Grades
                            updatedItems.MergeEverythingExceptMegaItemsAndGrades(response.UpdatedItems);
                        }
                    }

                    // If there's no next page, we're done
                    if (response.NextPage == null)
                    {
                        break;
                    }

                    // Otherwise, we continue to make the next request (the next page has already been passed)
                    partialSyncNumber++;

                    if (partialSyncNumber > 200)
                    {
                        throw new Exception("Partial sync seems to be in an infinite loop. NextPage: " + response.NextPage);
                    }

                } while (true);

                // Mark our items as sent
                await accountDataStore.ClearSyncing();

                // Create changes so we can process the sync changes.
                // If we are NOT doing multi-part, we'll throw if item already has been added.
                // Otherwise, we won't throw, since when doing a multi-part where we had
                // updates AND are doing a re-sync, there's no way for the server to ensure that
                // it doesn't send down duplicate items that it already sent down as part of the normal items.
                DataChanges changes = CreateChangesFromSyncResponse(updatedItems, deletedItems, throwIfExists: !isMultiPart);

                answer.SaveChangesTask = new SaveChangesTasks();

                // If there's actually changes, we'll process them
                if (!changes.IsEmpty())
                {
                    // Process the changes
                    answer.SaveChangesTask = await accountDataStore.ProcessOnlineChanges(changes, isMultiPart);
                }

                bool accountChanged = false;

                // If this was a multi-part sync
                if (isMultiPart)
                {
                    // We need to load and insert the mega items and grades
                    foreach (var partialFile in partialSyncFiles)
                    {
                        UpdatedItems partialItems;
                        using (var stream = await partialFile.OpenAsync(StorageEverywhere.FileAccess.Read))
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                using (var jsonReader = new JsonTextReader(streamReader))
                                {
                                    partialItems = new JsonSerializer().Deserialize<UpdatedItems>(jsonReader);
                                }
                            }
                        }

                        // Create changes so we can process the sync changes.
                        // We WILL throw if exists here, since these items are all part of a single response from the
                        // server, which means they're guaranteed to not have conflicts.
                        var changesPartial = CreateChangesFromSyncResponse(partialItems, null, throwIfExists: true);

                        // If there's actually changes, we'll process them
                        if (!changesPartial.IsEmpty())
                        {
                            // Process the changes
                            var saveChangesTask = await accountDataStore.ProcessOnlineChanges(changesPartial, true);
                            if (saveChangesTask.NeedsAccountToBeSaved)
                            {
                                answer.SaveChangesTask.NeedsAccountToBeSaved = true;
                            }
                        }
                    }

                    var dontWait = FileHelper.DeletePartialSyncsFolder();

                    // Now we need to reset calendar, and update tiles, and update reminders, since we disabled those
                    // while doing the multi-part insert.
                    if (account.IsAppointmentsUpToDate)
                    {
                        account.IsAppointmentsUpToDate = false;
                        accountChanged = true;
                        if (AppointmentsExtension.Current != null)
                        {
                            try
                            {
                                AppointmentsExtension.Current.ResetAllIfNeeded(account, accountDataStore);
                            }
                            catch (Exception ex)
                            {
                                TelemetryExtension.Current?.TrackException(ex);
                            }
                        }
                    }

                    // Update the tiles (don't wait on it)
                    try
                    {
                        Debug.WriteLine("Updating tile notifications");
                        answer.SaveChangesTask.UpdateTilesTask = TilesExtension.Current?.UpdateTileNotificationsForAccountAsync(account, accountDataStore);
                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to update tile notifications");
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    // And update class reminders (don't wait on it)
                    try
                    {
                        Debug.WriteLine("Updating class reminders");
                        answer.SaveChangesTask.UpdateClassRemindersTask = ClassRemindersExtension.Current?.ResetAllRemindersAsync(account);
                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to update class reminders");
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    // And update reminders (don't wait on it)
                    try
                    {
                        Debug.WriteLine("Updating toast reminders");
                        answer.SaveChangesTask.UpdateRemindersTask = RemindersExtension.Current?.ResetReminders(account, accountDataStore);
                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to update toast reminders");
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }

                accountChanged = accountChanged || answer.SaveChangesTask.NeedsAccountToBeSaved;

                if (account.SyncedDataVersion < AccountDataItem.CURRENT_SYNCED_DATA_VERSION)
                {
                    account.SyncedDataVersion = AccountDataItem.CURRENT_SYNCED_DATA_VERSION;
                    accountChanged = true;
                }


                // Only apply server settings if we don't have any of our own settings to upload (otherwise we might overwrite settings if the user changed them while sync was happening)
                if (response.Settings != null && !account.NeedsToSyncSettings)
                {
                    // Apply the synced settings
                    if (account.ApplySyncedSettings(response.Settings, response.DefaultGradeScaleIndex))
                    {
                        accountChanged = true;
                    }

                    // For now we'll just return the semester ID and on initial login the login task can apply it
                    // since we don't handle this dynamically changing while the app is already loaded.
                    answer.SelectedSemesterId = response.Settings.SelectedSemesterId;
                }

                if (account.PremiumAccountExpiresOn != response.PremiumAccountExpiresOn)
                {
                    account.PremiumAccountExpiresOn = response.PremiumAccountExpiresOn;
                    accountChanged = true;
                }

                if (responseChangeNumber > 0)
                {
                    if (account.CurrentChangeNumber != responseChangeNumber)
                    {
                        account.CurrentChangeNumber = responseChangeNumber;
                        accountChanged = true;
                    }
                }

                if (account.CurrentDefaultGradeScaleIndex != response.DefaultGradeScaleIndex)
                {
                    account.DefaultGradeScale = response.Settings.DefaultGradeScale;
                }


                // If account properties was changed, save account
                if (accountChanged)
                    await AccountsManager.Save(account);

                // Log when last synced
                account.LastSyncOn = DateTime.Now;

                if (isBatchingUpdates && answer.Error == null)
                {
                    // Sync again and wait on it (so that initial account created sync waits on this)
                    return await ExecuteSync(request);
                }

                // Queue another sync if it's needed
                if (needsAnotherSync)
                {
                    try
                    {
                        var dontWait = SyncAccountAsync(account);
                    }
                    catch { }
                }


                return answer;
            }

            catch (OperationCanceledException) { return new SyncResult(); }
            
            catch (Exception ex)
            {
                // Ignore typical issues, only capture unusal ones
                if (ExceptionHelper.IsHttpWebIssue(ex))
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    Debug.WriteLine("Error syncing (WebException): " + ex.ToString());
                    return new SyncResult()
                    {
                        Error = "Offline."
                    };
                }

                Debug.WriteLine("Error syncing: " + ex.ToString());
                TelemetryExtension.Current?.TrackException(ex);
                return new SyncResult()
                {
                    Error = ex.ToString()
                };
            }
        }

        private static DataChanges CreateChangesFromSyncResponse(UpdatedItems updatedItems, IEnumerable<DeletedItem> deletedItems, bool throwIfExists)
        {
            DataChanges changes = new DataChanges();

            if (updatedItems != null)
            {
                // Get the updated items (already sorted based on level)
                var updatedItemsList = updatedItems.AsEnumerable();

                foreach (var u in updatedItemsList)
                {
                    BaseDataItem dataItem = CreateItem(u);

                    changes.Add(dataItem, throwIfExists);
                }
            }

            if (deletedItems != null)
            {
                foreach (var d in deletedItems)
                    changes.DeleteItem(d.Identifier, throwIfExists);
            }

            return changes;
        }

        internal static BaseDataItem[] GetSyncItemsAsDataItems(IEnumerable<BaseItem> syncItems)
        {
            return syncItems.Select(i => CreateItem(i)).ToArray();
        }



        /// <summary>
        /// Initializes the BaseItemWin entity, gives it this account, and deserializes the data from the item. Also copies the GUID Identifier from the item into the new entity.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static BaseDataItem CreateItem(BaseItem item)
        {
            BaseDataItem entity;

            if (item is Year)
                entity = new DataItemYear();

            else if (item is Semester)
                entity = new DataItemSemester();

            else if (item is Class)
                entity = new DataItemClass();

            else if (item is MegaItem)
                entity = new DataItemMegaItem();

            else if (item is WeightCategory)
                entity = new DataItemWeightCategory();

            else if (item is Grade)
                entity = new DataItemGrade();

            else if (item is Schedule)
                entity = new DataItemSchedule();


            else
            {
                Debug.WriteLine("CreateItem, item wasn't any of the types");
                return null;
            }

            entity.Identifier = item.Identifier; //deserialize doesn't copy the identifier
            entity.Deserialize(item, null);

            return entity;
        }

        /// <summary>
        /// Returns true if currently syncing (or sync queued to happen)
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentlySyncing()
        {
            lock (_lock)
            {
                return _currTask != null || _queuedRequests.Count > 0;
            }
        }

        private static object _lockUploadImages = new object();
        private static bool _isUploadingImage = false;
        private static AccountDataItem _queuedAccountToUploadImages;
        private static CancellationTokenSource _uploadImagesCancellationTokenSource;

        private static void queueUploadImages(AccountDataItem account)
        {
            Debug.WriteLine("Image upload enqueue requested.");

            lock (_lockUploadImages)
            {
                // If we're uploading right now
                if (_isUploadingImage)
                {
                    // We'll have this account upload next
                    _queuedAccountToUploadImages = account;
                    Debug.WriteLine("Already uploading images, account set for next upload");
                    return;
                }

                // If we're syncing right now
                if (IsCurrentlySyncing())
                {
                    Debug.WriteLine("Currently syncing, so image upload will be called when sync finishes");
                    // We'll let sync finish and then it'll call back to me
                    return;
                }

                // If this account is allowed to sync
                if (CanUploadImages(account))
                {
                    _isUploadingImage = true;
                    _queuedAccountToUploadImages = account;
                }

                else
                {
                    Debug.WriteLine("Not allowed to upload images based on current settings.");
                    return;
                }
            }
            
            _uploadImagesCancellationTokenSource = new CancellationTokenSource();
            
            uploadImages(account, _uploadImagesCancellationTokenSource.Token);
        }

        private static async void uploadImages(AccountDataItem account, CancellationToken cancellationToken)
        {
            try
            {
                lock (_lockUploadImages)
                {
                    if (_queuedAccountToUploadImages != account)
                    {
                        _isUploadingImage = false;
                        return;
                    }
                }

                Debug.WriteLine("Checking for images to upload");

                var dataStore = await AccountDataStore.Get(account.LocalAccountId);
                cancellationToken.ThrowIfCancellationRequested();

                string next = await dataStore.GetNextImageToUploadAsync();
                cancellationToken.ThrowIfCancellationRequested();

                // If there's no image to upload
                if (next == null)
                {
                    Debug.WriteLine("No next image to upload");

                    AccountDataItem nextAccount = null;

                    lock (_lockUploadImages)
                    {
                        // Find out if another account is queued
                        nextAccount = _queuedAccountToUploadImages;

                        // If not, we'll mark as not uploading
                        if (nextAccount == null)
                            _isUploadingImage = false;
                        else
                            _queuedAccountToUploadImages = null; // Otherwise that one becomes the curr
                    }

                    // Otherwise there was a next account, so try doing its uploads
                    if (nextAccount != null)
                        uploadImages(nextAccount, cancellationToken);

                    return;
                }

                Debug.WriteLine("Uploading image: " + next);
                await uploadImage(account, next, cancellationToken);
                Debug.WriteLine("Uploaded image: " + next);

                cancellationToken.ThrowIfCancellationRequested();

                {
                    AccountDataItem nextAccount = account;

                    lock (_lockUploadImages)
                    {


                        // If a different account is queued, we'll upload that one. Otherwise we'll continue with our current which might have another image
                        if (_queuedAccountToUploadImages != null)
                            nextAccount = _queuedAccountToUploadImages;
                    }

                    uploadImages(nextAccount, cancellationToken);
                }
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    if (UploadImageProgress != null)
                        UploadImageProgress(null, new UploadImageProgressEventArgs(account, 0));
                }

                else
                {
                    Debug.WriteLine("Exception uploading image: " + ex.ToString());

                    if (!(ex is HttpRequestException))
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }

                lock (_lockUploadImages)
                {
                    _queuedAccountToUploadImages = null;
                    _isUploadingImage = false;
                }
            }
        }

        private static async System.Threading.Tasks.Task uploadImage(AccountDataItem account, string image, CancellationToken cancellationToken)
        {
            IFile imageFile = null;

            //image might have been deleted, so we try/catch
            try
            {
                imageFile = await getImageFile(account, image);
            }

            catch { }

            cancellationToken.ThrowIfCancellationRequested();


            if (imageFile != null)
            {
                await uploadBytes(account, image, imageFile, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
            }

            //if the image was deleted, we still mark it as uploaded since there's nothing to upload
            try
            {
                var dataStore = await AccountDataStore.Get(account.LocalAccountId);
                await dataStore.MarkImageUploadedAsync(image);
            }

            catch { }
        }

        private static async Task<IFile> getImageFile(AccountDataItem account, string image)
        {
            IFolder imagesFolder = await FileHelper.GetOrCreateImagesFolder(account.LocalAccountId);

            return await imagesFolder.GetFileAsync(image);
        }

        private static async System.Threading.Tasks.Task uploadBytes(AccountDataItem account, string imageName, IFile imageFile, CancellationToken cancellationToken)
        {
            UploadProgressReporter reporter = new UploadProgressReporter(account);

            try
            {
                // https://stackoverflow.com/questions/16416601/c-sharp-httpclient-4-5-multipart-form-data-upload
                using (Stream stream = await imageFile.OpenAsync(StorageEverywhere.FileAccess.Read))
                {
                    using (var client = new HttpClient())
                    {
                        using (var content = new MultipartFormDataContent())
                        {
                            HeadersHelper.SetLoginCredentials(content.Headers, account);
                            HeadersHelper.SetApiKey(content.Headers);

                            var streamContent = new ToolsPortable.Web.ProgressStreamContent(stream, cancellationToken);
                            streamContent.Progress = (sentBytes, totalBytes, totalBytesExpected) =>
                            {
                                reporter.ReportBytesSent(totalBytes, totalBytesExpected);
                            };

                            DateTime lastSent = DateTime.MinValue;
                            DateTime uploadStartTime = DateTime.Now;

                            content.Add(streamContent, "image", imageName);

                            using (var message = await client.PostAsync(Website.URL + "uploadimagemultipart", content))
                            {
                                message.EnsureSuccessStatusCode();

                                using (var responseStream = await message.Content.ReadAsStreamAsync())
                                {
                                    using (var responseReader = new StreamReader(responseStream))
                                    {
                                        using (var jsonReader = new JsonTextReader(responseReader))
                                        {
                                            var resp = new JsonSerializer().Deserialize<UploadImageResponse>(jsonReader);
                                            if (resp.Error != null)
                                            {
                                                throw new Exception(resp.Error);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    reporter.ReportDone();
                }

                //and we're done!
            }

            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                    throw ex;

                reporter.ReportFailed();

                throw ex;
            }
        }

        private class UploadProgressReporter
        {
            private AccountDataItem _account;
            private const double _percentageOfInitial = 0.05;
            private double _percentageOfUploading;
            private double _percentageOfBlob;
            private long _totalBytes;
            private bool _done;

            public UploadProgressReporter(AccountDataItem account)
            {
                _account = account;
                _percentageOfUploading = Settings.AverageImageBlobSaveSpeedInBytesPerSecond / (Settings.AverageImageUploadSpeedInBytesPerSecond + Settings.AverageImageBlobSaveSpeedInBytesPerSecond) * (1 - _percentageOfInitial);
                _percentageOfBlob = (1 - _percentageOfUploading - _percentageOfInitial);

                SetProgress(_percentageOfInitial);
            }

            private void SetProgress(double progress)
            {
                UploadImageProgress?.Invoke(null, new UploadImageProgressEventArgs(_account, progress));
            }

            private DateTime _lastTimeSentBytesSent;
            private DateTime _uploadStartTime;
            public void ReportBytesSent(long sent, long total)
            {
                if (_totalBytes == 0)
                {
                    _totalBytes = total;
                    _uploadStartTime = DateTime.Now;
                }

                if (sent == total)
                {
                    double uploadSpeedInBytesPerSecond = total / (DateTime.Now - _uploadStartTime).TotalSeconds;

                    // Update the average upload speed
                    Settings.AverageImageUploadSpeedInBytesPerSecond = (Settings.AverageImageUploadSpeedInBytesPerSecond + uploadSpeedInBytesPerSecond * 2) / 3;

                    SetProgress(_percentageOfInitial + _percentageOfUploading);

                    StartBlobSave();
                }

                else if ((DateTime.Now - _lastTimeSentBytesSent).TotalMilliseconds > 300)
                {
                    double progress = _percentageOfInitial + sent / (double)total * _percentageOfUploading; // Only let it go up to the estimated percentage of uploading time
                    SetProgress(progress);
                    _lastTimeSentBytesSent = DateTime.Now;
                }
            }

            private DateTime _blobSaveStartTime;
            public void StartBlobSave()
            {
                _blobSaveStartTime = DateTime.Now;

                System.Threading.Tasks.Task.Run(async delegate
                {
                    try
                    {
                        var estimatedBlobSaveTimeInSeconds = _totalBytes / Settings.AverageImageBlobSaveSpeedInBytesPerSecond * 1.2; // We give it some extra buffer

                        // Progress for uploading the blob
                        while (true)
                        {
                            await System.Threading.Tasks.Task.Delay(300);

                            if (_done)
                            {
                                break;
                            }

                            double elapsedSeconds = (DateTime.Now - _blobSaveStartTime).TotalSeconds;
                            double blobProgress = elapsedSeconds / estimatedBlobSaveTimeInSeconds * 0.9; // Only let it go up to 90%
                            if (blobProgress > 0.9)
                            {
                                blobProgress = 0.9;
                            }
                            double progress = _percentageOfInitial + _percentageOfUploading + _percentageOfBlob * blobProgress;

                            lock (this)
                            {
                                if (_done)
                                {
                                    break;
                                }

                                SetProgress(progress);
                            }

                            if (blobProgress == 0.9)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                });
            }

            public void ReportDone()
            {
                double blobSpeedInBytesPerSecond = _totalBytes / (DateTime.Now - _blobSaveStartTime).TotalSeconds;

                lock (this)
                {
                    _done = true;
                }

                SetProgress(1);
                Settings.AverageImageBlobSaveSpeedInBytesPerSecond = (Settings.AverageImageBlobSaveSpeedInBytesPerSecond + blobSpeedInBytesPerSecond * 2) / 3;
            }

            public void ReportFailed()
            {
                SetProgress(1);
            }
        }

        /// <summary>
        /// Returns true if user is connected to a network that they can upload images on (like free WiFi), based on their settings
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private static bool CanUploadImages(AccountDataItem account)
        {
            NetworkCostType costType = NetworkCostType.Unlimited;

            if (NetworkInfoExtension.Current != null)
            {
                costType = NetworkInfoExtension.Current.GetNetworkCostType();
            }

            if (costType == NetworkCostType.Disconnected)
                return false;

            if (account.ImageUploadOption == ImageUploadOptions.Always)
                return true;

            if (account.ImageUploadOption == ImageUploadOptions.Never)
                return false;

            // Otherwise we're in the "Wi-Fi Only" option

            if (costType == NetworkCostType.Unlimited)
                return true;

            return false;
        }

        [Flags]
        public enum ChangedSetting
        {
            GpaOption,
            WeekOneStartsOn,
            SelectedSemesterId,
            SchoolTimeZone,
            DefaultGradeScale,
            DefaultDoesAverageGradeTotals,
            DefaultDoesRoundGradesUp
        }

        public static System.Threading.Tasks.Task SyncSettings(AccountDataItem account)
        {
            return SyncSettings(account,
                ChangedSetting.GpaOption |
                ChangedSetting.WeekOneStartsOn |
                ChangedSetting.SelectedSemesterId |
                ChangedSetting.SchoolTimeZone |
                ChangedSetting.DefaultGradeScale |
                ChangedSetting.DefaultDoesAverageGradeTotals |
                ChangedSetting.DefaultDoesRoundGradesUp);
        }

        private static SyncSettingsMultiWorkerQueue _syncSettingsMultiWorkerQueue = new SyncSettingsMultiWorkerQueue();

        public static System.Threading.Tasks.Task SyncSettings(AccountDataItem account, ChangedSetting changedSettings)
        {
            if (!account.IsOnlineAccount)
            {
                return System.Threading.Tasks.Task.CompletedTask;
            }

            return _syncSettingsMultiWorkerQueue.Enqueue(account.LocalAccountId, new Tuple<AccountDataItem, ChangedSetting>(account, changedSettings));
        }

        private class SyncSettingsMultiWorkerQueue : AsyncMultiWorkerQueue<Guid, Tuple<AccountDataItem, ChangedSetting>>
        {
            protected override AsyncWorkerQueue<Tuple<AccountDataItem, ChangedSetting>> CreateWorkerQueue()
            {
                return new SyncSettingsWorkerQueue();
            }

            private class SyncSettingsWorkerQueue : AsyncWorkerQueue<Tuple<AccountDataItem, ChangedSetting>>
            {
                protected override async System.Threading.Tasks.Task DoWorkAsync(Tuple<AccountDataItem, ChangedSetting> data)
                {
                    var account = data.Item1;
                    var changedSettings = data.Item2;

                    SyncedSettings settings = new SyncedSettings();

                    if (changedSettings.HasFlag(ChangedSetting.GpaOption))
                    {
                        settings.GpaOption = account.GpaOption;
                    }
                    if (changedSettings.HasFlag(ChangedSetting.WeekOneStartsOn))
                    {
                        settings.WeekOneStartsOn = account.WeekOneStartsOn;
                    }
                    if (changedSettings.HasFlag(ChangedSetting.SelectedSemesterId) && account.CurrentSemesterId != Guid.Empty)
                    {
                        settings.SelectedSemesterId = account.CurrentSemesterId;
                    }
                    if (changedSettings.HasFlag(ChangedSetting.SchoolTimeZone) && account.SchoolTimeZone != null)
                    {
                        if (App.PowerPlannerApp.UsesIanaTimeZoneIds)
                        {
                            settings.SchoolTimeZone = account.SchoolTimeZone.Id;
                        }
                        else if (TimeZoneConverter.TZConvert.TryWindowsToIana(account.SchoolTimeZone.Id, out string iana))
                        {
                            settings.SchoolTimeZone = iana;
                        }
                    }
                    if (changedSettings.HasFlag(ChangedSetting.DefaultGradeScale))
                    {
                        settings.DefaultGradeScale = account.DefaultGradeScale;
                    }
                    if (changedSettings.HasFlag(ChangedSetting.DefaultDoesAverageGradeTotals))
                    {
                        settings.DefaultDoesAverageGradeTotals = account.DefaultDoesAverageGradeTotals;
                    }
                    if (changedSettings.HasFlag(ChangedSetting.DefaultDoesRoundGradesUp))
                    {
                        settings.DefaultDoesRoundGradesUp = account.DefaultDoesRoundGradesUp;
                    }

                    try
                    {
                        SyncSettingsRequest request = new SyncSettingsRequest()
                        {
                            Settings = settings
                        };

                        SyncSettingsResponse response = await account.PostAuthenticatedAsync<SyncSettingsRequest, SyncSettingsResponse>(Website.URL + "syncsettingsmodern", request);

                        if (response == null)
                            return;

                        if (response.Error != null)
                            return;

                        if (!base.IsAnotherQueued)
                        {
                            account.CurrentDefaultGradeScaleIndex = response.DefaultGradeScaleIndex;
                            account.NeedsToSyncSettings = false;
                            await AccountsManager.Save(account);
                        }

                        return;
                    }

                    catch (OperationCanceledException) { return; }

                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error syncing settings: " + ex.ToString());

                        // Ignore typical issues
                        if (!ExceptionHelper.IsHttpWebIssue(ex))
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                        return;
                    }
                }

                protected override Tuple<AccountDataItem, ChangedSetting> MergeData(Tuple<AccountDataItem, ChangedSetting> newData, Tuple<AccountDataItem, ChangedSetting> previouslyQueuedData)
                {
                    if (previouslyQueuedData == null)
                    {
                        return newData;
                    }

                    return new Tuple<AccountDataItem, ChangedSetting>(newData.Item1, newData.Item2 | previouslyQueuedData.Item2);
                }

                protected override void OnException(Exception ex, Tuple<AccountDataItem, ChangedSetting> data)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        public static async Task<bool> SetAsPremiumAccount(AccountDataItem account)
        {
            try
            {
                if (!account.IsOnlineAccount)
                {
                    return true;
                }

                AddPremiumAccountDurationResponse resp = await account.PostAuthenticatedAsync<AddPremiumAccountDurationRequest, AddPremiumAccountDurationResponse>(
                    Website.URL + "addpremiumaccountduration",
                    new AddPremiumAccountDurationRequest()
                    {
                        DaysToAdd = int.MaxValue
                    });

                if (resp.Error == null)
                {
                    await account.SetAsLifetimePremiumAsync();
                    return true;
                }
            }

            catch (Exception ex)
            when (ex is HttpRequestException
            || ex is OperationCanceledException // iOS's HttpClient seems to trigger this sometimes
            )
            {
                // Nothing, no need to log
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return false;
        }
    }
}
