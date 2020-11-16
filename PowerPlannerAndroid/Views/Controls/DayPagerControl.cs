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
using Android.Content.Res;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views.Controls
{
    public class DayPagerControl : InflatedView
    {
        private ViewPager2 _viewPager;
        public event EventHandler<DateTime> CurrentDateChanged;
        public event EventHandler<ViewItemTaskOrEvent> ItemClick;
        public event EventHandler<ViewItemHoliday> HolidayItemClick;
        public event EventHandler<ViewItemSchedule> ScheduleItemClick;
        public event EventHandler ScheduleClick;
        public event EventHandler ExpandClick;

        public DayPagerControl(Context context) : base(context, Resource.Layout.DayPager)
        {
            Initialize();
        }

        public DayPagerControl(Context context, IAttributeSet attrs) : base(context, Resource.Layout.DayPager, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            var _buttonExpand = FindViewById<ImageButton>(Resource.Id.ButtonExpand);
            _buttonExpand.Click += _buttonExpand_Click;

            _viewPager = FindViewById<ViewPager2>(Resource.Id.DayViewPager);
            _viewPager.OffscreenPageLimit = 1; // This means views on left, so 1 is actually 2 total offscreen views
            _viewPager.RegisterOnPageChangeCallback(new PageChangeCallback(this));
        }

        private void _buttonExpand_Click(object sender, EventArgs e)
        {
            ExpandClick?.Invoke(this, new EventArgs());
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
                            _dayPagerControl.UpdateHeaderText();
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
            }

            var account = await AccountsManager.GetOrLoad(itemsSource.LocalAccountId);

            var adapter = new DayPagerAdapter(account, itemsSource, currentDate);
            adapter.ItemClick += Adapter_ItemClick;
            adapter.HolidayItemClick += Adapter_HolidayItemClick;
            adapter.ScheduleItemClick += Adapter_ScheduleItemClick;
            adapter.ScheduleClick += Adapter_ScheduleClick;
            _viewPager.Adapter = adapter;
            _viewPager.SetCurrentItem(1000, false);

            UpdateHeaderText();
        }

        private void UpdateHeaderText()
        {
            FindViewById<TextView>(Resource.Id.TextViewHeaderText).Text = GetHeaderText(CurrentDate);
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

                FindViewById(Resource.Id.DayPagerHeader).Visibility = value ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private string GetHeaderText(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return PowerPlannerResources.GetRelativeDateToday().ToUpper();

            else if (date.Date == DateTime.Today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow().ToUpper();

            else if (date.Date == DateTime.Today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday().ToUpper();

            return PowerPlannerAppDataLibrary.Helpers.DateHelpers.ToMediumDateString(date).ToUpper();
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
                    LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                };
                control.ItemClick += SingleDayControl_ItemClick;
                control.HolidayItemClick += SingleDayControl_HolidayItemClick;
                control.ScheduleItemClick += SingleDayControl_ScheduleItemClick;
                control.ScheduleClick += SingleDayControl_ScheduleClick;
                _singleDayControls.Add(control);

                return new GenericRecyclerViewHolder(control);
            }
        }
    }
}