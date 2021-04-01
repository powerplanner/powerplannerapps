using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using ToolsPortable;
using InterfacesDroid.Themes;

namespace InterfacesDroid.Views
{
    public interface IMyCalendarDayView
    {
        void UpdateDay(DateTime date, MyCalendarMonthView.DayType dayType, bool isToday);
    }

    public class MyCalendarMonthView : FrameLayout
    {
        private LinearLayout _linearLayoutContent;
        private LinearLayout _tableLayoutDays;
        private TextView _title;
        private LinearLayout _linearLayoutDayHeaders;
        public MyCalendarView CalendarView { get; private set; }
        public readonly DayOfWeek FirstDayOfWeek;

        public MyCalendarMonthView(ViewGroup parent, MyCalendarView calendarView, DayOfWeek firstDayOfWeek) : base(parent.Context)
        {
            FirstDayOfWeek = firstDayOfWeek;
            CalendarView = calendarView;
            Initialize();
        }

        protected virtual void Initialize()
        {
            _linearLayoutContent = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical
            };
            base.AddView(_linearLayoutContent);

            var titleView = CreateTitle();
            if (titleView != null)
            {
                _linearLayoutContent.AddView(titleView);
            }
            _linearLayoutContent.AddView(CreateDayHeaders());

            _tableLayoutDays = new LinearLayout(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    0)
                {
                    Weight = 1
                },
                Orientation = Orientation.Vertical
            };

            for (int row = 0; row < 6; row++)
            {
                LinearLayout tableRow = new LinearLayout(Context)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.MatchParent,
                        0)
                    {
                        Weight = 1
                    }
                };

                for (int col = 0; col < 7; col++)
                {
                    View dayView = CreateDay();

                    if (!(dayView is IMyCalendarDayView))
                    {
                        throw new InvalidOperationException("CreateDay must return a View that implements IMyCalendarDayView");
                    }

                    LinearLayout.LayoutParams lp;
                    if (!(dayView.LayoutParameters is LinearLayout.LayoutParams))
                    {
                        dayView.LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent);
                    }
                    lp = dayView.LayoutParameters as LinearLayout.LayoutParams;
                    lp.Width = 0;
                    lp.Height = LinearLayout.LayoutParams.MatchParent;
                    lp.Weight = 1;

                    tableRow.AddView(dayView);
                }

                _tableLayoutDays.AddView(tableRow);
            }

            _linearLayoutContent.AddView(_tableLayoutDays);
        }

        /// <summary>
        /// A view that's overlayed on top of the entire month
        /// </summary>
        public View OverlayView
        {
            get { return base.ChildCount > 1 ? base.GetChildAt(1) : null; }
            set
            {
                while (base.ChildCount > 1)
                {
                    base.RemoveViewAt(1);
                }

                if (value != null)
                {
                    base.AddView(value);
                }
            }
        }

        private DateTime _month;
        public DateTime Month
        {
            get { return _month; }
            set
            {
                value = DateTools.GetMonth(value);

                if (_month == value)
                {
                    return;
                }

                _month = value;

                OnMonthChanged();
            }
        }

        protected virtual void OnMonthChanged()
        {
            if (_title != null)
                _title.Text = Month.ToString("MMMM yyyy");
            
            if (_tableLayoutDays != null)
            {
                UpdateTableLayoutDays();
            }
        }

        private void UpdateTableLayoutDays()
        {
            DateTime[,] array = CalendarArray.Generate(Month, FirstDayOfWeek);

            for (int row = 0; row < 6; row++)
            {
                LinearLayout tableRow = (LinearLayout)_tableLayoutDays.GetChildAt(row);

                for (int col = 0; col < 7; col++)
                {
                    DateTime date = array[row, col];
                    DayType dayType;

                    if (DateTools.SameMonth(date, Month))
                    {
                        dayType = DayType.ThisMonth;
                    }
                    else if (date < Month)
                    {
                        dayType = DayType.PrevMonth;
                    }
                    else
                    {
                        dayType = DayType.NextMonth;
                    }

                    IMyCalendarDayView dayView = (IMyCalendarDayView)tableRow.GetChildAt(col);
                    dayView.UpdateDay(date, dayType, date.Date == DateTime.Today);
                }
            }
        }

        protected virtual View CreateTitle()
        {
            _title = new TextView(Context);
            return _title;
        }

        public enum DayType
        {
            ThisMonth,
            PrevMonth,
            NextMonth
        }

        /// <summary>
        /// Your returned view should implement <see cref="IMyCalendarDayView"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual View CreateDay()
        {
            return new MyBasicDayView(Context);
        }

        private class MyBasicDayView : TextView, IMyCalendarDayView
        {
            public MyBasicDayView(Context context) : base(context) { }

            public void UpdateDay(DateTime date, DayType dayType, bool isToday)
            {
                Text = date.Day.ToString();
            }
        }

        protected virtual View CreateDayHeaders()
        {
            _linearLayoutDayHeaders = new LinearLayout(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    LinearLayout.LayoutParams.WrapContent)
            };

            DateTime date = DateTools.Last(FirstDayOfWeek);

            for (int i = 0; i < 7; i++, date = date.AddDays(1))
            {
                TextView dayHeader = new TextView(Context)
                {
                    Text = date.ToString("ddd"),
                    LayoutParameters = new LinearLayout.LayoutParams(
                        0,
                        LinearLayout.LayoutParams.WrapContent)
                    {
                        Weight = 1
                    },
                    Gravity = GravityFlags.Right
                };

                dayHeader.SetPadding(0, 0, ThemeHelper.AsPx(Context, 4), 0);

                _linearLayoutDayHeaders.AddView(dayHeader);
            }

            return _linearLayoutDayHeaders;
        }
    }
}