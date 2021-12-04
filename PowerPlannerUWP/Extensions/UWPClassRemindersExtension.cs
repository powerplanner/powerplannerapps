using Microsoft.Toolkit.Uwp.Notifications;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Notifications;

namespace PowerPlannerUWP.Extensions
{
    public class UWPClassRemindersExtension : ClassRemindersExtension
    {
        private const string ClassRemindersGroupPrefix = "cr";
        private static DateTimeFormatter _timeFormatter = new DateTimeFormatter("shorttime");
        private static string _timeToTime = LocalizedResources.GetString("String_TimeToTime");

        protected override Task ResetAllReminders(AccountDataItem account, ScheduleViewItemsGroup scheduleViewItemsGroup)
        {
            try
            {
                var notifier = ToastNotificationManager.CreateToastNotifier();
                var group = ClassRemindersGroupPrefix + "." + UWPRemindersExtension.GetId(account);

                // Clear current scheduled notifications
                try
                {
                    var scheduled = notifier.GetScheduledToastNotifications();
                    foreach (var s in scheduled)
                    {
                        try
                        {
                            if (s.Group == group)
                            {
                                notifier.RemoveFromSchedule(s);
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                if (scheduleViewItemsGroup == null)
                {
                    return Task.CompletedTask;
                }

                // This will be initialized
                var beforeTime = account.ClassRemindersTimeSpan.GetValueOrDefault();

                // Added in 17134
                bool isExpirationTimeSupported = ApiInformation.IsPropertyPresent(typeof(ScheduledToastNotification).FullName, "ExpirationTime");

                var today = DateTime.Today;
                var end = today.AddDays(8); // Schedule out for 8 days

                Dictionary<ViewItemSchedule, XmlDocument> generatedPayloads = new Dictionary<ViewItemSchedule, XmlDocument>();

                for (; today.Date < end.Date; today = today.AddDays(1).Date)
                {
                    // No need to lock changes, if changes occur an exception might occur, but that's fine, reminders would be reset once again anyways
                    var schedulesOnDay = SchedulesOnDay.Get(account, scheduleViewItemsGroup.Classes, today, account.GetWeekOnDifferentDate(today), trackChanges: false);

                    foreach (var s in schedulesOnDay)
                    {
                        var reminderTime = today.Add(s.StartTime.TimeOfDay).Subtract(beforeTime);
                        if (reminderTime >= DateTime.Now.AddSeconds(5))
                        {
                            XmlDocument payload;

                            if (!generatedPayloads.TryGetValue(s, out payload))
                            {
                                payload = GeneratePayload(account, s);
                                generatedPayloads[s] = payload;
                            }

                            var notif = new ScheduledToastNotification(payload, reminderTime)
                            {
                                Group = group,
                                Tag = s.Identifier.ToString() + "." + today.ToString("yy-dd-MM")
                            };

                            if (isExpirationTimeSupported)
                            {
                                notif.ExpirationTime = today.Add(s.EndTime.TimeOfDay);
                            }

                            try
                            {
                                notifier.AddToSchedule(notif);
                            }
                            catch (Exception ex)
                            {
                                // If OS is in a bad state, we'll just stop
                                TelemetryExtension.Current?.TrackException(new Exception("Adding toast to schedule failed: " + ex.Message, ex));
                                return Task.CompletedTask;
                            }
                        }
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                if (UWPExceptionHelper.TrackIfNotificationsIssue(ex, "ClassReminders"))
                {
                }
                else
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                return Task.CompletedTask;
            }
        }

        private static XmlDocument GeneratePayload(AccountDataItem account, ViewItemSchedule s)
        {
            var c = s.Class;

            var builder = new ToastContentBuilder()
                .AddToastActivationInfo(new ViewClassArguments()
                {
                    LocalAccountId = account.LocalAccountId,
                    ItemId = c.Identifier,
                    LaunchSurface = LaunchSurface.Toast
                }.SerializeToString(), ToastActivationType.Foreground)
                .SetToastScenario(ToastScenario.Reminder)
                .AddText(c.Name)
                .AddText(string.Format(_timeToTime, _timeFormatter.Format(s.StartTime), _timeFormatter.Format(s.EndTime)));

            if (!string.IsNullOrWhiteSpace(s.Room))
            {
                builder.AddText(s.Room);
            }

            builder.AddToastInput(new ToastSelectionBox("snoozeTime")
            {
                DefaultSelectionBoxItemId = "5",
                Items =
                {
                    new ToastSelectionBoxItem("5", PowerPlannerResources.GetXMinutes(5)),
                    new ToastSelectionBoxItem("10", PowerPlannerResources.GetXMinutes(10)),
                    new ToastSelectionBoxItem("15", PowerPlannerResources.GetXMinutes(15)),
                    new ToastSelectionBoxItem("30", PowerPlannerResources.GetXMinutes(30)),
                    new ToastSelectionBoxItem("45", PowerPlannerResources.GetXMinutes(45))
                }
            });

            builder.AddButton(new ToastButtonSnooze()
            {
                SelectionBoxId = "snoozeTime"
            });

            builder.AddButton(new ToastButtonDismiss());

            return builder.GetToastContent().GetXml();
        }
    }
}
