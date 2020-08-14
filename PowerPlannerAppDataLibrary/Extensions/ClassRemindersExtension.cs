using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class ClassRemindersExtension
    {
        public static ClassRemindersExtension Current { get; set; }

        private SimpleAsyncWorkerQueue _workQueue = new SimpleAsyncWorkerQueue();
        public Task ResetAllRemindersAsync(AccountDataItem account)
        {
            // If they weren't enabled, we won't do anything. The only time they get disabled is from settings, and we'll clear when that happens.
            if (!account.AreClassRemindersEnabled())
            {
                return Task.CompletedTask;
            }

            return _workQueue.QueueOrMergeAsync(account.LocalAccountId, () => Task.Run(async delegate 
            {
                try
                {
                    ScheduleViewItemsGroup scheduleViewItemsGroup = null;

                    var currSemesterId = account.CurrentSemesterId;
                    if (currSemesterId != Guid.Empty && account.AreClassRemindersEnabled())
                    {
                        // We don't need to worry about holding a lock though, if the collections change and that breaks us, that's fine,
                        // that implies that the data has been changed anyways and another ResetReminders will come in.
                        // Therefore we should also expect to get some exceptions here.
                        scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, currSemesterId, trackChanges: false, includeWeightCategories: false);
                    }

                    await ResetAllRemindersAsync(account, scheduleViewItemsGroup);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }));
        }

        /// <summary>
        /// Reset the class reminders. scheduleViewItemsGroup may be null (indicating no semester or reminders disabled).
        /// </summary>
        /// <param name="account"></param>
        /// <param name="scheduleViewItemsGroup"></param>
        protected abstract Task ResetAllRemindersAsync(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup);

        /// <summary>
        /// This is only used when user disables reminders in the settings.
        /// </summary>
        public Task RemoveAllRemindersAsync(AccountDataItem account)
        {
            return ResetAllRemindersAsync(account, null);
        }
    }
}
