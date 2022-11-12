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
using Android.Appwidget;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAndroid.Receivers;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Services
{
    [Service(Permission = Android.Manifest.Permission.BindRemoteviews, Exported = false)]
    public class WidgetScheduleService : RemoteViewsService
    {
        public override IRemoteViewsFactory OnGetViewFactory(Intent intent)
        {
            int appWidgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, 0);

            return new WidgetScheduleFactory(ApplicationContext, appWidgetId);
        }

        public class WidgetScheduleFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
        {
            private Context _context;
            private List<object> _items;
            private DateTime _now;
            private int _appWidgetId;
            private Guid _localAccountId;

            public WidgetScheduleFactory(Context context, int appWidgetId)
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
                    // We actually have 5 view types, there are...
                    // - day headers
                    // - empty item
                    // - Classes without room
                    // - Classes with room
                    // - Holiday
                    // We can't simply use SetVisibility on those cause the way view recycling works it messes up when you use SetVisibility
                    return 5;
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
                        var dateHeaderView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetScheduleDateListItem);
                        dateHeaderView.SetTextViewText(Resource.Id.WidgetScheduleDateHeaderTextView, WidgetAgendaService.WidgetAgendaFactory.ToFriendlyDate((DateTime)item, _now));

                        return dateHeaderView;
                    }

                    if (item is string)
                    {
                        // String represents "Nothing due!", etc
                        var emptyView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetScheduleEmptyListItem);
                        emptyView.SetTextViewText(Resource.Id.WidgetScheduleEmptyListItemTextView, item as string);

                        return emptyView;
                    }

                    if (item is ViewItemHoliday)
                    {
                        var holiday = item as ViewItemHoliday;

                        RemoteViews holidayView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetScheduleHolidayListItem);
                        holidayView.SetTextViewText(Resource.Id.WidgetScheduleHolidayNameTextView, holiday.Name.Trim());
                        holidayView.SetInt(Resource.Id.WidgetScheduleHolidayListItem, "setBackgroundColor", new Android.Graphics.Color(228, 0, 137));

                        Intent holidayIntent = new Intent()
                            .SetAction(Intent.ActionView)
                            .SetData(Android.Net.Uri.Parse("powerplanner:?" + new ViewHolidayArguments()
                            {
                                ItemId = holiday.Identifier,
                                LaunchSurface = LaunchSurface.SecondaryTile,
                                LocalAccountId = _localAccountId
                            }.SerializeToString()));
                        holidayView.SetOnClickFillInIntent(Resource.Id.WidgetScheduleHolidayListItem, holidayIntent);

                        return holidayView;
                    }

                    var schedule = item as ViewItemSchedule;
                    var c = schedule.Class;
                    if (c == null)
                    {
                        var emptyView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetScheduleEmptyListItem);
                        return emptyView;
                    }

                    bool hasRoom = !string.IsNullOrWhiteSpace(schedule.Room);

                    RemoteViews classView = new RemoteViews(_context.PackageName, hasRoom ? Resource.Layout.WidgetScheduleClassWithRoomListItem : Resource.Layout.WidgetScheduleClassListItem);
                    classView.SetTextViewText(Resource.Id.WidgetScheduleClassNameTextView, c.Name.Trim());
                    classView.SetTextViewText(Resource.Id.WidgetScheduleClassTimeTextView, Views.ListItems.MyScheduleItemView.GetStringTimeToTime(schedule));
                    if (hasRoom)
                    {
                        classView.SetTextViewText(Resource.Id.WidgetScheduleClassRoomTextView, schedule.Room.Trim());
                    }
                    classView.SetInt(Resource.Id.WidgetScheduleClassListItem, "setBackgroundColor", ColorTools.GetColor(c.Color));

                    Intent classIntent = new Intent()
                        .SetAction(Intent.ActionView)
                        .SetData(Android.Net.Uri.Parse("powerplanner:?" + new ViewClassArguments()
                        {
                            ItemId = c.Identifier,
                            LaunchSurface = LaunchSurface.SecondaryTile,
                            LocalAccountId = _localAccountId
                        }.SerializeToString()));
                    classView.SetOnClickFillInIntent(Resource.Id.WidgetScheduleClassListItem, classIntent);

                    return classView;
                }
                catch (Exception ex)
                {
                    // Out of range exception can be expected since the items list can change while we're working
                    if (!(ex is ArgumentOutOfRangeException))
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    var emptyView = new RemoteViews(_context.PackageName, Resource.Layout.WidgetScheduleEmptyListItem);
                    return emptyView;
                }
            }

            public void OnCreate()
            {
                // Nothing to do - OnDataSetChanged() will be called by Android after OnCreate()
            }

            private async System.Threading.Tasks.Task InitializeDataAsync()
            {
                List<object> items = null;
                _now = DateTime.Now;
                bool hasAccount = false;
                bool hasSemester = false;
                List<ViewItemSchedule> schedulesToday = new List<ViewItemSchedule>();

                try
                {
                    await System.Threading.Tasks.Task.Run(async delegate
                    {
                        var account = await AccountsManager.GetLastLogin();

                        if (account != null)
                        {
                            _localAccountId = account.LocalAccountId;
                            hasAccount = true;

                            const int DAYS_INCLUDING_TODAY = 7;
                            var scheduleTileData = await ScheduleTileDataHelper.LoadAsync(account, _now.Date, DAYS_INCLUDING_TODAY);

                            if (scheduleTileData.HasSemester)
                            {
                                hasSemester = true;

                                items = new List<object>();

                                foreach (var dayData in scheduleTileData.GetDataForAllDays())
                                {
                                    if (dayData.Holidays.Any())
                                    {
                                        // If we already displayed these holidays, skip subsequent duplicate days
                                        if (items.Count >= dayData.Holidays.Length && items.TakeLast(dayData.Holidays.Length).SequenceEqual(dayData.Holidays))
                                        {
                                            // Skip
                                        }
                                        else
                                        {
                                            // If not today
                                            if (!dayData.IsToday)
                                            {
                                                // We add the date header only for non-today
                                                items.Add(dayData.Date);
                                            }

                                            items.AddRange(dayData.Holidays);
                                        }
                                    }

                                    else if (dayData.Schedules.Any())
                                    {
                                        // If not today
                                        if (!dayData.IsToday)
                                        {
                                            // If there's currently no items
                                            if (items.Count == 0)
                                            {
                                                // Add the text saying no class today!
                                                items.Add(PowerPlannerResources.GetString("String_NoClassToday"));
                                            }

                                            // We add the date header only for non-today
                                            items.Add(dayData.Date);
                                        }

                                        // If today
                                        if (dayData.IsToday)
                                        {
                                            foreach (var s in dayData.Schedules)
                                            {
                                                if (s.EndTime.TimeOfDay > _now.TimeOfDay)
                                                {
                                                    items.Add(s);
                                                    schedulesToday.Add(s);
                                                }
                                            }

                                            if (items.Count == 0)
                                            {
                                                items.Add(PowerPlannerResources.GetString("String_NoMoreClasses"));
                                            }
                                        }
                                        else
                                        {
                                            items.AddRange(dayData.Schedules);
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
                                PowerPlannerResources.GetString("String_NoClassesThisWeek")
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
                    DateTime nextChangeTime = _now.Date.AddDays(1);
                    if (schedulesToday.Count > 0)
                    {
                        nextChangeTime = _now.Date.Add(schedulesToday.First().EndTime.TimeOfDay);
                    }

                    AlarmManagerHelper.Schedule(
                        context: _context,
                        receiverType: typeof(UpdateWidgetScheduleReceiver),
                        wakeTime: nextChangeTime);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
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