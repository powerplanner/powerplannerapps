using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class RemindersExtension
    {
        /* ClearReminders only gets called when deleting an account, and clears both current and scheduled
         * ResetReminders gets called frequently and simply reschedules and updates current
         * ClearCurrentReminders gets called upon launch to clear only current ones
         * 
         * ClearReminders can be replaced by...
         *  - Technically nothing, but doesn't matter
         *  
         * ResetReminders can be replaced by...
         *  - ClearReminders
         *  - ResetReminders
         *  
         * ClearCurrentReminders can be replaced by...
         *  - ClearReminders
         *  - ClearCurrentReminders
         *  
         * Since ClearReminders is a niche case we're going to not worry about intelligently replacing
         * */

        public static RemindersExtension Current { get; set; }

        private MultipleChannelsWorkQueue _workQueue = new MultipleChannelsWorkQueue();
        private enum WorkTypes
        {
            ClearReminders,
            ResetReminders,
            ClearCurrentReminders
        }

        /// <summary>
        /// Platforms that need notification permission (iOS) can use this to prompt the user at the right time, instead of at app launch
        /// </summary>
        public virtual void RequestReminderPermission()
        {
            // Nothing, extending classes can implement
        }

        public async Task ClearReminders(Guid localAccountId)
        {
            try
            {
                await _workQueue.QueueOrMergeAsync(
                    channelIdentifier: localAccountId,
                    mergeIdentifier: WorkTypes.ClearReminders,
                    workerFunction: delegate
                    {
                        return ActuallyClearReminders(localAccountId);
                    });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected abstract Task ActuallyClearReminders(Guid localAccountId);

        public async Task ResetReminders(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                await _workQueue.QueueOrMergeAsync(
                    channelIdentifier: account.LocalAccountId,
                    mergeIdentifier: WorkTypes.ResetReminders,
                    workerFunction: delegate
                    {
                        return ActuallyResetReminders(account, data);
                    });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected abstract Task ActuallyResetReminders(AccountDataItem account, AccountDataStore data);

        protected virtual bool SupportsClearingCurrentReminders => false;
        public async Task ClearCurrentReminders(Guid localAccountId)
        {
            try
            {
                if (SupportsClearingCurrentReminders)
                {
                    await _workQueue.QueueOrMergeAsync(
                        channelIdentifier: localAccountId,
                        mergeIdentifier: WorkTypes.ClearCurrentReminders,
                        workerFunction: delegate
                        {
                            return ActuallyClearCurrentReminders(localAccountId);
                        });
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected virtual Task ActuallyClearCurrentReminders(Guid localAccountId) { throw new NotImplementedException(); }


        public static DateTime GetDayBeforeReminderTime(DateTime date, AccountDataItem account, AgendaViewItemsGroup agenda)
        {
            DateTime? classEndTime = account.GetClassEndTime(date, agenda.Classes);
            if (classEndTime == null)
                return date.Date.AddHours(15); // Default time is 3:00 PM

            return classEndTime.Value.AddMinutes(10); // Otherwise 10 mins after the class end time
        }

        /// <summary>
        /// Returns something like "Due 11:00 AM" or "Due today" or just "11:00 AM" if event
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetDueTimeAsString(BaseViewItemHomeworkExam item)
        {
            string answer = "";
            
            if (item is ViewItemHomework)
            {
                answer += "Due ";
            }

            switch (item.GetActualTimeOption())
            {
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.BeforeClass:
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.Custom:
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.StartOfClass:
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.EndOfClass:
                    answer += item.GetDueDateWithTime().ToString("t");
                    break;

                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.DuringClass:
                    answer += "during class";
                    break;

                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.AllDay:
                default:
                    answer += "today";
                    break;
            }

            if (item.TryGetEndDateWithTime(out DateTime endTime))
            {
                answer += " to " + endTime.ToString("t");
            }

            return answer.Substring(0, 1).ToUpper() + answer.Substring(1);
        }
    }
}
