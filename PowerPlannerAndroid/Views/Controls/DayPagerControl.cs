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

        public void Initialize(SemesterItemsViewGroup itemsSource, DateTime currentDate)
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
            }

            var adapter = new DayPagerAdapter(itemsSource, currentDate);
            adapter.ItemClick += Adapter_ItemClick;
            adapter.HolidayItemClick += Adapter_HolidayItemClick;
            adapter.ScheduleItemClick += Adapter_ScheduleItemClick;
            adapter.ScheduleClick += Adapter_ScheduleClick;
            _viewPager.Adapter = adapter;
            _viewPager.SetCurrentItem(1000, false);
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

        private class DayPagerAdapter : RecyclerView.Adapter
        {
            public event EventHandler<ViewItemTaskOrEvent> ItemClick;
            public event EventHandler<ViewItemHoliday> HolidayItemClick;
            public event EventHandler<ViewItemSchedule> ScheduleItemClick;
            public event EventHandler ScheduleClick;

            public DateTime CenterDate { get; private set; }
            public DateTime FirstDate { get; private set; }

            public SemesterItemsViewGroup ItemsSource { get; private set; }

            public DayPagerAdapter(SemesterItemsViewGroup itemsSource, DateTime currentDate)
            {
                currentDate = currentDate.Date;

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

                var tasksOrEventsOnDay = TasksOrEventsOnDay.Get(ItemsSource.Items, date);

                (holder.ItemView as SingleDayControl).Initialize(date, tasksOrEventsOnDay, ItemsSource);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var control = new SingleDayControl(parent)
                {
                    LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                };
                control.ItemClick += SingleDayControl_ItemClick;
                control.HolidayItemClick += SingleDayControl_HolidayItemClick;
                control.ScheduleItemClick += SingleDayControl_ScheduleItemClick;
                control.ScheduleClick += SingleDayControl_ScheduleClick;

                return new GenericRecyclerViewHolder(control);
            }
        }
    }
}