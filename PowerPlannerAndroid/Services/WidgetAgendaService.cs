using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Extensions;
using Android.Appwidget;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAndroid.Receivers;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerAndroid.Services
{
    [Service(Permission = Android.Manifest.Permission.BindRemoteviews, Exported = false)]
    public class WidgetAgendaService : RemoteViewsService
    {
        public override IRemoteViewsFactory OnGetViewFactory(Intent intent)
        {
            int appWidgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, 0);

            Android.Util.Log.Debug("WidgetAgendaService", "OnGetViewFactory()");
            return new WidgetAgendaFactory(ApplicationContext, appWidgetId);
        }

        public class WidgetAgendaFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
        {
            private Context _context;
            private List<object> _items;
            private DateTime _now;
            private int _appWidgetId;
            private Guid _localAccountId;

            public WidgetAgendaFactory(Context context, int appWidgetId)
            {
                _context = context;
                _appWidgetId = appWidgetId;
            }

            public int Count
            {
                get
                {
                    return _items != null ? _items.Count : 0;
                }
            }

            public bool HasStableIds
            {
                get
                {
                    return true;
                }
            }

            public RemoteViews LoadingView
            {
                get
                {
                    return new RemoteViews(_context.PackageName, Resource.Layout.WidgetLoading);
                }
            }

            public int ViewTypeCount
            {
                get
                {
                    // We're using three view types, day headers, empty item, and items themselves
                    return 3;
                }
            }

            public long GetItemId(int position)
            {
                return position;
            }

            public RemoteViews GetViewAt(int position)
            {
                try
                {
                    object item = _items[position];

                    if (item is DateTime)
                    {
                        var dateHeaderView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetAgendaDateListItem);
                        dateHeaderView.SetTextViewText(Resource.Id.WidgetAgendaDateHeaderTextView, ToFriendlyDate((DateTime)item, _now));

                        return dateHeaderView;
                    }

                    if (item is string)
                    {
                        // String represents "Nothing due!", etc
                        var emptyView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetAgendaEmptyListItem);
                        emptyView.SetTextViewText(Resource.Id.WidgetAgendaEmptyListItemTextView, item as string);

                        return emptyView;
                    }

                    var task = item as BaseViewItemHomeworkExam;
                    var c = task.GetClassOrNull();
                    if (c == null)
                    {
                        var emptyView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetAgendaEmptyListItem);
                        return emptyView;
                    }

                    RemoteViews taskView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetAgendaTaskListItem);
                    taskView.SetTextViewText(Resource.Id.WidgetAgendaTaskTextView, task.Name);
                    taskView.SetInt(Resource.Id.WidgetAgendaTaskColorBar, "setBackgroundColor", ColorTools.GetColor(c.Color));

                    Intent taskIntent = new Intent()
                        .SetAction(Intent.ActionView)
                        .SetData(Android.Net.Uri.Parse("powerplanner:?" + ArgumentsHelper.CreateArgumentsForView(task, _localAccountId).SerializeToString()));
                    taskView.SetOnClickFillInIntent(Resource.Id.WidgetAgendaTaskListItem, taskIntent);

                    return taskView;
                }
                catch (Exception ex)
                {
                    // Out of range exception can be expected since the items list can change while we're working
                    if (!(ex is ArgumentOutOfRangeException))
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    var emptyView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetAgendaEmptyListItem);
                    return emptyView;
                }
            }

            /// <summary>
            /// Returns "x Days Ago", "Today", "Tomorrow", "Two Days", "This Xx", "Next Xx"
            /// </summary>
            /// <param name="dueDate"></param>
            /// <returns></returns>
            public static string ToFriendlyDate(DateTime dueDate, DateTime relativeTo)
            {
                dueDate = dueDate.Date;
                relativeTo = relativeTo.Date;

                if (dueDate < relativeTo)
                    return ((relativeTo - dueDate).Days == 1) ? PowerPlannerResources.GetRelativeDateYesterday() : PowerPlannerResources.GetRelativeDateXDaysAgo((relativeTo - dueDate).Days);

                else if (dueDate == relativeTo)
                    return PowerPlannerResources.GetRelativeDateToday();

                else if (dueDate == relativeTo.AddDays(1))
                    return PowerPlannerResources.GetRelativeDateTomorrow();

                else if (dueDate == relativeTo.AddDays(2))
                    return PowerPlannerResources.GetRelativeDateInTwoDays();

                else if (dueDate < relativeTo.AddDays(7))
                    return PowerPlannerResources.GetRelativeDateThisDayOfWeek(dueDate.DayOfWeek);

                else if (dueDate < relativeTo.AddDays(14))
                    return PowerPlannerResources.GetRelativeDateNextDayOfWeek(dueDate.DayOfWeek);

                // Aug 17
                else if (dueDate < DateTime.MaxValue)
                    return dueDate.ToString("MMM d");

                return null;
            }

            internal static string trim(string original, int length)
            {
                if (original.Length > length)
                    return original.Substring(0, length);

                return original;
            }

            public void OnCreate()
            {
                // Nothing to do - OnDataSetChanged() will be called by Android after OnCreate()
            }

            private async System.Threading.Tasks.Task InitializeDataAsync()
            {
                List<object> items = null;
                List<BaseViewItemHomeworkExam> tasks = null;
                _now = DateTime.Now;
                bool hasAccount = false;
                bool isDisabledInSettings = false;
                bool hasSemester = false;

                try
                {
                    await System.Threading.Tasks.Task.Run(async delegate
                    {
                        var account = await AccountsManager.GetLastLogin();

                        AccountDataStore data = null;

                        if (account != null)
                            data = await AccountDataStore.Get(account.LocalAccountId);

                        if (data != null)
                        {
                            hasAccount = true;

                            isDisabledInSettings = account.MainTileSettings.IsDisabled();

                            if (!isDisabledInSettings)
                            {
                                _localAccountId = account.LocalAccountId;
                                var currSemesterId = account.CurrentSemesterId;
                                if (currSemesterId != Guid.Empty)
                                {
                                    ScheduleViewItemsGroup scheduleViewGroup;
                                    try
                                    {
                                        scheduleViewGroup = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);
                                    }
                                    catch
                                    {
                                        // If semester not found
                                        scheduleViewGroup = null;
                                    }

                                    if (scheduleViewGroup != null)
                                    {
                                        DateTime dateToStartDisplayingFrom = DateTime.SpecifyKind(account.MainTileSettings.GetDateToStartDisplayingOn(_now.Date), DateTimeKind.Utc);

                                        // We don't track changes since we need a fresh version that has been filtered right now.
                                        // Otherwise when we update for an event expiring, if we have a cached version, that event wouldn't have expired!
                                        var agendaViewGroup = await AgendaViewItemsGroup.LoadAsync(account.LocalAccountId, scheduleViewGroup.Semester, _now.Date, trackChanges: false);
                                        hasSemester = true;

                                        // We lock the outside, since we are allowing trackChanges on the view items groups (so we have a chance of loading a cached one)... and since we're on a background thread, the lists inside the
                                        // view items groups could change while we're enumerating, hence throwing an exception. So we lock it to ensure this won't happen, and then we return a copy of the items that we need.
                                        using (await agendaViewGroup.DataChangeLock.LockForReadAsync())
                                        {
                                            var filtered = agendaViewGroup.Items
                                                .Where(i => i.Date.Date >= dateToStartDisplayingFrom);

                                            if (!account.MainTileSettings.ShowHomework)
                                            {
                                                filtered = filtered.Where(i => !(i is ViewItemHomework));
                                            }

                                            if (!account.MainTileSettings.ShowExams)
                                            {
                                                filtered = filtered.Where(i => !(i is ViewItemExam));
                                            }

                                            // Agenda view group doesn't sort, so we have to sort it
                                            tasks = filtered
                                                .OrderBy(i => i)
                                                .Take(20)
                                                .ToList();

                                            // Add date headers
                                            items = new List<object>();
                                            DateTime lastHeader = DateTime.MinValue;
                                            foreach (var t in tasks)
                                            {
                                                if (lastHeader != t.Date.Date)
                                                {
                                                    items.Add(t.Date.Date);
                                                    lastHeader = t.Date.Date;
                                                }

                                                items.Add(t);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    if (items == null || items.Count == 0)
                    {
                        if (hasSemester)
                        {
                            items = new List<object>()
                            {
                                PowerPlannerResources.GetString("String_NothingDue")
                            };
                        }
                        else if (isDisabledInSettings)
                        {
                            items = new List<object>()
                            {
                                PowerPlannerResources.GetString("String_WidgetDisabled")
                            };
                        }
                        else if (hasAccount)
                        {
                        items = new List<object>()
                            {
                                PowerPlannerResources.GetString("String_NoSemester")
                            };
                        }
                        else
                        {
                            items = new List<object>()
                            {
                                PowerPlannerResources.GetString("String_NoAccount")
                            };
                        }
                    }

                    _items = items;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                // Schedule next
                try
                {
                    if (tasks != null && tasks.Count > 0)
                    {
                        DateTime? nextChangeTime = GetNextAgendaChangeTime(tasks, _now);
                        if (nextChangeTime != null)
                        {
                            AlarmManagerHelper.Schedule(
                                context: _context,
                                receiverType: typeof(UpdateWidgetAgendaReceiver),
                                wakeTime: nextChangeTime.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            private static DateTime? GetNextAgendaChangeTime(IEnumerable<BaseViewItemHomeworkExam> items, DateTime now)
            {
                // If there's no items
                if (!items.Any())
                {
                    return null;
                }

                // If there's an event that ends later today
                foreach (var i in items.OfType<ViewItemExam>().Where(i => i.EndTime.Date == now.Date))
                {
                    DateTime endDateWithTime;
                    if (i.TryGetEndDateWithTime(out endDateWithTime) && endDateWithTime > now)
                    {
                        // Then we'll transition at that time
                        return endDateWithTime;
                    }
                }

                // Otherwise we just transition when the day changes
                return now.Date.AddDays(1);
            }

            public void OnDataSetChanged()
            {
                try
                {
                    // The problem is that if I create an async task, even if I use the Wait() syntax,
                    // the system seems to just deadlock and the widget never loads.
                    // So I need to just do the loading on its own non-blocking thread, and then
                    // when it's done, I call the notifyDataSetChanged and will update the data.

                    var updateTask = InitializeDataAsync();

                    int times = 0;
                    // IsCompleted returns true when either RanToCompletion, Faulted, or Canceled
                    while (!updateTask.IsCompleted && times < 20)
                    {
                        System.Threading.Thread.Sleep(500);
                        times++;
                    }

                    if (updateTask.IsFaulted)
                    {
                        TelemetryExtension.Current?.TrackException(updateTask.Exception);
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            public void OnDestroy()
            {
                // Nothing to do
            }
        }
    }
}