using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using ToolsPortable;
using UserNotifications;

namespace PowerPlanneriOS.Extensions
{
    public class IOSClassRemindersExtension : ClassRemindersExtension
    {
        public const string CLASS_REMINDER_PREFIX = "CLASS_REMINDER:";
        private const string DATE_FORMAT = "yyyyMMdd";
        private const int DAYS_IN_ADVANCE = 7;

        private UNUserNotificationCenter _notificationCenter;

        public IOSClassRemindersExtension()
        {
            try
            {
                _notificationCenter = UNUserNotificationCenter.Current;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override async Task ResetAllReminders(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup)
        {
            try
            {
                await Task.Run(async delegate
                {
                    // Remove all existing class reminder notifications for this account
                    await RemoveAllClassReminderNotificationsAsync(account.LocalAccountId);

                    // If no schedule or reminders disabled, we're done
                    if (scheduleViewItemsGroup == null || !account.AreClassRemindersEnabled())
                    {
                        return;
                    }

                    DateTime today = DateTime.Today;
                    DateTime endDate = today.AddDays(DAYS_IN_ADVANCE);

                    // Load holidays so we can skip scheduling reminders on holiday days
                    var holidayHelper = await SchedulesOnDayExcludingHolidays.LoadAsync(
                        account.LocalAccountId, 
                        scheduleViewItemsGroup.Semester, 
                        today, 
                        endDate);

                    var beforeTime = account.ClassRemindersTimeSpan.GetValueOrDefault();

                    // Schedule notifications for each class over the next week
                    for (DateTime date = today; date < endDate; date = date.AddDays(1))
                    {
                        var schedulesOnDay = holidayHelper.Get(
                            account, 
                            scheduleViewItemsGroup.Classes, 
                            date, 
                            account.GetWeekOnDifferentDate(date));

                        foreach (var schedule in schedulesOnDay)
                        {
                            var reminderTime = schedule.StartTimeInLocalTime(date).Subtract(beforeTime);

                            // Only schedule if at least 5 seconds in the future
                            if (reminderTime >= DateTime.Now.AddSeconds(5))
                            {
                                await ScheduleClassReminderNotification(
                                    account.LocalAccountId,
                                    schedule,
                                    date,
                                    reminderTime);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task ScheduleClassReminderNotification(
            Guid localAccountId, 
            ViewItemSchedule schedule, 
            DateTime date, 
            DateTime reminderTime)
        {
            try
            {
                var content = new UNMutableNotificationContent
                {
                    Title = schedule.Class.Name
                };

                // Format time range
                string timeRange = string.Format(
                    PowerPlannerResources.GetString("String_TimeToTime"),
                    schedule.StartTimeInLocalTime(date).ToString("t"),
                    schedule.EndTimeInLocalTime(date).ToString("t"));

                // Add room if available
                if (!string.IsNullOrWhiteSpace(schedule.Room))
                {
                    content.Subtitle = timeRange;
                    content.Body = schedule.Room;
                }
                else
                {
                    content.Subtitle = timeRange;
                }

                // Calculate trigger time
                double secondsUntilReminder = (reminderTime - DateTime.Now).TotalSeconds;
                if (secondsUntilReminder < 5)
                {
                    return;
                }

                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(secondsUntilReminder, false);

                // Create unique identifier for this notification
                string identifier = GetIdentifierForClassReminder(localAccountId, schedule, date);

                var request = UNNotificationRequest.FromIdentifier(identifier, content, trigger);

                await _notificationCenter.AddNotificationRequestAsync(request);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static string GetIdentifierStartStringForAccount(Guid localAccountId)
        {
            return IOSRemindersExtension.ACCOUNT + localAccountId + ":" + CLASS_REMINDER_PREFIX;
        }

        private static string GetIdentifierForClassReminder(Guid localAccountId, ViewItemSchedule schedule, DateTime date)
        {
            return GetIdentifierStartStringForAccount(localAccountId) + 
                   schedule.Identifier + ":" + 
                   date.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public static bool TryParseClassReminderIdentifier(string identifier, out Guid scheduleId)
        {
            int index = identifier.IndexOf(CLASS_REMINDER_PREFIX);
            if (index != -1)
            {
                string remaining = identifier.Substring(index + CLASS_REMINDER_PREFIX.Length);
                int colonIndex = remaining.IndexOf(':');
                if (colonIndex != -1)
                {
                    string scheduleIdString = remaining.Substring(0, colonIndex);
                    return Guid.TryParse(scheduleIdString, out scheduleId);
                }
            }

            scheduleId = Guid.Empty;
            return false;
        }

        private async Task<string[]> GetClassReminderNotificationIdentifiersAsync(Guid localAccountId)
        {
            try
            {
                var notifs = await _notificationCenter.GetPendingNotificationRequestsAsync();
                if (notifs == null || notifs.Length == 0)
                {
                    return new string[0];
                }

                List<string> ids = new List<string>();
                string idStartString = GetIdentifierStartStringForAccount(localAccountId);

                foreach (var notif in notifs)
                {
                    if (notif.Identifier.StartsWith(idStartString))
                    {
                        ids.Add(notif.Identifier);
                    }
                }

                return ids.ToArray();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return new string[0];
            }
        }

        private async Task RemoveAllClassReminderNotificationsAsync(Guid localAccountId)
        {
            try
            {
                string[] idsToRemove = await GetClassReminderNotificationIdentifiersAsync(localAccountId);
                if (idsToRemove.Length > 0)
                {
                    _notificationCenter.RemovePendingNotificationRequests(idsToRemove);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
