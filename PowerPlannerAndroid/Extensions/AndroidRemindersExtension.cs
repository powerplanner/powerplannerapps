using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using System.Threading;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.Helpers;
using Android.Service.Notification;
using PowerPlannerAndroid.Services;
using PowerPlannerAndroid.Receivers;
using Android.Graphics;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary;
using AndroidX.Core.App;

namespace PowerPlannerAndroid.Extensions
{
    public class AndroidRemindersExtension : RemindersExtension
    {
        protected override async Task ActuallyClearReminders(Guid localAccountId)
        {
            // This occurs when an account is deleted

            try
            {
                await Task.Run(async delegate
                {
                    var notifManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

                    await InitializeChannelsAsync(notifManager);

                    // Remove the day before
                    notifManager.Cancel(localAccountId.ToString(), NotificationIds.DAY_BEFORE_NOTIFICATION);

                    // Remove the day of notifications
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void clearReminders(Guid localAccountId, CancellationToken token, AlarmManager alarmManager, Context context)
        {
            // For cancelling a pending alarm, the action, data, type, class, and categories of the intent need to be the same.
            
        }

        protected override async Task ActuallyResetReminders(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                await Task.Run(async delegate
                {
                    await InitializeChannelsAsync((NotificationManager)Application.Context.GetSystemService(Context.NotificationService));

                    Context context = Application.Context;

                    await UpdateAndScheduleNotifications(context, account, fromForeground: true);
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Returns true if the time is at least 10 seconds in the future
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static bool isTimeOkay(DateTime time)
        {
            return time >= DateTime.Now.AddSeconds(10);
        }

        public static async Task UpdateAndScheduleDayBeforeNotification(Context context, Guid localAccountId, bool fromForeground)
        {
            await InitializeChannelsAsync((NotificationManager)Application.Context.GetSystemService(Context.NotificationService));

            AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);
            if (account == null)
            {
                // If the account has been deleted, we'll do nothing
                return;
            }

            await UpdateAndScheduleDayBeforeNotification(context, account, fromForeground);
        }

        public static async Task UpdateAndScheduleNotifications(Context context, AccountDataItem account, bool fromForeground)
        {
            DateTime now = DateTime.Now;
            var agendaItems = await AndroidRemindersExtension.GetAgendaViewItemsGroup(account, now.Date);
            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            await InitializeChannelsAsync(notificationManager);

            await UpdateAndScheduleDayBeforeNotification(context, account, agendaItems, notificationManager, now, fromForeground);
            await UpdateAndScheduleDayOfNotifications(account, agendaItems, context, notificationManager, now, fromForeground);
        }

        private static async Task UpdateAndScheduleDayBeforeNotification(Context context, AccountDataItem account, bool fromForeground)
        {
            DateTime now = DateTime.Now;
            var agendaItems = await AndroidRemindersExtension.GetAgendaViewItemsGroup(account, now.Date);
            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            await UpdateAndScheduleDayBeforeNotification(context, account, agendaItems, notificationManager, now, fromForeground);
        }

        private static async Task UpdateAndScheduleDayBeforeNotification(Context context, AccountDataItem account, AgendaViewItemsGroup agendaItems, NotificationManager notificationManager, DateTime now, bool fromForeground)
        {
            // Update the day before notification
            await UpdateDayBeforeNotification(account, agendaItems, context, notificationManager, fromForeground);

            // And then schedule the next alarm
            ScheduleDayBeforeAlarm(context, account, now.Date, agendaItems);
        }

        public static async Task UpdateAndScheduleDayOfNotifications(Context context, Guid localAccountId, bool fromForeground)
        {
            await InitializeChannelsAsync((NotificationManager)Application.Context.GetSystemService(Context.NotificationService));

            AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);
            if (account != null)
            {
                await UpdateAndScheduleDayOfNotifications(context, account, fromForeground);
            }
        }

        private static async Task UpdateAndScheduleDayOfNotifications(Context context, AccountDataItem account, bool fromForeground)
        {
            DateTime now = DateTime.Now;
            var agendaItems = await GetAgendaViewItemsGroup(account, now.Date);
            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            await UpdateAndScheduleDayOfNotifications(account, agendaItems, context, notificationManager, now, fromForeground);
        }

        private static async Task UpdateAndScheduleDayOfNotifications(AccountDataItem account, AgendaViewItemsGroup agendaItems, Context context, NotificationManager notificationManager, DateTime now, bool fromForeground)
        {
            string tagForAccountStartsWith = account.LocalAccountId.ToString();
            List<StatusBarNotification> existingNotifs;
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                existingNotifs = notificationManager.GetActiveNotifications().Where(i => i.Id == NotificationIds.DAY_OF_NOTIFICATIONS && i.Tag != null && i.Tag.StartsWith(tagForAccountStartsWith)).ToList();
            }
            else
            {
                // GetActiveNotifications didn't exist before version 23
                existingNotifs = new List<StatusBarNotification>();
            }

            // If no current semester, or no items, then just clear
            if (agendaItems == null || agendaItems.Items.Count == 0)
            {
                foreach (var notif in existingNotifs)
                {
                    notificationManager.Cancel(notif.Tag, NotificationIds.DAY_OF_NOTIFICATIONS);
                }
                return;
            }

            DateTime nextReminderTime;

            var itemsThatCouldHaveNotifs = GetItemsThatCouldHaveDayOfNotifications(account, agendaItems, now, out nextReminderTime);
            
            // Remove ones that don't exist anymore
            for (int i = 0; i < existingNotifs.Count; i++)
            {
                var existing = existingNotifs[i];
                Guid identifier;
                if (TryGetItemIdFromDayOfTag(existing.Tag, out identifier))
                {
                    if (!itemsThatCouldHaveNotifs.Any(x => x.Item1.Identifier == identifier))
                    {
                        notificationManager.Cancel(existing.Tag, NotificationIds.DAY_OF_NOTIFICATIONS);
                        existingNotifs.RemoveAt(i);
                        i--;
                    }
                }
            }
            
            foreach (var item in itemsThatCouldHaveNotifs)
            {
                UpdateDayOfNotification(account.LocalAccountId, item.Item1, item.Item2, context, notificationManager, existingNotifs, fromForeground);
            }

            // Need to mark them as sent
            Guid[] newlySentTaskReminders = itemsThatCouldHaveNotifs.Select(i => i.Item1).Where(i => i.Type == TaskOrEventType.Task && !i.HasSentReminder).Select(i => i.Identifier).ToArray();
            Guid[] newlySentEventReminders = itemsThatCouldHaveNotifs.Select(i => i.Item1).Where(i => i.Type == TaskOrEventType.Event && !i.HasSentReminder).Select(i => i.Identifier).ToArray();

            if (newlySentTaskReminders.Length > 0 || newlySentEventReminders.Length > 0)
            {
                var dataStore = await AccountDataStore.Get(account.LocalAccountId);
                if (dataStore != null)
                {
                    await dataStore.MarkAndroidRemindersSent(newlySentTaskReminders, newlySentEventReminders);
                }
            }
            
            // Schedule the next alarm
            if (nextReminderTime > now)
            {
                AlarmManagerHelper.Schedule(
                    context: context,
                    receiverType: typeof(UpdateDayOfNotificationsReceiver),
                    wakeTime: nextReminderTime,
                    uriData: "powerplanner:?localAccountId=" + account.LocalAccountId,
                    wakeDevice: true);
            }
        }

        private static bool TryGetItemIdFromDayOfTag(string tag, out Guid answer)
        {
            int index = tag.IndexOf(';');
            if (index == -1)
            {
                answer = Guid.Empty;
                return false;
            }

            if (Guid.TryParse(tag.Substring(index + 1), out answer))
            {
                return true;
            }

            answer = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Returns a list of items that should have active notifications (ignoring whether user already dismissed them)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="agendaItems"></param>
        /// <param name="now"></param>
        /// <param name="nextReminderTime"></param>
        /// <returns></returns>
        private static List<Tuple<ViewItemTaskOrEvent, DateTime>> GetItemsThatCouldHaveDayOfNotifications(AccountDataItem account, AgendaViewItemsGroup agendaItems, DateTime now, out DateTime nextReminderTime)
        {
            List<Tuple<ViewItemTaskOrEvent, DateTime>> shouldBeActive = new List<Tuple<ViewItemTaskOrEvent, DateTime>>();

            nextReminderTime = DateTime.MinValue;
            PowerPlannerSending.Schedule.Week currentWeek = account.GetWeekOnDifferentDate(now);

            // Add past-due incomplete tasks (we don't add events since they're "complete" when they're past-due)
            foreach (var task in agendaItems.Items.Where(i => i.Type == TaskOrEventType.Task && i.Date.Date < now.Date).OrderBy(i => i))
            {
                shouldBeActive.Add(new Tuple<ViewItemTaskOrEvent, DateTime>(task, task.Date.Date));
            }

            // Look at items due today
            // NOTE: Assuming that end time is always on the same day. If we allow events that span multiple days,
            // we might need to update this so it correctly expires the event on the future date.
            // To make sure we include items due 00:30 on the next day, we have to factor in tomorrow
            DateTime tomorrow = now.Date.AddDays(1);
            var weekForTomorrow = account.GetWeekOnDifferentDate(tomorrow);
            foreach (var item in agendaItems.Items.Where(i => i.Date.Date == now.Date || i.Date.Date == tomorrow).OrderBy(i => i))
            {
                // Get the reminder time
                DateTime reminderTime = GetDayOfReminderTime(item.Date.Date == tomorrow ? weekForTomorrow : currentWeek, item);

                // If it's past the reminder time
                if (now >= reminderTime)
                {
                    // If it has an end time
                    DateTime endTime;
                    if (item.TryGetEndDateWithTime(out endTime))
                    {
                        // We have to consider the end time for updating reminders, since events
                        // expire once they're over (so need to update again right when an event completes)
                        if (endTime > now && (nextReminderTime == DateTime.MinValue || endTime < nextReminderTime))
                        {
                            shouldBeActive.Add(new Tuple<ViewItemTaskOrEvent, DateTime>(item, reminderTime));
                            nextReminderTime = endTime;
                        }
                    }

                    else
                    {
                        // Otherwise add it
                        shouldBeActive.Add(new Tuple<ViewItemTaskOrEvent, DateTime>(item, reminderTime));
                    }
                }
                else if (nextReminderTime == DateTime.MinValue || reminderTime < nextReminderTime)
                {
                    nextReminderTime = reminderTime;
                }
            }

            // If no time found, we pick an item that's due in the future
            if (nextReminderTime == DateTime.MinValue)
            {
                DateTime? nextDateWithItems = agendaItems.Items.Where(i => i.Date.Date > now.Date).OrderBy(i => i.Date.Date).FirstOrDefault()?.Date.Date;
                if (nextDateWithItems != null)
                {
                    // Get the week for that date
                    var week = account.GetWeekOnDifferentDate(nextDateWithItems.Value);

                    // Look through all items on that date
                    foreach (var item in agendaItems.Items.Where(i => i.Date.Date == nextDateWithItems.Value))
                    {
                        // Pick the smallest reminder time
                        DateTime reminderTime = GetDayOfReminderTime(currentWeek, item);
                        if (nextReminderTime == DateTime.MinValue || reminderTime < nextReminderTime)
                        {
                            nextReminderTime = reminderTime;
                        }
                    }
                }
            }

            return shouldBeActive;
        }

        private static DateTime GetDayOfReminderTime(PowerPlannerSending.Schedule.Week weekOnItemDate, ViewItemTaskOrEvent item)
        {
            bool hadSpecificTime;
            return item.GetDayOfReminderTime(out hadSpecificTime);
        }
        
        private static void UpdateDayOfNotification(Guid localAccountId, ViewItemTaskOrEvent item, DateTime dayOfReminderTime, Context context, NotificationManager notificationManager, List<StatusBarNotification> existingNotifs, bool fromForeground)
        {
            ViewItemClass c = item.Class;
            if (c == null)
            {
                return;
            }

            string tag = localAccountId.ToString() + ";" + item.Identifier.ToString();
            var existing = existingNotifs.FirstOrDefault(i => i.Tag == tag);

            // If the reminder has already been sent and the notification doesn't exist,
            // that means the user dismissed it, so don't show it again
            if (item.HasSentReminder && existing == null)
            {
                return;
            }

            // If there's no existing notification, and the updated time is greater than the desired reminder time,
            // then it was edited after the notification should have been sent, so don't notify, since user was already
            // aware of this item (by the fact that they were editing it)
            if (existing == null && item.Updated.ToLocalTime() > dayOfReminderTime)
            {
                return;
            }

            if (existing == null && fromForeground)
            {
                return;
            }

            const string EXTRA_UNIQUE_ID = "UniqueId";
            string extraUniqueId = (item.Name + ";" + c.Name).GetHashCode().ToString();

            // If there's an existing, and it hasn't changed, do nothing
            if (existing != null && existing.Notification.Extras.GetString(EXTRA_UNIQUE_ID).Equals(extraUniqueId))
            {
                return;
            }

            BaseArguments launchArgs;
            if (item.Type == TaskOrEventType.Task)
            {
                launchArgs = new ViewTaskArguments()
                {
                    LocalAccountId = localAccountId,
                    ItemId = item.Identifier,
                    LaunchSurface = LaunchSurface.Toast
                };
            }
            else
            {
                launchArgs = new ViewEventArguments()
                {
                    LocalAccountId = localAccountId,
                    ItemId = item.Identifier,
                    LaunchSurface = LaunchSurface.Toast
                };
            }
            
            var builder = CreateReminderBuilder(context, localAccountId, launchArgs);
            Bundle b = new Bundle();
            b.PutString(EXTRA_UNIQUE_ID, extraUniqueId);
            builder.SetExtras(b);
            builder.SetContentTitle(item.Name);
            builder.SetChannelId(GetChannelIdForDayOf(localAccountId));
            string subtitle = item.Subtitle;
            if (subtitle != null)
            {
                builder.SetContentText(subtitle);
            }
            notificationManager.Notify(tag, NotificationIds.DAY_OF_NOTIFICATIONS, BuildReminder(builder));
        }

        private static async Task UpdateDayBeforeNotification(AccountDataItem account, AgendaViewItemsGroup agendaItems, Context context, NotificationManager notificationManager, bool fromForeground)
        {
            string tag = account.LocalAccountId.ToString();

            // If no current semester, or no items, then just clear
            if (agendaItems == null || agendaItems.Items.Count == 0)
            {
                notificationManager.Cancel(tag, NotificationIds.DAY_BEFORE_NOTIFICATION);
                return;
            }

            StatusBarNotification existing;
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                existing = notificationManager.GetActiveNotifications().FirstOrDefault(i => i.Id == NotificationIds.DAY_BEFORE_NOTIFICATION && i.Tag == tag);
            }
            else
            {
                // GetActiveNotifications didn't exist till API 23
                existing = null;
            }

            const string EXTRA_UNIQUE_ID = "UniqueId";
            
            DateTime now = DateTime.Now;
            
            DateTime reminderTime = RemindersExtension.GetDayBeforeReminderTime(now.Date, account, agendaItems);

            bool sendingDueTomorrow = now >= reminderTime;

            NotificationCompat.Builder builder;

            if (sendingDueTomorrow)
            {
                var itemsDueTomorrow = agendaItems.Items.Where(i => i.Date.Date == now.Date.AddDays(1)).OrderBy(i => i).ToArray();

                // If nothing tomorrow, we clear
                if (itemsDueTomorrow.Length == 0)
                {
                    notificationManager.Cancel(tag, NotificationIds.DAY_BEFORE_NOTIFICATION);
                    return;
                }

                string extraUniqueId = "DueTomorrow" + string.Join(";", itemsDueTomorrow.Select(i => i.Name)).GetHashCode();

                // If already updated
                if (existing != null && existing.Notification.Extras.GetString(EXTRA_UNIQUE_ID).Equals(extraUniqueId))
                {
                    return;
                }

                // If all the items have been updated more recently than the reminder time, we won't show a new notification
                if (existing == null && itemsDueTomorrow.All(i => i.Updated.ToLocalTime() > reminderTime))
                {
                    return;
                }

                // If we already sent the notification today
                if (account.DateLastDayBeforeReminderWasSent.Date == now.Date)
                {
                    // If the notification has been dismissed, do nothing
                    if (existing == null)
                    {
                        return;
                    }
                }

                // Otherwise we need to update the date that it was sent
                else
                {
                    account.DateLastDayBeforeReminderWasSent = now;
                    await AccountsManager.Save(account);
                }

                if (existing == null && fromForeground)
                {
                    return;
                }

                builder = CreateReminderBuilderForDayBefore(context, account.LocalAccountId);
                Bundle b = new Bundle();
                b.PutString(EXTRA_UNIQUE_ID, extraUniqueId);
                builder.SetExtras(b);
                // "Due tomorrow"
                builder.SetContentTitle(PowerPlannerResources.GetDueX(PowerPlannerResources.GetRelativeDateTomorrow().ToLower()));
                PopulateNotificationDayBeforeWithItems(builder, itemsDueTomorrow);
            }

            // Otherwise "due today"
            else
            {
                var itemsDueToday = agendaItems.Items.Where(i => i.Date.Date == now.Date).OrderBy(i => i).ToArray();

                // If nothing left today, we clear
                if (itemsDueToday.Length == 0)
                {
                    notificationManager.Cancel(tag, NotificationIds.DAY_BEFORE_NOTIFICATION);
                    return;
                }

                string extraUniqueId = "DueToday" + string.Join(";", itemsDueToday.Select(i => i.Name)).GetHashCode();

                // If already updated
                if (existing != null && existing.Notification.Extras.GetString(EXTRA_UNIQUE_ID).Equals(extraUniqueId))
                {
                    return;
                }

                // If the notification doesn't currently exist (user potentially dismissed it) and we've already sent either yesterday or today
                if (existing == null && account.DateLastDayBeforeReminderWasSent.Date == DateTime.Today.AddDays(-1) || account.DateLastDayBeforeReminderWasSent.Date == DateTime.Today)
                {
                    return;
                }

                // If all the items have been updated more recently than the reminder time, we won't show a new notification
                if (existing == null && itemsDueToday.All(i => i.Updated.ToLocalTime() > reminderTime))
                {
                    return;
                }

                // If we already sent the notification yesterday
                if (account.DateLastDayBeforeReminderWasSent.Date == now.Date.AddDays(-1))
                {
                    // If the notification has been dismissed, do nothing
                    if (existing == null)
                    {
                        return;
                    }
                }

                // Otherwise we need to update the date that it was sent
                else
                {
                    account.DateLastDayBeforeReminderWasSent = now.Date.AddDays(-1);
                    await AccountsManager.Save(account);
                }

                if (existing == null && fromForeground)
                {
                    return;
                }

                builder = CreateReminderBuilderForDayBefore(context, account.LocalAccountId);
                Bundle b = new Bundle();
                b.PutString(EXTRA_UNIQUE_ID, extraUniqueId);
                builder.SetExtras(b);
                // "Due today"
                builder.SetContentTitle(PowerPlannerResources.GetDueX(PowerPlannerResources.GetRelativeDateToday().ToLower()));
                PopulateNotificationDayBeforeWithItems(builder, itemsDueToday);
            }
            
            notificationManager.Notify(tag, NotificationIds.DAY_BEFORE_NOTIFICATION, BuildReminder(builder));
        }

        private static void PopulateNotificationDayBeforeWithItems(NotificationCompat.Builder builder, ViewItemTaskOrEvent[] items)
        {
            if (items.Length == 1)
            {
                builder.SetContentText(items[0].Name);
            }

            else
            {
                builder.SetContentText(items.Length + " items");
                var inboxStyle = new NotificationCompat.InboxStyle();
                for (int i = 0; i < 5 && i < items.Length; i++)
                {
                    inboxStyle.AddLine(items[i].Name);
                }
                if (items.Length > 5)
                {
                    inboxStyle.SetSummaryText("+" + (items.Length - 5) + " more");
                }
                builder.SetStyle(inboxStyle);
            }
        }

        private static Notification BuildReminder(NotificationCompat.Builder builder)
        {
            Notification notif = builder.Build();
            return notif;
        }

        private static object _initializeChannelsLock = new object();
        private static Task _initializeChannelsTask;
        /// <summary>
        /// Only intiializes channels if they haven't been initialized yet, you can call this multiple times
        /// </summary>
        /// <param name="notificationManager"></param>
        /// <returns></returns>
        public static Task InitializeChannelsAsync(NotificationManager notificationManager)
        {
            Task channelsTaskToAwait;
            lock (_initializeChannelsLock)
            {
                if (_initializeChannelsTask == null || _initializeChannelsTask.IsFaulted)
                {
                    _initializeChannelsTask = ActuallyInitializeChannelsAsync(notificationManager);
                }
                channelsTaskToAwait = _initializeChannelsTask;
            }

            return channelsTaskToAwait;
        }

        private static async Task ActuallyInitializeChannelsAsync(NotificationManager notificationManager)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var accounts = await AccountsManager.GetAllAccounts();
            var groups = notificationManager.NotificationChannelGroups;

            // Delete groups which the account was deleted
            foreach (var g in groups)
            {
                if (!accounts.Any(a => a.LocalAccountId.ToString() == g.Id))
                {
                    notificationManager.DeleteNotificationChannelGroup(g.Id);
                }
            }

            // Add/update groups
            foreach (var a in accounts)
            {
                var g = groups.FirstOrDefault(i => i.Id == a.LocalAccountId.ToString());
                if (g != null)
                {
                    // If details are already correct
                    if (g.Name == a.Username)
                    {
                        // Do nothing
                        continue;
                    }
                }

                // Otherwise need to create (or update) group
                g = new NotificationChannelGroup(a.LocalAccountId.ToString(), a.Username);
                notificationManager.CreateNotificationChannelGroup(g);
            }

            // Update/create channels
            foreach (var a in accounts)
            {
                var channelClassReminders = new NotificationChannel(GetChannelIdForClassReminders(a.LocalAccountId), "Class reminders", NotificationImportance.High)
                {
                    Description = "Class schedule reminders, appears up to 60 minutes before each class starts, configurable in settings.",
                    Group = a.LocalAccountId.ToString(),
                    LightColor = new Color(55, 84, 198), // #3754C6 (a bit more vibrant than my other theme colors)
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channelClassReminders.SetShowBadge(true);
                channelClassReminders.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channelClassReminders);

                var channelDayBefore = new NotificationChannel(GetChannelIdForDayBefore(a.LocalAccountId), "Day before reminders", NotificationImportance.High)
                {
                    Description = "Daily reminders that tell you what incomplete tasks or events you have coming up tomorrow.",
                    Group = a.LocalAccountId.ToString(),
                    LightColor = new Color(55, 84, 198), // #3754C6 (a bit more vibrant than my other theme colors)
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channelDayBefore.SetShowBadge(true);
                channelDayBefore.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channelDayBefore);

                var channelDayOf = new NotificationChannel(GetChannelIdForDayOf(a.LocalAccountId), "Day of reminders", NotificationImportance.High)
                {
                    Description = "Reminders that appear an hour before an incomplete task or event is due.",
                    Group = a.LocalAccountId.ToString(),
                    LightColor = new Color(55, 84, 198), // #3754C6 (a bit more vibrant than my other theme colors)
                    LockscreenVisibility = NotificationVisibility.Public
                };
                channelDayOf.SetShowBadge(true);
                channelDayOf.EnableVibration(true);
                notificationManager.CreateNotificationChannel(channelDayOf);
            }
        }

        private static string GetChannelIdForDayBefore(Guid localAccountId)
        {
            return localAccountId + "_DayBefore";
        }

        private static string GetChannelIdForDayOf(Guid localAccountId)
        {
            return localAccountId + "_DayOf";
        }

        public static string GetChannelIdForClassReminders(Guid localAccountId)
        {
            return localAccountId + "_ClassReminders";
        }

        private static NotificationCompat.Builder CreateReminderBuilderForDayBefore(Context context, Guid localAccountId)
        {
            return CreateReminderBuilder(context, localAccountId, new ViewPageArguments()
            {
                LocalAccountId = localAccountId,
                Page = ViewPageArguments.Pages.Agenda,
                LaunchSurface = LaunchSurface.Toast
            })
            .SetChannelId(GetChannelIdForDayBefore(localAccountId));
        }

        public static NotificationCompat.Builder CreateReminderBuilder(Context context, Guid localAccountId, BaseArguments launchArgs)
        {
            Intent intent = new Intent(context, typeof(MainActivity))
                .SetAction(Intent.ActionView)
                .SetData(Android.Net.Uri.Parse("powerplanner:?" + launchArgs.SerializeToString()));

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            // By setting SDK target to 21 or higher, the logo will automatically become white on the system tray,
            // and will use the color specified when displayed in the notification itself
            var builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_powerplanner_notification)
                .SetColor(new Color(55, 84, 198).ToArgb()) // #3754C6 (a bit more vibrant than my other theme colors)
                .SetCategory(Notification.CategoryReminder)
                .SetContentIntent(pendingIntent)
                .SetShowWhen(false)
                .SetOnlyAlertOnce(true)
                .SetAutoCancel(true);

            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
            {
                // High priority causes the popup banner to appear
                builder.SetPriority((int)NotificationPriority.High);
            }

            return builder;
        }

        public static void ScheduleDayBeforeAlarm(Context context, AccountDataItem account, DateTime today, AgendaViewItemsGroup agendaItems)
        {
            if (agendaItems == null)
            {
                return;
            }

            DateTime dayBeforeReminderTime = RemindersExtension.GetDayBeforeReminderTime(today, account, agendaItems);

            DateTime timeToScheduleAt;

            // If we haven't reached that time yet for "due tomorrow"
            if (dayBeforeReminderTime > DateTime.Now)
            {
                timeToScheduleAt = dayBeforeReminderTime.AddMilliseconds(1);
            }

            // Otherwise we'll need to set the timer for the "due today"
            else
            {
                timeToScheduleAt = today.AddDays(1).AddMilliseconds(1);
            }

            AlarmManagerHelper.Schedule(
                context: context,
                receiverType: typeof(UpdateDayBeforeNotificationReceiver),
                wakeTime: timeToScheduleAt,
                uriData: "powerplanner:?localAccountId=" + account.LocalAccountId,
                wakeDevice: true);
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
    }
}