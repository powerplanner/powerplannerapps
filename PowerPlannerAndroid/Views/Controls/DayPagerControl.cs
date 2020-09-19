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
using Android.Util;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using AndroidX.ViewPager2.Widget;
using AndroidX.RecyclerView.Widget;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.DataLayer;
using ToolsPortable;

namespace PowerPlannerAndroid.Views.Controls
{
    public class DayPagerControl : FrameLayout
    {
        private ViewPager2 _viewPager;
        public event EventHandler<DateTime> CurrentDateChanged;
        public event EventHandler<ViewItemTaskOrEvent> ItemClick;
        public event EventHandler<ViewItemHoliday> HolidayItemClick;
        public event EventHandler<ViewItemSchedule> ScheduleItemClick;
        public event EventHandler ScheduleClick;
        public event EventHandler ExpandClick;

        public DayPagerControl(Context context) : base(context)
        {
            Initialize();
        }

        public DayPagerControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        protected DayPagerControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            _viewPager = new ViewPager2(this.Context)
            {
                OffscreenPageLimit = 3,
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            _viewPager.RegisterOnPageChangeCallback(new PageChangeCallback(this));

            base.AddView(_viewPager);
        }

        private class PageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private DayPagerControl _dayPagerControl;

            public PageChangeCallback(DayPagerControl dayPagerControl)
            {
                _dayPagerControl = dayPagerControl;
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    if (_dayPagerControl.ItemsSource != null && _dayPagerControl._viewPager.Adapter != null)
                    {
                        var adapter = _dayPagerControl._viewPager.Adapter as DayPagerAdapter;

                        DateTime date = adapter.GetDate(position);

                        try
                        {
                            _dayPagerControl.CurrentDateChanged?.Invoke(this, date);
                        }

                        catch { }
                    }
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private SemesterItemsViewGroup _itemsSource;
        public SemesterItemsViewGroup ItemsSource
        {
            get { return _itemsSource; }
        }

        public DateTime CurrentDate
        {
            get
            {
                DayPagerAdapter adapter = _viewPager.Adapter as DayPagerAdapter;

                if (adapter == null)
                {
                    return DateTime.Today;
                }

                return adapter.GetDate(_viewPager.CurrentItem);
            }
        }

        public async void Initialize(SemesterItemsViewGroup itemsSource, DateTime currentDate)
        {
            _itemsSource = itemsSource;

            // Unwire events from old one
            var prevAdapter = _viewPager.Adapter as DayPagerAdapter;
            if (prevAdapter != null)
            {
                prevAdapter.ItemClick -= Adapter_ItemClick;
                prevAdapter.HolidayItemClick -= Adapter_HolidayItemClick;
                prevAdapter.ScheduleItemClick -= Adapter_ScheduleItemClick;
                prevAdapter.ScheduleClick -= Adapter_ScheduleClick;
                prevAdapter.ExpandClick -= Adapter_ExpandClick;
            }

            var account = await AccountsManager.GetOrLoad(itemsSource.LocalAccountId);

            var adapter = new DayPagerAdapter(account, itemsSource, currentDate)
            {
                ShowHeader = ShowHeader
            };
            adapter.ItemClick += Adapter_ItemClick;
            adapter.HolidayItemClick += Adapter_HolidayItemClick;
            adapter.ScheduleItemClick += Adapter_ScheduleItemClick;
            adapter.ScheduleClick += Adapter_ScheduleClick;
            adapter.ExpandClick += Adapter_ExpandClick;
            _viewPager.Adapter = adapter;
            _viewPager.SetCurrentItem(1000, false);
        }

        private void Adapter_ExpandClick(object sender, EventArgs e)
        {
            ExpandClick?.Invoke(this, new EventArgs());
        }

        private void Adapter_HolidayItemClick(object sender, ViewItemHoliday e)
        {
            HolidayItemClick?.Invoke(this, e);
        }

        private void Adapter_ScheduleClick(object sender, EventArgs e)
        {
            ScheduleClick?.Invoke(this, e);
        }

        private void Adapter_ScheduleItemClick(object sender, ViewItemSchedule e)
        {
            ScheduleItemClick?.Invoke(this, e);
        }

        private void Adapter_ItemClick(object sender, ViewItemTaskOrEvent e)
        {
            ItemClick?.Invoke(this, e);
        }

        private bool _showHeader = true;
        public bool ShowHeader
        {
            get => _showHeader;
            set
            {
                if (_showHeader == value)
                {
                    return;
                }

                _showHeader = value;

                if (_viewPager?.Adapter is DayPagerAdapter dayPagerAdapter)
                {
                    dayPagerAdapter.ShowHeader = value;
                }
            }
        }

        private class DayPagerAdapter : RecyclerView.Adapter
        {
            public event EventHandler<ViewItemTaskOrEvent> ItemClick;
            public event EventHandler<ViewItemHoliday> HolidayItemClick;
            public event EventHandler<ViewItemSchedule> ScheduleItemClick;
            public event EventHandler ScheduleClick;
            public event EventHandler ExpandClick;

            public DateTime CenterDate { get; private set; }
            public DateTime FirstDate { get; private set; }

            public SemesterItemsViewGroup ItemsSource { get; private set; }
            public AccountDataItem Account { get; private set; }

            public DayPagerAdapter(AccountDataItem account, SemesterItemsViewGroup itemsSource, DateTime currentDate)
            {
                currentDate = currentDate.Date;

                Account = account;
                ItemsSource = itemsSource;
                CenterDate = currentDate;
                FirstDate = currentDate.AddDays(-1000);
            }

            public override int ItemCount => 2001;

            private void SingleDayControl_HolidayItemClick(object sender, ViewItemHoliday e)
            {
                HolidayItemClick?.Invoke(this, e);
            }

            private void SingleDayControl_ScheduleClick(object sender, EventArgs e)
            {
                ScheduleClick?.Invoke(this, e);
            }

            private void SingleDayControl_ScheduleItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.ViewItemSchedule e)
            {
                ScheduleItemClick?.Invoke(this, e);
            }

            private void SingleDayControl_ItemClick(object sender, ViewItemTaskOrEvent e)
            {
                ItemClick?.Invoke(sender, e);
            }

            public DateTime GetDate(int position)
            {
                return FirstDate.AddDays(position);
            }

            public int GetPosition(DateTime date)
            {
                return (date.Date - FirstDate.Date).Days;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                DateTime date = GetDate(position);

                var tasksOrEventsOnDay = TasksOrEventsOnDay.Get(Account, ItemsSource.Items, date);

                (holder.ItemView as SingleDayControl).Initialize(date, tasksOrEventsOnDay, ItemsSource);
            }

            private WeakReferenceList<SingleDayControl> _singleDayControls = new WeakReferenceList<SingleDayControl>();

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var control = new SingleDayControl(parent)
                {
                    LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent),
                    ShowHeader = ShowHeader
                };
                control.ItemClick += SingleDayControl_ItemClick;
                control.HolidayItemClick += SingleDayControl_HolidayItemClick;
                control.ScheduleItemClick += SingleDayControl_ScheduleItemClick;
                control.ScheduleClick += SingleDayControl_ScheduleClick;
                control.ExpandClick += SingleDayControl_ExpandClick;
                _singleDayControls.Add(control);

                return new GenericRecyclerViewHolder(control);
            }

            private void SingleDayControl_ExpandClick(object sender, EventArgs e)
            {
                ExpandClick?.Invoke(this, new EventArgs());
            }

            private bool _showHeader;
            public bool ShowHeader
            {
                get => _showHeader;
                set
                {
                    if (_showHeader == value)
                    {
                        return;
                    }

                    _showHeader = value;

                    foreach (var control in _singleDayControls)
                    {
                        control.ShowHeader = value;
                    }
                }
            }
        }
    }
}