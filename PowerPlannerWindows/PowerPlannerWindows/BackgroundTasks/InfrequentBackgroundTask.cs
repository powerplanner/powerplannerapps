using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.Extensions;
using PowerPlannerUWP.TileHelpers;
using System;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;

namespace PowerPlannerUWP.BackgroundTasks
{
    public class InfrequentBackgroundTask
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async void Handle(BackgroundActivatedEventArgs args)
        {
            var taskInstance = args.TaskInstance;

            taskInstance.Canceled += TaskInstance_Canceled;

            var deferral = taskInstance.GetDeferral();

            try
            {
                var accounts = await AccountsManager.GetAllAccounts();

                var cancellationToken = _cancellationTokenSource.Token;

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var a in accounts)
                    {
                        var data = await AccountDataStore.Get(a.LocalAccountId);

                        cancellationToken.ThrowIfCancellationRequested();

                        await ClassRemindersExtension.Current?.ResetAllRemindersAsync(a);
                        await RemindersExtension.Current?.ResetReminders(a, data);
                        await TileHelper.UpdateTileNotificationsForAccountAsync(a, data);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                catch (OperationCanceledException) { }
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
