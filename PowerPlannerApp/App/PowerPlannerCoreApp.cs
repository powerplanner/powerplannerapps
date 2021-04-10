using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Extensions;
using PowerPlannerApp.SyncLayer;
using PowerPlannerApp.ViewItems;
using PowerPlannerApp.ViewLists;
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

        /// <summary>
        /// Finds the next date that the class occurs on. If the class already started today, returns the
        /// NEXT instance of that class.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static DateTime? GetNextClassDate(AccountDataItem account, ViewItemClass c)
        {
            if (c.Schedules == null || c.Schedules.Count == 0)
            {
                return null;
            }

            var now = DateTime.Now;
            DateTime date = now.Date;

            // Look up to 2 weeks (and one day) in advance
            // We include the extra one day since if the class is currently going on, we want the NEXT instance of that class,
            // which could possibly be 2 weeks out
            for (int i = 0; i < 15; i++, date = date.AddDays(1))
            {
                var currWeek = account.GetWeekOnDifferentDate(date);

                // If there's a schedule on that day
                // (If it's not today, then we're good... otherwise if it's today,
                // make sure that the class hasn't started yet - if it started, we want the NEXT instance of the class)
                if (c.Schedules.Any(s =>
                    s.DayOfWeek == date.DayOfWeek
                    && (s.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks || s.ScheduleWeek == currWeek)
                    && (date.Date != now.Date || s.StartTime.TimeOfDay > now.TimeOfDay)))
                {
                    return date;
                }
            }

            return null;
        }

        public static ViewItemClass GetFirstClassOnDay(DateTime date, AccountDataItem account, IEnumerable<ViewItemClass> classes)
        {
            if (classes == null)
                return null;

            var currWeek = account.GetWeekOnDifferentDate(date);

            var schedules = SchedulesOnDay.Get(account, classes, date, currWeek, trackChanges: false);

            return schedules.FirstOrDefault()?.Class;
        }

        public static ViewItemClass GetClosestClassBasedOnSchedule(DateTime now, AccountDataItem account, IEnumerable<ViewItemClass> classes)
        {
            if (classes == null)
                return null;

            var currWeek = account.GetWeekOnDifferentDate(now);

            var schedules = SchedulesOnDay.Get(account, classes, now, currWeek, trackChanges: false);

            ViewItemSchedule closestBefore = null;
            ViewItemSchedule closestAfter = null;

            //look through all schedules
            foreach (var s in schedules)
            {
                //if the class is currently going on
                if (now.TimeOfDay >= s.StartTime.TimeOfDay && now.TimeOfDay <= s.EndTime.TimeOfDay)
                {
                    return s.Class;
                }

                //else if the class is in the future, we instantly select it for the after class since it's sorted from earliest to latest
                else if (s.StartTime.TimeOfDay >= now.TimeOfDay)
                {
                    // Make sure it's only 10 mins after
                    if ((s.StartTime.TimeOfDay - now.TimeOfDay) < TimeSpan.FromMinutes(10))
                    {
                        closestAfter = s;
                    }

                    // Regardless we break
                    break;
                }

                else
                {
                    // Make sure it's only 10 mins before
                    if ((now.TimeOfDay - s.EndTime.TimeOfDay) < TimeSpan.FromMinutes(10))
                    {
                        closestBefore = s;
                    }
                }
            }

            if (closestAfter == null && closestBefore == null)
            {
                return null;
            }

            else if (closestAfter == null)
                return closestBefore.Class;

            else if (closestBefore == null)
                return closestAfter.Class;

            else if ((now.TimeOfDay - closestBefore.EndTime.TimeOfDay) < (closestAfter.StartTime.TimeOfDay - now.TimeOfDay))
                return closestBefore.Class;

            else
                return closestAfter.Class;
        }
    }
}
