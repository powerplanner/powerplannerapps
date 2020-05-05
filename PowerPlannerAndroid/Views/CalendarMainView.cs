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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.Controls;
using InterfacesDroid.Helpers;
using Android.Graphics.Drawables;
using Android.Graphics;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using ToolsPortable;
using InterfacesDroid.DataTemplates;
using Android.Content.Res;
using Android.Graphics.Drawables.Shapes;
using System.ComponentModel;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewLists;
using System.Collections.Specialized;
using AndroidX.Core.Content;
using AndroidX.Core.View;

namespace PowerPlannerAndroid.Views
{
    public class CalendarMainView : InterfacesDroid.Views.PopupViewHost<CalendarViewModel>
    {
        private MyCalendarView _calendarView;
        private DayPagerControl _dayPagerControl;

        public CalendarMainView(ViewGroup root) : base(Resource.Layout.CalendarMain, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            _calendarView = FindViewById<MyCalendarView>(Resource.Id.CalendarView);
            _calendarView.Adapter = new MyCalendarAdapter(ViewModel, ViewModel.DisplayMonth, ViewModel.FirstDayOfWeek);
            _calendarView_DisplayMonthChanged(_calendarView, new EventArgs());
            _calendarView.DisplayMonthChanged += _calendarView_DisplayMonthChanged;
            _calendarView.SelectedDateChanged += _calendarView_SelectedDateChanged;
            _calendarView.SelectedDate = ViewModel.SelectedDate;

            var addItemControl = FindViewById<FloatingAddItemControl>(Resource.Id.FloatingAddItemControl);
            addItemControl.SupportsAddHoliday = true;
            addItemControl.OnRequestAddExam += AddItemControl_OnRequestAddExam;
            addItemControl.OnRequestAddHomework += AddItemControl_OnRequestAddHomework;
            addItemControl.OnRequestAddHoliday += AddItemControl_OnRequestAddHoliday;

            _dayPagerControl = FindViewById<DayPagerControl>(Resource.Id.DayPagerControl);
            _dayPagerControl.Initialize(ViewModel.SemesterItemsViewGroup, ViewModel.SelectedDate);
            _dayPagerControl.ItemClick += _dayPagerControl_ItemClick;
            _dayPagerControl.HolidayItemClick += _dayPagerControl_HolidayItemClick;
            _dayPagerControl.ScheduleItemClick += _dayPagerControl_ScheduleItemClick;
            _dayPagerControl.ScheduleClick += _dayPagerControl_ScheduleClick;
            _dayPagerControl.CurrentDateChanged += _dayPagerControl_CurrentDateChanged;

        }

        private void _dayPagerControl_HolidayItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.ViewItemHoliday e)
        {
            ViewModel.ViewHoliday(e);
        }

        private void AddItemControl_OnRequestAddHoliday(object sender, EventArgs e)
        {
            ViewModel.AddHoliday();
        }

        private void _dayPagerControl_ScheduleClick(object sender, EventArgs e)
        {
            ViewModel.ViewSchedule();
        }

        private void _dayPagerControl_ScheduleItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.ViewItemSchedule e)
        {
            ViewModel.ViewClass(e.Class);
        }

        private void AddItemControl_OnRequestAddHomework(object sender, EventArgs e)
        {
            ViewModel.AddHomework();
        }

        private void AddItemControl_OnRequestAddExam(object sender, EventArgs e)
        {
            ViewModel.AddExam();
        }

        private void _calendarView_SelectedDateChanged(object sender, EventArgs e)
        {
            if (_calendarView.SelectedDate != null)
            {
                ViewModel.SelectedDate = _calendarView.SelectedDate.Value;
            }
        }

        private void _calendarView_DisplayMonthChanged(object sender, EventArgs e)
        {
            DateTime displayMonth = _calendarView.DisplayMonth;
            ViewModel.DisplayMonth = displayMonth;
        }

        private class MyCalendarAdapter : MyCalendarView.CalendarAdapter
        {
            private CalendarViewModel _viewModel;

            public MyCalendarAdapter(CalendarViewModel viewModel, DateTime month, DayOfWeek firstDayOfWeek) : base(month, firstDayOfWeek)
            {
                _viewModel = viewModel;
            }

            public override MyCalendarMonthView GetView(ViewGroup parent, MyCalendarView calendarView)
            {
                return new MySmallCalendarMonthView(parent, calendarView, _viewModel, FirstDayOfWeek);
            }
        }

        private class MySmallCalendarMonthView : MyCalendarMonthView
        {
            private CalendarViewModel _viewModel;
            private TextView _title;

            public MySmallCalendarMonthView(ViewGroup parent, MyCalendarView calendarView, CalendarViewModel viewModel, DayOfWeek firstDayOfWeek) : base(parent, calendarView, firstDayOfWeek)
            {
                _viewModel = viewModel;

                // Now that we've set the view model, call the actual Initialize, which will then call CreateDay (which depends on the view model being set)
                base.Initialize();
            }

            protected override void Initialize()
            {
                // We'll delay initialize until after we've set the view model
            }

            protected override View CreateTitle()
            {
                _title = new TextView(Context)
                {
                    TextSize = 16
                };
                _title.SetPaddingRelative(ThemeHelper.AsPx(Context, 16), ThemeHelper.AsPx(Context, 12), 0, ThemeHelper.AsPx(Context, 12));
                return _title;
            }

            protected override void OnMonthChanged()
            {
                _title.Text = Month.ToString("MMMM yyyy");

                var semester = _viewModel?.SemesterItemsViewGroup?.Semester;
                if (semester != null && !semester.IsMonthDuringThisSemester(Month))
                {
                    OverlayView = new DifferentSemesterOverlayControl(Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(
                            FrameLayout.LayoutParams.MatchParent,
                            FrameLayout.LayoutParams.MatchParent)
                    };
                }
                else
                {
                    OverlayView = null;
                }

                base.OnMonthChanged();
            }

            protected override View CreateDay()
            {
                return new MyCalendarDayView(Context, CalendarView, _viewModel);
            }

            private class MyCalendarDayView : FrameLayout, IMyCalendarDayView
            {
                private MyCalendarView _calendarView;
                private CalendarViewModel _viewModel;
                private DateTime _date;
                private View _selectedRectangleView;
                private View _backgroundView;
                private View _backgroundOverlayView;
                private TextView _tv;
                private ColorStateList _defaultTextColors;
                private MyHomeworkCircles _myHomeworkCircles;
                private HolidaysOnDay _holidaysOnDay;
                private NotifyCollectionChangedEventHandler _holidaysChangedHandler;

                public MyCalendarDayView(Context context, MyCalendarView calendarView, CalendarViewModel viewModel) : base(context)
                {
                    _calendarView = calendarView;
                    _viewModel = viewModel;

                    int margin = ThemeHelper.AsPx(Context, 1);

                    Clickable = true;
                    LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.MatchParent);

                    _backgroundView = new View(Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(
                            FrameLayout.LayoutParams.MatchParent,
                            FrameLayout.LayoutParams.MatchParent)
                        {
                            TopMargin = margin,
                            LeftMargin = margin,
                            RightMargin = margin,
                            BottomMargin = margin
                        }
                    };
                    base.AddView(_backgroundView);

                    _backgroundOverlayView = new View(Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(
                            FrameLayout.LayoutParams.MatchParent,
                            FrameLayout.LayoutParams.MatchParent)
                        {
                            TopMargin = margin,
                            LeftMargin = margin,
                            RightMargin = margin,
                            BottomMargin = margin
                        },
                        Visibility = ViewStates.Gone
                    };
                    base.AddView(_backgroundOverlayView);

                    var padding = ThemeHelper.AsPx(Context, 4);

                    _myHomeworkCircles = new MyHomeworkCircles(Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(
                            FrameLayout.LayoutParams.MatchParent,
                            FrameLayout.LayoutParams.WrapContent)
                        {
                            Gravity = GravityFlags.Bottom,
                            LeftMargin = padding,
                            BottomMargin = padding,
                            RightMargin = padding
                        }
                    };
                    this.AddView(_myHomeworkCircles);

                    _tv = new TextView(Context)
                    {
                        Gravity = GravityFlags.Top | GravityFlags.Right,
                        LayoutParameters = new FrameLayout.LayoutParams(
                            FrameLayout.LayoutParams.MatchParent,
                            FrameLayout.LayoutParams.WrapContent)
                        {
                            TopMargin = ThemeHelper.AsPx(Context, 2),
                            RightMargin = padding
                        }
                    };
                    _defaultTextColors = _tv.TextColors;

                    this.AddView(_tv);
                    
                    _selectedRectangleView = new View(Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(
                            FrameLayout.LayoutParams.MatchParent,
                            FrameLayout.LayoutParams.MatchParent),
                        Visibility = ViewStates.Gone
                    };
                    _selectedRectangleView.Background = ContextCompat.GetDrawable(Context, Resource.Drawable.CalendarSelectedDayBorder);
                    ViewCompat.SetBackgroundTintList(_selectedRectangleView, new ColorStateList(new int[][] { new int[0] }, new int[] { ColorTools.GetColor(this.Context, Resource.Color.accent) }));
                    this.AddView(_selectedRectangleView);

                    this.Click += MyCalendarDayView_Click;

                    UpdateSelectedStatus();

                    _calendarView.SelectedDateChanged += new WeakEventHandler<EventArgs>(_calendarView_SelectedDateChanged).Handler;
                }
                
                private void _calendarView_SelectedDateChanged(object sender, EventArgs e)
                {
                    UpdateSelectedStatus();
                }

                private void MyCalendarDayView_Click(object sender, EventArgs e)
                {
                    _calendarView.SelectedDate = _date;
                }

                private void UpdateSelectedStatus()
                {
                    if (_calendarView.SelectedDate != null && _calendarView.SelectedDate.Value.Date == _date.Date)
                    {
                        _selectedRectangleView.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        _selectedRectangleView.Visibility = ViewStates.Gone;
                    }
                }

                public void UpdateDay(DateTime date, DayType dayType, bool isToday)
                {
                    _date = date;

                    if (isToday)
                    {
                        _backgroundView.SetBackgroundResource(Resource.Color.calendarBackgroundToday);
                    }
                    else
                    {
                        switch (dayType)
                        {
                            case DayType.ThisMonth:
                                _backgroundView.SetBackgroundResource(Resource.Color.calendarBackgroundThisMonth);
                                break;

                            default:
                                _backgroundView.SetBackgroundResource(Resource.Color.calendarBackgroundOther);
                                break;
                        }
                    }

                    _tv.Text = date.Day.ToString();

                    if (isToday)
                    {
                        _tv.SetTextColor(ColorTools.GetColor(this.Context, Resource.Color.calendarTextToday));
                    }
                    else
                    {
                        _tv.SetTextColor(_defaultTextColors);
                    }

                    UpdateSelectedStatus();

                    _myHomeworkCircles.SetItemsSource(_viewModel.SemesterItemsViewGroup.Items.Sublist(i => i is BaseViewItemHomeworkExam && i.Date.Date == date.Date && !(i as BaseViewItemHomeworkExam).IsComplete()));

                    if (_holidaysOnDay != null && _holidaysChangedHandler != null)
                    {
                        _holidaysOnDay.CollectionChanged -= _holidaysChangedHandler;
                    }
                    _holidaysOnDay = HolidaysOnDay.Create(_viewModel.SemesterItemsViewGroup.Items, date.Date);
                    _holidaysChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(_holidaysOnDay_CollectionChanged).Handler;
                    _holidaysOnDay.CollectionChanged += _holidaysChangedHandler;
                    UpdateIsHoliday();
                }

                private void _holidaysOnDay_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    UpdateIsHoliday();
                }

                private void UpdateIsHoliday()
                {
                    if (_holidaysOnDay == null || _holidaysOnDay.Count == 0)
                    {
                        _backgroundOverlayView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        _backgroundOverlayView.Visibility = ViewStates.Visible;
                        _backgroundOverlayView.SetBackgroundColor(Color.Argb(51, 255, 0, 0));
                    }
                }
            }
        }

        private class MyHomeworkCircles : LinearLayout
        {
            private ItemsControlWrapper _itemsControlWrapper;

            public MyHomeworkCircles(Context context) : base(context)
            {
                _itemsControlWrapper = new ItemsControlWrapper(this)
                {
                    ItemTemplate = new CustomDataTemplate<BaseViewItemHomeworkExamGrade>(CreateCircle)
                };
            }

            public void SetItemsSource(MyObservableList<BaseViewItemHomeworkExamGrade> items)
            {
                _itemsControlWrapper.ItemsSource = items;
            }

            private View CreateCircle(ViewGroup root, BaseViewItemHomeworkExamGrade item)
            {
                View view = new View(root.Context)
                {
                    Background = ContextCompat.GetDrawable(root.Context, Resource.Drawable.circle),
                    LayoutParameters = new LinearLayout.LayoutParams(
                        ThemeHelper.AsPx(Context, 5),
                        ThemeHelper.AsPx(Context, 5))
                    {
                        RightMargin = ThemeHelper.AsPx(Context, 3)
                    }
                };

                if (item is BaseViewItemHomeworkExam)
                {
                    ViewCompat.SetBackgroundTintList(view, new ColorStateList(new int[][]
                    {
                        new int[] { }
                    },
                    new int[]
                    {
                        ColorTools.GetColor((item as BaseViewItemHomeworkExam).GetClassOrNull().Color).ToArgb()
                    }));
                }

                return view;
            }
        }

        private void _calendarView_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            DateTime date = new DateTime(e.Year, e.Month + 1, e.DayOfMonth, 0, 0, 0, DateTimeKind.Local);

            ViewModel.SelectedDate = date;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedDate):
                    if (_dayPagerControl.CurrentDate != ViewModel.SelectedDate)
                        _dayPagerControl.Initialize(ViewModel.SemesterItemsViewGroup, ViewModel.SelectedDate);
                    _calendarView.SelectedDate = ViewModel.SelectedDate;
                    break;
            }
        }

        private void _dayPagerControl_CurrentDateChanged(object sender, DateTime e)
        {
            ViewModel.SelectedDate = e;
        }

        private void _dayPagerControl_ItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.BaseViewItems.BaseViewItemHomeworkExam e)
        {
            ViewModel.ShowItem(e);
        }
    }
}