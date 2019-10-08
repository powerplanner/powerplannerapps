using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Threading;
using Windows.UI.Notifications;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;
using Windows.Foundation.Metadata;
using ToolsUniversal;
using Microsoft.Toolkit.Uwp.Notifications;
using PowerPlannerUWPLibrary.Helpers;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPRemindersExtension : RemindersExtension
    {
        protected override Task ActuallyClearReminders(Guid localAccountId)
        {
            try
            {
                clearReminders(localAccountId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (UWPExceptionHelper.TrackIfNotificationsIssue(ex, nameof(ActuallyClearReminders)))
                {
                }
                else
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    Debug.WriteLine("ClearReminders failed - " + ex.ToString());
                }
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Runs on a new thread
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        protected override async Task ActuallyResetReminders(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                await System.Threading.Tasks.Task.Run(async delegate
                {
                    await resetReminders(account, data, CancellationToken.None);
                });
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                if (UWPExceptionHelper.TrackIfNotificationsIssue(ex, "ResetReminders"))
                {
                }
                else
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    Debug.WriteLine("ResetReminders failed - " + ex.ToString());
                }
            }
        }


        private static ToastNotifier _toastNotifier;
        private static ToastNotifier toastNotifier
        {
            get
            {
                if (_toastNotifier == null)
                {
                    _toastNotifier = ToastNotificationManager.CreateToastNotifier();
                }

                return _toastNotifier;
            }
        }

        private void clearReminders(Guid localAccountId, CancellationToken token)
        {
            foreach (var n in toastNotifier.GetScheduledToastNotifications().Where(i => i.Id.Equals(getId(localAccountId))).ToArray())
            {
                token.ThrowIfCancellationRequested();

                toastNotifier.RemoveFromSchedule(n);
            }
        }

        private static string getId(Guid localAccountId)
        {
            return localAccountId.GetHashCode().ToString();
        }

        private static string getId(AccountDataItem account)
        {
            return getId(account.LocalAccountId);
        }

        private async Task resetReminders(AccountDataItem account, AccountDataStore data, CancellationToken token)
        {
            if (account == null)
                return;

            clearReminders(account.LocalAccountId, token);

            token.ThrowIfCancellationRequested();

            var semesterId = account.CurrentSemesterId;
            if (semesterId == Guid.Empty)
                return;

            DateTime todayAsUtc = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            ViewItemTaskOrEvent[] itemsDueTodayOrGreater;
            ViewItemSchedule[] allSchedules;

            try
            {
                ScheduleViewItemsGroup viewModelSchedule = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, semesterId, trackChanges: true, includeWeightCategories: false);

                AgendaViewItemsGroup viewModel = await AgendaViewItemsGroup.LoadAsync(account.LocalAccountId, viewModelSchedule.Semester, DateTime.SpecifyKind(todayAsUtc, DateTimeKind.Local), trackChanges: true);

                // Data we need to load and hold are...
                // - Items due today or greater
                // - All schedules
                // We don't need to worry about holding a lock though, if the collections change and that breaks us, that's fine,
                // that implies that the data has been changed anyways and another ResetReminders will come in.
                // Therefore we should also expect to get some exceptions here.

                allSchedules = viewModelSchedule.Classes.SelectMany(i => i.Schedules).ToArray();
                itemsDueTodayOrGreater = viewModel.Items.Where(i => i.Date.Date >= DateTime.SpecifyKind(todayAsUtc, DateTimeKind.Local)).ToArray();
            }
            catch
            {
                // Semester wasn't found or other misc error
                return;
            }

            // Since we're no longer inside the lock and we're using a view group that tracks changes, any properties we access on the view items could change at any time.
            // Therefore we need to take that into consideration and be careful about what we do.

            if (account.RemindersDayBefore)
            {
                Dictionary<DateTime, List<ViewItemTaskOrEvent>> groupedByDay = new Dictionary<DateTime, List<ViewItemTaskOrEvent>>();

                DateTime tomorrow = DateTime.SpecifyKind(todayAsUtc.AddDays(1), DateTimeKind.Local);

                //select all incomplete homework that is due on tomorrow or later
                foreach (ViewItemTaskOrEvent h in itemsDueTodayOrGreater.Where(i => i.Date.Date >= tomorrow))
                {
                    token.ThrowIfCancellationRequested();

                    List<ViewItemTaskOrEvent> group;

                    var hDate = h.Date.Date;

                    if (!groupedByDay.TryGetValue(hDate, out group))
                    {
                        group = new List<ViewItemTaskOrEvent>();
                        groupedByDay[hDate] = group;
                    }

                    group.Add(h);
                }

                foreach (var pair in groupedByDay)
                {
                    token.ThrowIfCancellationRequested();

                    DateTime dueOn = pair.Key;
                    List<ViewItemTaskOrEvent> items = pair.Value;

                    DateTime reminderTime = getDayBeforeReminderTime(dueOn.AddDays(-1), account, allSchedules);

                    if (!isTimeOkay(reminderTime))
                        continue;


                    ViewItemTaskOrEvent[] homeworks = items.OfType<ViewItemTaskOrEvent>().ToArray();
                    ViewItemTaskOrEvent[] exams = items.OfType<ViewItemTaskOrEvent>().ToArray();


                    if (homeworks.Length > 0)
                    {
                        XmlDocument xml = generateToastReminder(
                            homeworks.Length == 1 ? "You have 1 item due tomorrow" : "You have " + homeworks.Length + " items due tomorrow",
                            getItemLineText(homeworks[0]),
                            homeworks.Length >= 2 ? getItemLineText(homeworks[1]) : null,
                            new QueryStringHelper()
                            .SetLocalAccountId(account.LocalAccountId)
                            .SetAction("DayBeforeHomeworkReminder")
                            .ToString()
                        );

                        string remoteId = null;
                        if (account.IsOnlineAccount)
                        {
                            int hashedItems = string.Join(";", homeworks.Select(i => i.Identifier)).GetHashCode();
                            remoteId = $"PP_DayBeforeHomeworks_{account.AccountId}_{hashedItems}";
                        }

                        schedule(
                            generateScheduledToastNotification(
                                xml,
                                reminderTime,
                                getId(account), //id's don't need to be unique
                                remoteId)
                            );
                    }

                    if (exams.Length > 0)
                    {
                        XmlDocument xml = generateToastReminder(
                            exams.Length == 1 ? "You have 1 event tomorrow" : "You have " + exams.Length + " events tomorrow",
                            getItemLineText(exams[0]),
                            exams.Length >= 2 ? getItemLineText(exams[1]) : null,
                            new QueryStringHelper()
                            .SetLocalAccountId(account.LocalAccountId)
                            .SetAction("DayBeforeExamReminder")
                            .ToString()
                        );

                        string remoteId = null;
                        if (account.IsOnlineAccount)
                        {
                            int hashedItems = string.Join(";", exams.Select(i => i.Identifier)).GetHashCode();
                            remoteId = $"PP_DayBeforeExams_{account.AccountId}_{hashedItems}";
                        }

                        schedule(
                            generateScheduledToastNotification(
                            xml,
                            reminderTime,
                            getId(account),
                            remoteId));
                    }
                }
            }

            if (account.RemindersDayOf)
            {
                foreach (ViewItemTaskOrEvent h in itemsDueTodayOrGreater)
                {
                    token.ThrowIfCancellationRequested();
                    bool hasClassTime = false;

                    DateTime reminderTime = getDayOfReminderTime(h, account, ref hasClassTime);

                    if (!isTimeOkay(reminderTime))
                        continue;

                    string subtitle = getClassName(h) + " - ";

                    if (h.Type == TaskOrEventType.Task)
                        subtitle += "due ";

                    if (hasClassTime)
                    {
                        subtitle += "in one hour";
                    }

                    else
                    {
                        subtitle += "today";
                    }

                    XmlDocument xml = generateToastReminder(
                        TrimString(h.Name, 200),
                        subtitle,
                        TrimString(h.Details, 200),
                        ArgumentsHelper.CreateArgumentsForView(h, account.LocalAccountId).SerializeToString()
                    );

                    string remoteId = null;
                    if (account.IsOnlineAccount)
                    {
                        remoteId = $"PP_DayOf_{account.AccountId}_{h.Identifier}";
                    }

                    schedule(
                        generateScheduledToastNotification(
                            xml,
                            reminderTime,
                            getId(account),
                            remoteId));
                }
            }
        }

        private static DateTime getDayOfReminderTime(ViewItemTaskOrEvent h, AccountDataItem account, ref bool hasClassTime)
        {
            ViewItemClass c = h.Class;

            if (c == null)
                return DateTime.MinValue;

            return h.GetDayOfReminderTime(out hasClassTime);
        }

        private static string getItemLineText(ViewItemTaskOrEvent item)
        {
            return getClassName(item) + " - " + TrimString(item.Name, 150);
        }

        private static string getClassName(ViewItemTaskOrEvent item)
        {
            if (item == null)
                return "";

            var c = item.Class;

            if (c != null)
                return c.Name;

            return "";
        }

        private static void schedule(ScheduledToastNotification notification)
        {
            toastNotifier.AddToSchedule(notification);
        }

        private static ScheduledToastNotification generateScheduledToastNotification(XmlDocument xml, DateTime startTime, string id, string remoteId)
        {
            var notif = new ScheduledToastNotification(xml, startTime)
            {
                Id = id
            };

            // If RemoteId property is present
            if (remoteId != null && ApiInformation.IsPropertyPresent(typeof(ScheduledToastNotification).FullName, nameof(ScheduledToastNotification.RemoteId)))
            {
                notif.RemoteId = remoteId;
            }

            return notif;
        }

        private static XmlDocument generateToastReminder(string title, string item1, string item2, string launch)
        {
            ToastBindingGeneric binding = new ToastBindingGeneric()
            {
                Children =
                {
                    new AdaptiveText()
                    {
                        Text = title
                    }
                }
            };

            if (item1 != null)
            {
                binding.Children.Add(new AdaptiveText()
                {
                    Text = item1
                });
            }

            if (item2 != null)
            {
                binding.Children.Add(new AdaptiveText()
                {
                    Text = item2
                });
            }

            ToastContent content = new ToastContent()
            {
                Launch = launch,
                Scenario = ToastScenario.Reminder,
                Visual = new ToastVisual()
                {
                    BindingGeneric = binding
                },

                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastSelectionBox("1")
                        {
                            Items =
                            {
                                new ToastSelectionBoxItem("5", "5 minutes"),
                                new ToastSelectionBoxItem("15", "15 minutes"),
                                new ToastSelectionBoxItem("60", "1 hour"),
                                new ToastSelectionBoxItem("240", "4 hours"),
                                new ToastSelectionBoxItem("1440", "1 day")
                            },

                            DefaultSelectionBoxItemId = "5"
                        }
                    },

                    Buttons =
                    {
                        new ToastButtonSnooze()
                        {
                            SelectionBoxId = "1"
                        },

                        new ToastButtonDismiss()
                    }
                }
            };

            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(content.GetContent());

            //return doc;
            return content.GetXml();
        }

        /// <summary>
        /// Returns true if the time is at least 5 seconds in the future
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static bool isTimeOkay(DateTime time)
        {
            return time >= DateTime.Now.AddSeconds(10);
        }

        private static DateTime getDayBeforeReminderTime(DateTime date, AccountDataItem account, IEnumerable<ViewItemSchedule> allSchedules)
        {
            date = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);

            //if they're using custom end times
            if (account.CustomEndTimes.ContainsKey(date.DayOfWeek))
                return date.Add(account.CustomEndTimes[date.DayOfWeek]).AddMinutes(10);

            PowerPlannerSending.Schedule.Week week = account.GetWeekOnDifferentDate(date);

            //otherwise get all the schedules
            IEnumerable<ViewItemSchedule> schedules;

            schedules = allSchedules.Where(i => i.DayOfWeek == date.DayOfWeek && (i.ScheduleWeek == week || i.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks));

            // If there aren't any schedules on that day
            if (!schedules.Any())
            {
                return date.AddHours(15); // 3:00 PM is default time for day before reminders
            }

            return date.Add(schedules.Max(i => i.EndTime.TimeOfDay)).AddMinutes(10); //day before reminders show up 10 mins after last class
        }

        internal static string TrimString(string str, int maxLength)
        {
            if (str == null)
            {
                return "";
            }

            str = str.Trim();

            if (str.Length > maxLength)
                str = str.Substring(0, maxLength - 3) + "...";

            return str;
        }
    }
}