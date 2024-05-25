using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAndroid.Receivers;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidClassRemindersExtension : ClassRemindersExtension
    {
        protected override async Task ResetAllReminders(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup)
        {
            var notifManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

            await AndroidRemindersExtension.InitializeChannelsAsync(notifManager);

            // There should only be one class reminder visible at a time (no duplicate classes). 

            /*
             * There should only be one reminder visible at a time (no classes at the same time).
             * 
             * Reminder should disappear at the end of class (or if back-to-back classes, next reminder simply replaces).
             * 
             * We'll always show a popup notification, no concept of silent updating for these.
             * 
             * So essentially, we schedule a task to run at the time the reminder is supposed to appear (or disappear), when that code is triggered we see what class we're currently in (or currently should have a notification for), and show (or remove) notification, and then we find out what time we next should remove or show a notification and schedule another task.
             * 
             * Code for resetting should clear a notification if the class/schedule was deleted/moved to a different time, or update if renamed, but it should NOT show a new notification. Then, it should schedule a task to run at the time the next reminder is supposed to appear (or when the current reminder is supposed to disappear).
             * 
             * Scheduled task code, when ran, should either show a new notification, or remove current notification, and then schedule the next task again
             * 
             * */

            // If no classes or no semester, ensure we've cleared notification and then we're done
            if (scheduleViewItemsGroup == null)
            {
                notifManager.Cancel(NotificationIds.CLASS_REMINDER_NOTIFICATION);
                return;
            }

            // Give ourselves 5 second buffer
            DateTime now = DateTime.Now.AddSeconds(5);

            var currSchedule = FindCurrentReminder(account, scheduleViewItemsGroup, now, out DateTime dateOfClass);
            if (currSchedule == null)
            {
                // Remove any existing
                notifManager.Cancel(NotificationIds.CLASS_REMINDER_NOTIFICATION);
            }
            else
            {
                var existing = notifManager.GetActiveNotifications().FirstOrDefault(i => i.Id == NotificationIds.CLASS_REMINDER_NOTIFICATION);
                if (existing != null)
                {
                    // Update silently
                    ShowNotification(notifManager, Application.Context, account.LocalAccountId, currSchedule, dateOfClass, silentUpdate: true);
                }
            }

            ScheduleNext(account, scheduleViewItemsGroup, now, currSchedule);
        }

        private static void ScheduleNext(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup, DateTime now, ViewItemSchedule currSchedule)
        {
            var startTimeToLookFrom = now;
            if (currSchedule != null && currSchedule.StartTimeInLocalTime(now) > now)
            {
                // Handling case where updating from the pre-class reminder, so we don't then pick up that class again as the "next" class
                startTimeToLookFrom = currSchedule.StartTimeInLocalTime(now).AddSeconds(1);
            }

            var nextSchedule = FindNextSchedule(account, scheduleViewItemsGroup, startTimeToLookFrom, now.Date.AddDays(15)); // 15 days should cover week a/b

            DateTime? nextUpdateTime = null;

            if (currSchedule != null)
            {
                // Reminder clears at end of the class
                nextUpdateTime = currSchedule.EndTimeInLocalTime(now);
            }

            if (nextSchedule != null)
            {
                var reminderTime = nextSchedule.Item1.Add(nextSchedule.Item2.StartTimeInLocalTime(now).TimeOfDay).Subtract(account.ClassRemindersTimeSpan.GetValueOrDefault());
                if (nextUpdateTime == null || reminderTime < nextUpdateTime.Value)
                {
                    nextUpdateTime = reminderTime;
                }
            }

            if (nextUpdateTime != null && nextUpdateTime.Value > DateTime.Now)
            {
                AlarmManagerHelper.Schedule(
                    context: Application.Context,
                    receiverType: typeof(UpdateClassReminderReceiver),
                    wakeTime: nextUpdateTime.Value.AddSeconds(1), // Delay by 1 second just to give a bit of buffer
                    uriData: "powerplanner:?localAccountId=" + account.LocalAccountId,
                    wakeDevice: true);
            }
        }

        public static async Task OnBackgroundTaskExecutingAsync(Context context, Guid localAccountId)
        {
            var notifManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);
            if (account == null || !account.AreClassRemindersEnabled())
            {
                CancelNotification(notifManager);
                return;
            }

            if (account.CurrentSemesterId == Guid.Empty)
            {
                CancelNotification(notifManager);
                return;
            }

            ScheduleViewItemsGroup scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(localAccountId, account.CurrentSemesterId, trackChanges: false, includeWeightCategories: false);
            if (scheduleViewItemsGroup == null)
            {
                CancelNotification(notifManager);
                return;
            }

            DateTime now = DateTime.Now;
            var currSchedule = FindCurrentReminder(account, scheduleViewItemsGroup, now, out DateTime dateOfClass);

            if (currSchedule == null)
            {
                CancelNotification(notifManager);
            }
            else
            {
                // Show the new notification
                ShowNotification(notifManager, context, localAccountId, currSchedule, dateOfClass);
            }

            // Schedule the next change
            ScheduleNext(account, scheduleViewItemsGroup, now, currSchedule);
        }

        private static void ShowNotification(NotificationManager notifManager, Context context, Guid localAccountId, ViewItemSchedule currSchedule, DateTime date, bool silentUpdate = false)
        {
            // Show the new notification
            var builder = AndroidRemindersExtension.CreateReminderBuilder(context, localAccountId, new ViewClassArguments()
            {
                LocalAccountId = localAccountId,
                ItemId = currSchedule.Class.Identifier,
                LaunchSurface = LaunchSurface.Toast
            }, AndroidRemindersExtension.GetChannelIdForClassReminders(localAccountId));

            builder.SetShowWhen(false);
            builder.SetContentTitle(currSchedule.Class.Name);

            string summaryText = string.Format(PowerPlannerResources.GetString("String_TimeToTime"), currSchedule.StartTimeInLocalTime(date).ToString("t"), currSchedule.EndTimeInLocalTime(date).ToString("t"));
            if (!string.IsNullOrWhiteSpace(currSchedule.Room))
            {
                builder.SetContentText(summaryText + " - " + currSchedule.Room);
                builder.SetStyle(new NotificationCompat.BigTextStyle()
                    .BigText(summaryText + "\n" + currSchedule.Room));
            }
            else
            {
                builder.SetContentText(summaryText);
            }

            if (silentUpdate)
            {
                builder.SetSilent(true);
            }

            notifManager.Notify(NotificationIds.CLASS_REMINDER_NOTIFICATION, builder.Build());
        }

        private static ViewItemSchedule FindCurrentReminder(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup, DateTime now, out DateTime dateOfClass)
        {
            dateOfClass = now.Date;
            var beforeTime = account.ClassRemindersTimeSpan.GetValueOrDefault();

            SchedulesOnDay schedulesToday = SchedulesOnDay.Get(account, scheduleViewItemsGroup.Classes, dateOfClass, account.GetWeekOnDifferentDate(dateOfClass), trackChanges: false);

            var currSchedule = schedulesToday.OrderByDescending(i => i.StartTimeInLocalTime(now)).FirstOrDefault(i => now >= i.StartTimeInLocalTime(now).Subtract(beforeTime) && now <= i.EndTimeInLocalTime(now));
            if (currSchedule != null)
            {
                dateOfClass = now.Date;
                return currSchedule;
            }

            // Otherwise, might be a schedule tomorrow at like 1am
            dateOfClass = now.Date.AddDays(1);
            SchedulesOnDay schedulesTomorrow = SchedulesOnDay.Get(account, scheduleViewItemsGroup.Classes, dateOfClass, account.GetWeekOnDifferentDate(dateOfClass), trackChanges: false);


            currSchedule = schedulesTomorrow.FirstOrDefault(i => now >= i.StartTimeInLocalTime(now).Subtract(beforeTime) && now <= i.EndTimeInLocalTime(now));
            return currSchedule;
        }

        private static void CancelNotification(NotificationManager notificationManager)
        {
            notificationManager.Cancel(NotificationIds.CLASS_REMINDER_NOTIFICATION);
        }

        private static Tuple<DateTime, ViewItemSchedule> FindNextSchedule(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup, DateTime now, DateTime maxDate)
        {
            var schedulesOnDay = SchedulesOnDay.Get(account, scheduleViewItemsGroup.Classes, now.Date, account.GetWeekOnDifferentDate(now.Date), trackChanges: false);

            ViewItemSchedule nextSchedule = schedulesOnDay.FirstOrDefault(i => i.StartTimeInLocalTime(now) > now);

            if (nextSchedule != null)
            {
                return new Tuple<DateTime, ViewItemSchedule>(now.Date, nextSchedule);
            }

            now = now.Date.AddDays(1);
            if (now >= maxDate)
            {
                return null;
            }

            return FindNextSchedule(account, scheduleViewItemsGroup, now, maxDate);
        }

        private static void ScheduleNextAlarm(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup)
        {
            DateTime now = DateTime.Now.AddSeconds(5);

            SchedulesOnDay schedulesToday = SchedulesOnDay.Get(account, scheduleViewItemsGroup.Classes, now.Date, account.GetWeekOnDifferentDate(now.Date), trackChanges: false);
        }
    }
}