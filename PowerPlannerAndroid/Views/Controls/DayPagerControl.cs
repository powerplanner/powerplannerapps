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
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using AndroidX.ViewPager.Widget;

namespace PowerPlannerAndroid.Views.Controls
{
    public class DayPagerControl : ViewPager
    {
        public event EventHandler<DateTime> CurrentDateChanged;
        public event EventHandler<BaseViewItemHomeworkExam> ItemClick;
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
            this.OffscreenPageLimit = 3;

            this.PageSelected += DayPagerControl_PageSelected;
        }

        private void DayPagerControl_PageSelected(object sender, PageSelectedEventArgs e)
        {
            try
            {
                if (ItemsSource != null && Adapter != null)
                {
                    var adapter = this.Adapter as DayPagerAdapter;

                    DateTime date = adapter.GetDate(e.Position);

                    try
                    {
                        CurrentDateChanged?.Invoke(this, date);
                    }

                    catch { }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
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
                DayPagerAdapter adapter = Adapter as DayPagerAdapter;

                if (adapter == null)
                {
                    return DateTime.Today;
                }

                return adapter.GetDate(this.CurrentItem);
            }
        }

        public void Initialize(SemesterItemsViewGroup itemsSource, DateTime currentDate)
        {
            _itemsSource = itemsSource;

            // Unwire events from old one
            var prevAdapter = Adapter as DayPagerAdapter;
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
            this.Adapter = adapter;
            this.SetCurrentItem(1000, false);
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

        private void Adapter_ItemClick(object sender, BaseViewItemHomeworkExam e)
        {
            ItemClick?.Invoke(this, e);
        }

        private class DayPagerAdapter : PagerAdapter
        {
            public event EventHandler<BaseViewItemHomeworkExam> ItemClick;
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

            public override int Count
            {
                get
                {
                    return 2001;
                }
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
            {
                return view == objectValue;
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                DateTime date = GetDate(position);

                var homeworksOnDay = HomeworksOnDay.Get(ItemsSource.Items, date);

                SingleDayControl control = _destroyedControls.FirstOrDefault();

                if (control != null)
                {
                    _destroyedControls.Remove(control);

                    try
                    {
                        control.Initialize(date, homeworksOnDay, ItemsSource);
                    }

                    // ObjectDisposedException actually shouldn't ever occur here. If it does, we should analyze why.
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    // So we'll fall back to creating a new one
                    control = null;
                }

                if (control == null)
                {
                    control = new SingleDayControl(container);
                    control.ItemClick += SingleDayControl_ItemClick;
                    control.HolidayItemClick += SingleDayControl_HolidayItemClick;
                    control.ScheduleItemClick += SingleDayControl_ScheduleItemClick;
                    control.ScheduleClick += SingleDayControl_ScheduleClick;
                    control.Initialize(date, homeworksOnDay, ItemsSource);
                }

                container.AddView(control);

                return control;
            }

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

            private void SingleDayControl_ItemClick(object sender, BaseViewItemHomeworkExam e)
            {
                ItemClick?.Invoke(sender, e);
            }

            // Using a strong reference list so that neither the Java nor the C# object gets disposed (Java shouldn't get disposed
            // since the C# one has a strong reference).
            private List<SingleDayControl> _destroyedControls = new List<SingleDayControl>();

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue)
            {
                var control = (SingleDayControl)objectValue;
                control.Deinitialize();
                container.RemoveView(control);
                _destroyedControls.Add(control);
            }

            public DateTime GetDate(int position)
            {
                return FirstDate.AddDays(position);
            }

            public int GetPosition(DateTime date)
            {
                return (date.Date - FirstDate.Date).Days;
            }
        }
    }
}