using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace PowerPlannerUWP.BackgroundTasks
{
    public class ToastBackgroundTask
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async void Handle(BackgroundActivatedEventArgs args)
        {
            var taskInstance = args.TaskInstance;

            taskInstance.Canceled += TaskInstance_Canceled;

            var deferral = taskInstance.GetDeferral();

            try
            {
                var toastActionTrigger = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;

                var toastArgs = ArgumentsHelper.Parse(toastActionTrigger.Argument);

                if (toastArgs is BaseArgumentsWithAccount toastArgsWithAccount)
                {
                    Guid localAccountId = toastArgsWithAccount.LocalAccountId;

                    if (localAccountId == Guid.Empty)
                        return;

                    AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);

                    if (account == null)
                        return;

                    var cancellationToken = _cancellationTokenSource.Token;

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (toastArgs is MarkTasksCompleteArguments markCompleteArgs)
                        {
                            TelemetryExtension.Current?.TrackEvent("ToastMarkedCompleted");

                            DataChanges changes = new DataChanges();

                            foreach (var id in markCompleteArgs.ItemIds)
                            {
                                changes.Add(new DataItemMegaItem()
                                {
                                    Identifier = id,
                                    PercentComplete = 1
                                }, onlyEdit: true);
                            }

                            // Need to wait for all tile/toast tasks to complete before returning and completing deferral
                            await PowerPlannerApp.Current.SaveChanges(account, changes, waitForSaveAndSyncTasks: true);
                        }
                    }

                    catch (OperationCanceledException) { }

                    // Wait for the calendar integration to complete
                    await AppointmentsExtension.Current?.GetTaskForAllCompleted();
                }
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
