﻿using PowerPlannerAppDataLibrary.Extensions;
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
                ClearReminders(localAccountId, CancellationToken.None);
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
                    await ResetReminders(account, CancellationToken.None);
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
        private static ToastNotifier ToastNotifier
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

        private void ClearReminders(Guid localAccountId, CancellationToken token)
        {
            foreach (var n in ToastNotifier.GetScheduledToastNotifications().Where(i => i.Id.Equals(GetId(localAccountId))).ToArray())
            {
                token.ThrowIfCancellationRequested();

                ToastNotifier.RemoveFromSchedule(n);
            }
        }

        private static string GetId(Guid localAccountId)
        {
            return localAccountId.GetHashCode().ToString();
        }

        private static string GetId(AccountDataItem account)
        {
            return GetId(account.LocalAccountId);
        }

        private async Task ResetReminders(AccountDataItem account, CancellationToken token)
        {
            if (account == null)
                return;

            ClearReminders(account.LocalAccountId, token);

            token.ThrowIfCancellationRequested();

            var semesterId = account.CurrentSemesterId;
            if (semesterId == Guid.Empty)
                return;

            DateTime todayAsUtc = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            BaseViewItemHomeworkExam[] itemsDueTodayOrGreater;
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
                Dictionary<DateTime, List<BaseViewItemHomeworkExam>> groupedByDay = new Dictionary<DateTime, List<BaseViewItemHomeworkExam>>();

                DateTime tomorrow = DateTime.SpecifyKind(todayAsUtc.AddDays(1), DateTimeKind.Local);

                //select all incomplete homework that is due on tomorrow or later
                foreach (BaseViewItemHomeworkExam h in itemsDueTodayOrGreater.Where(i => i.Date.Date >= tomorrow))
                {
                    token.ThrowIfCancellationRequested();

                    var hDate = h.Date.Date;

                    if (!groupedByDay.TryGetValue(hDate, out List<BaseViewItemHomeworkExam> group))
                    {
                        group = new List<BaseViewItemHomeworkExam>();
                        groupedByDay[hDate] = group;
                    }

                    group.Add(h);
                }

                foreach (var pair in groupedByDay)
                {
                    token.ThrowIfCancellationRequested();

                    DateTime dueOn = pair.Key;
                    List<BaseViewItemHomeworkExam> items = pair.Value;

                    DateTime reminderTime = GetDayBeforeReminderTime(dueOn.AddDays(-1), account, allSchedules);

                    if (!IsTimeOkay(reminderTime))
                        continue;


                    ViewItemHomework[] homeworks = items.OfType<ViewItemHomework>().ToArray();
                    ViewItemExam[] exams = items.OfType<ViewItemExam>().ToArray();


                    if (homeworks.Length > 0)
                    {
                        XmlDocument xml = GenerateToastReminder(
                            homeworks.Length == 1 ? "You have 1 item due tomorrow" : "You have " + homeworks.Length + " items due tomorrow",
                            GetItemLineText(homeworks[0]),
                            homeworks.Length >= 2 ? GetItemLineText(homeworks[1]) : null,
#pragma warning disable 0612
                            new QueryStringHelper()
#pragma warning restore 0612
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

                        Schedule(
                            GenerateScheduledToastNotification(
                                xml,
                                reminderTime,
                                GetId(account), //id's don't need to be unique
                                remoteId)
                            );
                    }

                    if (exams.Length > 0)
                    {
                        XmlDocument xml = GenerateToastReminder(
                            exams.Length == 1 ? "You have 1 event tomorrow" : "You have " + exams.Length + " events tomorrow",
                            GetItemLineText(exams[0]),
                            exams.Length >= 2 ? GetItemLineText(exams[1]) : null,
#pragma warning disable 0612
                            new QueryStringHelper()
#pragma warning restore 0612
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

                        Schedule(
                            GenerateScheduledToastNotification(
                            xml,
                            reminderTime,
                            GetId(account),
                            remoteId));
                    }
                }
            }

            if (account.RemindersDayOf)
            {
                foreach (BaseViewItemHomeworkExam h in itemsDueTodayOrGreater)
                {
                    token.ThrowIfCancellationRequested();
                    bool hasClassTime = false;

                    DateTime reminderTime = GetDayOfReminderTime(h, ref hasClassTime);

                    if (!IsTimeOkay(reminderTime))
                        continue;

                    string subtitle = GetClassName(h) + " - ";

                    if (h is BaseViewItemHomework)
                        subtitle += "due ";

                    if (hasClassTime)
                    {
                        subtitle += "in one hour";
                    }

                    else
                    {
                        subtitle += "today";
                    }

                    XmlDocument xml = GenerateToastReminder(
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

                    Schedule(
                        GenerateScheduledToastNotification(
                            xml,
                            reminderTime,
                            GetId(account),
                            remoteId));
                }
            }
        }

        private static DateTime GetDayOfReminderTime(BaseViewItemHomeworkExam h, ref bool hasClassTime)
        {
            ViewItemClass c = h.GetClassOrNull();

            if (c == null)
                return DateTime.MinValue;

            return h.GetDayOfReminderTime(out hasClassTime);
        }

        private static string GetItemLineText(BaseViewItemHomeworkExam item)
        {
            return GetClassName(item) + " - " + TrimString(item.Name, 150);
        }

        private static string GetClassName(BaseViewItemHomeworkExam item)
        {
            if (item == null)
                return "";

            var c = item.GetClassOrNull();

            if (c != null)
                return c.Name;

            return "";
        }

        private static void Schedule(ScheduledToastNotification notification)
        {
            ToastNotifier.AddToSchedule(notification);
        }

        private static ScheduledToastNotification GenerateScheduledToastNotification(XmlDocument xml, DateTime startTime, string id, string remoteId)
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

        private static XmlDocument GenerateToastReminder(string title, string item1, string item2, string launch)
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
        private static bool IsTimeOkay(DateTime time)
        {
            return time >= DateTime.Now.AddSeconds(10);
        }

        private static DateTime GetDayBeforeReminderTime(DateTime date, AccountDataItem account, IEnumerable<ViewItemSchedule> allSchedules)
        {
            date = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);

            //if they're using custom end times
            if (account.CustomEndTimes.ContainsKey(date.DayOfWeek))
                return date.Add(account.CustomEndTimes[date.DayOfWeek]).AddMinutes(10);

            PowerPlannerSending.Schedule.Week week = account.GetWeekOnDifferentDate(date);

            //otherwise get all the schedules
            IEnumerable<ViewItemSchedule> schedules;

            schedules = allSchedules.Where(i => i.DayOfWeek == date.DayOfWeek && (i.ScheduleWeek == week || i.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks));

            try
            {
                return date.Add(schedules.Max(i => i.EndTime.TimeOfDay)).AddMinutes(10); //day before reminders show up 10 mins after last class
            }
            catch
            {
                //if there aren't any schedules on that day
                return date.AddHours(15); //3:00 PM is default time for day before reminders
            }
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