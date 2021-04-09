using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Extensions;
using PowerPlannerApp.SyncLayer;
using PowerPlannerApp.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerApp.App
{
    public static class PowerPlannerCoreApp
    {
        public static void Initialize()
        {
            SharedInitialization.Initialize();
            PortableDispatcher.ObtainDispatcherFunction = () => FormsPortableDispatcher.Current;
            PortableMessageDialog.Extension = ShowMessageDialogAsync;
        }

        public static async Task ShowMessageDialogAsync(PortableMessageDialog portableDialog)
        {
            await AppShell.Current.MainPage.DisplayAlert(portableDialog.Title, portableDialog.Content, "Ok");
        }

        /// <summary>
        /// Android should set this to true as TimeZoneInfo operates on IANA ids. Windows should keep this false;
        /// </summary>
        public static bool UsesIanaTimeZoneIds { get; set; }

        public static async Task SaveChanges(AccountDataItem account, DataChanges changes, bool waitForSaveAndSyncTasks = false)
        {
            if (account == null)
            {
                throw new NullReferenceException("account was null.");
            }

            var dataStore = await AccountDataStore.Get(account.LocalAccountId);
            var saveChangeTasks = await dataStore.ProcessLocalChanges(changes);
            System.Threading.Tasks.Task<SyncResult> syncTask = null;

            // Don't await this, we don't want it blocking
            if (account.IsOnlineAccount)
            {
                if (waitForSaveAndSyncTasks)
                {
                    syncTask = Sync.SyncAccountAsync(account);
                }
                else
                {
                    SyncWithoutBlocking(account);
                }
            }

            if (waitForSaveAndSyncTasks)
            {
                // Need to wait for the tile/toast tasks to finish
                await saveChangeTasks.WaitForAllTasksAsync();
                if (syncTask != null)
                {
                    var syncResult = await syncTask;
                    if (syncResult != null && syncResult.SaveChangesTask != null)
                    {
                        await syncResult.SaveChangesTask.WaitForAllTasksAsync();
                    }
                }
            }
        }

        private static async void SyncWithoutBlocking(AccountDataItem account)
        {
            try
            {
                await Sync.SyncAccountAsync(account);
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Returns true if the class takes place on the specified date.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool DoesClassOccurOnDate(AccountDataItem account, DateTime date, ViewItemClass c)
        {
            if (c.Schedules == null || c.Schedules.Count == 0 || account == null)
            {
                return false;
            }

            var currWeek = account.GetWeekOnDifferentDate(date);

            return c.Schedules.Any(s =>
                    s.DayOfWeek == date.DayOfWeek
                    && (s.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks || s.ScheduleWeek == currWeek));
        }
    }
}
