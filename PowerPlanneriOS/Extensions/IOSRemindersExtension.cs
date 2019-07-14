using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Threading.Tasks;
using UserNotifications;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using ToolsPortable;
using System.Globalization;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlanneriOS.Extensions
{
    public class IOSRemindersExtension : RemindersExtension
    {
        public const string ACCOUNT = "Account:";
        public const string DAY_BEFORE = "DayBefore:";
        public const string DAY_OF_TASK = "DayOfTask:";
        public const string DAY_OF_EVENT = "DayOfEvent:";
        public const string DAY_BEFORE_DATE_FORMAT = "yyyyMMdd";

        private const int DAYS_IN_ADVANCE = 7;
        private UNUserNotificationCenter _notificationCenter;

        public IOSRemindersExtension()
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

        protected override async Task ActuallyClearReminders(Guid localAccountId)
        {
            // This gets called upon account being deleted
            try
            {
                await Task.Run(async delegate
                {
                    await RemoveAllNotificationsAsync(localAccountId);
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override bool SupportsClearingCurrentReminders => true;
        protected override async Task ActuallyClearCurrentReminders(Guid localAccountId)
        {
            // This gets called upon opening/returning to app/logging in
            try
            {
                await Task.Run(async delegate
                {
                    await RemoveAllCurrentNotificationsAsync(localAccountId);
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override async Task ActuallyResetReminders(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                // If reminders disabled, do nothing
                if (!account.RemindersDayBefore && !account.RemindersDayOf)
                {
                    return;
                }

                await Task.Run(async delegate
                {
                    // This gets called whenever changes are made in the account's data.
                    AgendaViewItemsGroup agendaItems = await GetAgendaViewItemsGroup(account, DateTime.Now);

                    // If no current semester, or no items, just clear all
                    if (agendaItems == null || agendaItems.Items.Count == 0)
                    {
                        await RemoveAllNotificationsAsync(account.LocalAccountId);
                        return;
                    }

                    DateTime now = DateTime.Now;

                    await UpdateScheduledNotificationsAsync(account, agendaItems, now);
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task UpdateScheduledNotificationsAsync(AccountDataItem account, AgendaViewItemsGroup agendaItems, DateTime now)
        {
            // Just remove all, we would be overwriting them anyways
            await RemoveAllScheduledNotificationsAsync(account.LocalAccountId);

            await AddScheduledDayBeforeNotifications(account, agendaItems, now);
            await AddScheduledDayOfNotifications(account, agendaItems, now);
        }

        private async Task AddScheduledDayBeforeNotifications(AccountDataItem account, AgendaViewItemsGroup agendaItems, DateTime now)
        {
            try
            {
                string idStartString = GetIdentifierStartStringForDayBefore(account.LocalAccountId);

                DateTime artificialToday = now.Date;
                for (int i = 0; i < DAYS_IN_ADVANCE; i++, artificialToday = artificialToday.AddDays(1))
                {
                    DateTime artificialTomorrow = artificialToday.AddDays(1);
                    BaseViewItemHomeworkExam[] itemsOnDay = agendaItems.Items.Where(x => x.Date.Date == artificialTomorrow).OrderBy(x => x).ToArray();

                    if (itemsOnDay.Length > 0)
                    {
                        DateTime reminderTime = RemindersExtension.GetDayBeforeReminderTime(artificialToday, account, agendaItems);

                        string title = "Due tomorrow";
                        string body = "";
                        if (itemsOnDay.Length == 1)
                        {
                            body = StringTools.TrimLengthWithEllipses(itemsOnDay[0].Name, 200);
                        }
                        else
                        {
                            title += " (" + itemsOnDay.Length + ")";

                            foreach (var item in itemsOnDay.Take(5))
                            {
                                body += StringTools.TrimLengthWithEllipses(item.Name, 30) + "\n";
                            }
                            body = body.Trim();
                        }
                        await ScheduleDayBeforeNotification(idStartString + artificialToday.ToString(DAY_BEFORE_DATE_FORMAT, CultureInfo.InvariantCulture), title, body, reminderTime);
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task AddScheduledDayOfNotifications(AccountDataItem account, AgendaViewItemsGroup agendaItems, DateTime now)
        {
            try
            {
                DateTime today = now.Date;
                DateTime maxDate = today.AddDays(DAYS_IN_ADVANCE);

                BaseViewItemHomeworkExam[] itemsDueTodayOrGreater = agendaItems.Items.Where(i => i.Date.Date >= today && i.Date.Date <= maxDate).OrderBy(i => i).ToArray();

                foreach (var item in itemsDueTodayOrGreater)
                {
                    bool hasClassTime = false;

                    DateTime reminderTime = GetDayOfReminderTime(item, out hasClassTime);

                    if (!IsTimeOkay(reminderTime))
                        continue;

                    string title = StringTools.TrimLengthWithEllipses(item.Name, 150);

                    string subtitle = GetDueTimeAsString(item);

                    string body = GetClassName(item);
                    if (!string.IsNullOrWhiteSpace(item.Details))
                    {
                        body += " - " + StringTools.TrimLengthWithEllipses(item.Details.Trim(), 200);
                    }

                    await ScheduleDayOfNotification(account.LocalAccountId, item, title, subtitle, body, reminderTime);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static string GetClassName(BaseViewItemHomeworkExam item)
        {
            if (item == null)
                return "";

            var c = item.GetClassOrNull();

            if (c != null)
                return StringTools.TrimLengthWithEllipses(c.Name, 30);

            return "";
        }

        /// <summary>
        /// Returns true if the time is at least 5 seconds in the future
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static bool IsTimeOkay(DateTime time)
        {
            return time >= DateTime.Now.AddSeconds(5);
        }

        private static DateTime GetDayOfReminderTime(BaseViewItemHomeworkExam item, out bool hadSpecificTime)
        {
            return item.GetDayOfReminderTime(out hadSpecificTime);
        }

        private async Task<string[]> GetCurrentNotificationIdentifiersAsync(Guid localAccountId)
        {
            List<string> ids = new List<string>();
            string idStartString = GetIdentifierStartStringForAccount(localAccountId);

            foreach (var notif in await _notificationCenter.GetDeliveredNotificationsAsync())
            {
                if (notif.Request.Identifier.StartsWith(idStartString))
                {
                    ids.Add(notif.Request.Identifier);
                }
            }

            return ids.ToArray();
        }

        private async Task<string[]> GetScheduledNotificationIdentifiersAsync(Guid localAccountId)
        {
            List<string> ids = new List<string>();
            string idStartString = GetIdentifierStartStringForAccount(localAccountId);

            // This seems to randomly crash the app when running in the simulator and called multiple times, but works
            // perfectly fine on my device. So have to assume it's an issue with the simulator.
            foreach (var notif in await _notificationCenter.GetPendingNotificationRequestsAsync())
            {
                if (notif.Identifier.StartsWith(idStartString))
                {
                    ids.Add(notif.Identifier);
                }
            }

            return ids.ToArray();
        }

        /// <summary>
        /// Removes both scheduled and delivered
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private async Task RemoveAllNotificationsAsync(Guid localAccountId)
        {
            // Remove scheduled first cause otherwise if we clear current first, a scheduled could get delivered afterwards
            await RemoveAllScheduledNotificationsAsync(localAccountId);
            await RemoveAllCurrentNotificationsAsync(localAccountId);
        }

        private async Task RemoveAllScheduledNotificationsAsync(Guid localAccountId)
        {
            string[] idsToRemove = await GetScheduledNotificationIdentifiersAsync(localAccountId);
            if (idsToRemove.Length > 0)
            {
                _notificationCenter.RemovePendingNotificationRequests(idsToRemove);
            }
        }

        private async Task RemoveAllCurrentNotificationsAsync(Guid localAccountId)
        {
            string[] idsToRemove = await GetCurrentNotificationIdentifiersAsync(localAccountId);
            if (idsToRemove.Length > 0)
            {
                _notificationCenter.RemoveDeliveredNotifications(idsToRemove);
            }
        }

        private static string GetIdentifierStartStringForAccount(Guid localAccountId)
        {
            return ACCOUNT + localAccountId + ":";
        }

        private static string GetIdentifierStartStringForDayBefore(Guid localAccountId)
        {
            return GetIdentifierStartStringForAccount(localAccountId) + DAY_BEFORE;
        }

        private static string GetIdentifierForDayOfItem(Guid localAccountId, BaseViewItemHomeworkExam item)
        {
            return GetIdentifierStartStringForAccount(localAccountId) + ((item is ViewItemHomework) ? DAY_OF_TASK : DAY_OF_EVENT) + item.Identifier;
        }

        public static bool TryParseAccount(string identifier, out Guid localAccountId)
        {
            if (identifier.StartsWith(ACCOUNT))
            {
                int startIndex = ACCOUNT.Length;
                int endIndex = identifier.IndexOf(':', startIndex);
                if (endIndex != -1)
                {
                    return Guid.TryParse(identifier.Substring(startIndex, endIndex - startIndex), out localAccountId);
                }
            }

            localAccountId = Guid.Empty;
            return false;
        }

        public static bool TryParseDayBeforeIdentifier(string identifier, out DateTime date)
        {
            int index = identifier.IndexOf(DAY_BEFORE);
            if (index != -1)
            {
                string dateString = identifier.Substring(index + DAY_BEFORE.Length);
                if (DateTime.TryParseExact(dateString, DAY_BEFORE_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime dateResult))
                {
                    date = dateResult.ToLocalTime();
                    return true;
                }
            }

            date = DateTime.MinValue;
            return false;
        }

        public static bool TryParsingDayOfTaskIdentifier(string identifier, out Guid itemIdentifier)
        {
            return TryParsingDayOfIdentifier(identifier, DAY_OF_TASK, out itemIdentifier);
        }

        public static bool TryParsingDayOfEventIdentifier(string identifier, out Guid itemIdentifier)
        {
            return TryParsingDayOfIdentifier(identifier, DAY_OF_EVENT, out itemIdentifier);
        }

        private static bool TryParsingDayOfIdentifier(string identifier, string type, out Guid itemIdentifier)
        {
            int index = identifier.IndexOf(type);
            if (index != -1)
            {
                string identifierString = identifier.Substring(index + type.Length);
                return Guid.TryParse(identifierString, out itemIdentifier);
            }

            itemIdentifier = Guid.Empty;
            return false;
        }

        private static async Task<AgendaViewItemsGroup> GetAgendaViewItemsGroup(AccountDataItem account, DateTime today)
        {
            try
            {
                // We're getting cached versions which might change as we're using the app, but that's fine, since if items change, another
                // ResetReminders will be incoming anyways
                var scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);
                return await AgendaViewItemsGroup.LoadAsync(account.LocalAccountId, scheduleViewItemsGroup.Semester, today, trackChanges: true);
            }

            // If semester didn't exist, it throws null reference exception
            catch { return null; }
        }

        private async Task ScheduleDayOfNotification(Guid localAccountId, BaseViewItemHomeworkExam item, string title, string subtitle, string body, DateTime deliveryTime)
        {
            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Subtitle = subtitle
            };

            if (!string.IsNullOrWhiteSpace(body))
            {
                content.Body = body;
            }

            await ScheduleNotification(GetIdentifierForDayOfItem(localAccountId, item), content, deliveryTime);
        }

        private async Task ScheduleDayBeforeNotification(string id, string title, string body, DateTime deliveryTime)
        {
            await ScheduleNotification(id, new UNMutableNotificationContent()
            {
                Title = title,
                Body = body
            }, deliveryTime);
        }

        private async Task ScheduleNotification(string id, UNMutableNotificationContent content, DateTime deliveryTime)
        {
            try
            {
                // Don't schedule unless 5 secs in future, otherwise possibly throws assertion error
                double seconds = (deliveryTime - DateTime.Now).TotalSeconds;
                if (seconds < 5)
                {
                    return;
                }

                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(seconds, false);

                // This code doesn't seem to work
                //var components = new NSDateComponents();
                //components.Second += 5;
                //var trigger = UserNotifications.UNCalendarNotificationTrigger.CreateTrigger(components, false);

                var request = UNNotificationRequest.FromIdentifier(id, content, trigger);

                await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}