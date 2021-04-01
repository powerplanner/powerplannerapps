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
using Android.Database;
using Java.Lang;
using ToolsPortable;
using AndroidX.ViewPager2.Widget;
using AndroidX.RecyclerView.Widget;

namespace InterfacesDroid.Views
{
    public class MyCalendarView : FrameLayout
    {
        private ViewPager2 _viewPager;
        public event EventHandler DisplayMonthChanged;
        public event EventHandler SelectedDateChanged;

        public MyCalendarView(Context context) : base(context)
        {
            Initialize();
        }

        public MyCalendarView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _viewPager = new ViewPager2(Context)
            {
                OffscreenPageLimit = 1, // This means how many on either side, so 1 is actually 2 total offscreen views
                LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent)
            };
            _viewPager.RegisterOnPageChangeCallback(new PageChangeCallback(this));

            base.AddView(_viewPager);
        }

        private class PageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private MyCalendarView _view;
            public PageChangeCallback(MyCalendarView view)
            {
                _view = view;
            }

            public override void OnPageSelected(int position)
            {
                if (_view.Adapter != null)
                {
                    _view.DisplayMonthChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// You can set the DisplayMonth by assigning a new Adapter
        /// </summary>
        public DateTime DisplayMonth
        {
            get
            {
                if (Adapter == null)
                {
                    return DateTime.MinValue;
                }

                return Adapter.GetMonth(_viewPager.CurrentItem);
            }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get
            {
                return _selectedDate;
            }

            set
            {
                if ((_selectedDate == null && value == null)
                    || (_selectedDate != null && value != null && value.Value.Date == _selectedDate.Value.Date))
                {
                    return;
                }

                if (value != null)
                {
                    value = value.Value.Date;
                }
                _selectedDate = value;
                SelectedDateChanged?.Invoke(this, new EventArgs());
            }
        }

        private CalendarAdapter _adapter;
        public CalendarAdapter Adapter
        {
            get { return _adapter; }
            set
            {
                _adapter = value;

                if (value != null)
                {
                    _viewPager.Adapter = new MyCalendarPagerAdapter(value, this);
                    _viewPager.SetCurrentItem(1000, false);
                }

                else
                {
                    _viewPager.Adapter = null;
                }
            }
        }

        public abstract class CalendarAdapter
        {
            public DateTime CenterMonth { get; private set; }
            public DateTime FirstMonth { get; private set; }
            public readonly DayOfWeek FirstDayOfWeek;

            public CalendarAdapter(DateTime month, DayOfWeek firstDayOfWeek)
            {
                FirstDayOfWeek = firstDayOfWeek;
                month = DateTools.GetMonth(month);

                CenterMonth = month;
                FirstMonth = DateTools.GetMonth(month.AddMonths(-1000));
            }

            public int Count
            {
                get { return 2001; }
            }

            public abstract MyCalendarMonthView GetView(ViewGroup parent, MyCalendarView calendarView);

            public DateTime GetMonth(int position)
            {
                return DateTools.GetMonth(FirstMonth.AddMonths(position));
            }

            public int GetPosition(DateTime month)
            {
                return DateTools.DifferenceInMonths(month, FirstMonth);
            }
        }

        private class MyCalendarPagerAdapter : RecyclerView.Adapter
        {
            private CalendarAdapter _calendarAdapter;
            private MyCalendarView _calendarView;

            public MyCalendarPagerAdapter(CalendarAdapter calendarAdapter, MyCalendarView calendarView)
            {
                _calendarView = calendarView;
                _calendarAdapter = calendarAdapter;
            }

            public override int ItemCount => _calendarAdapter.Count;

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder.ItemView is MyCalendarMonthView monthView)
                {
                    monthView.Month = _calendarAdapter.GetMonth(position);
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                return new GenericRecyclerViewHolder(_calendarAdapter.GetView(parent, _calendarView));
            }
        }
    }
}