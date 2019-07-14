using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerUWPLibrary;
using PowerPlannerUWPLibrary.Extensions;
using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;

namespace BackgroundTasksProject
{
    public sealed class RawPushBackgroundTask : IBackgroundTask
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            InitializeUWP.Initialize();

            taskInstance.Canceled += TaskInstance_Canceled;

            var deferral = taskInstance.GetDeferral();


            try
            {
                RawNotification notification = (RawNotification)taskInstance.TriggerDetails;

                long accountId = UWPPushExtension.GetAccountIdFromRawNotification(notification);

                if (accountId == 0)
                    return;

                AccountDataItem account = (await AccountsManager.GetAllAccounts()).FirstOrDefault(i => i.AccountId == accountId);

                if (account == null)
                    return;

                var cancellationToken = _cancellationTokenSource.Token;

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = await Sync.SyncAccountAsync(account);

                    // If succeeded
                    if (result != null && result.Error == null)
                    {
                        // Flag as updated by background task so foreground app can update data
                        AccountDataStore.SetUpdatedByBackgroundTask();
                    }

                    // Need to wait for the tile/toast tasks to finish before we release the deferral
                    if (result != null && result.SaveChangesTask != null)
                    {
                        await result.SaveChangesTask.WaitForAllTasksAsync();
                    }
                }

                catch (OperationCanceledException) { }

                // Wait for the calendar integration to complete
                await AppointmentsExtension.Current?.GetTaskForAllCompleted();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
