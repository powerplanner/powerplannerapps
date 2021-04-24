using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;

namespace InterfacesiOS.Views.Calendar
{
    public class BareUICalendarView : BareUISlideView<BareUICalendarMonthView>
    {
        public event EventHandler<DateTime> DateClicked;
        public event EventHandler<DateTime> DisplayMonthChanged;

        ~BareUICalendarView()
        {
            System.Diagnostics.Debug.WriteLine("CalendarView disposed");
        }

        public BareUICalendarView(DayOfWeek firstDayOfWeek) : base(postponeInitialization: true)
        {
            _firstDayOfWeek = firstDayOfWeek;

            Initialize();

            foreach (var monthView in GetViews())
            {
                monthView.DateClicked += new WeakEventHandler<DateTime>(MonthView_DateClicked).Handler;
            }
        }

        private void MonthView_DateClicked(object sender, DateTime e)
        {
            DateClicked?.Invoke(this, e);
        }

        private DayOfWeek _firstDayOfWeek;
        public DayOfWeek FirstDayOfWeek
        {
            get => _firstDayOfWeek;
            set
            {
                if (_firstDayOfWeek != value)
                {
                    _firstDayOfWeek = value;
                    UpdateAllViews();
                }
            }
        }

        private DateTime _displayMonth;
        /// <summary>
        /// Assign the DisplayMonth to initialize the calendar
        /// </summary>
        public DateTime DisplayMonth
        {
            get { return _displayMonth; }
            set
            {
                var newValue = DateTools.GetMonth(value);
                if (_displayMonth == newValue)
                {
                    return;
                }

                _displayMonth = newValue;
                UpdateAllViews();
            }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if ((_selectedDate == null && value == null)
                    || (_selectedDate != null && value != null && _selectedDate.Value == value.Value.Date))
                {
                    return;
                }

                _selectedDate = value?.Date;
                UpdateAllViews();
            }
        }

        private BareUICalendarItemsSourceProvider _provider;
        public BareUICalendarItemsSourceProvider Provider
        {
            get { return _provider; }
            set
            {
                _provider = value;
                foreach (var monthView in GetViews())
                {
                    monthView.Provider = value;
                }
            }
        }

        public void UpdateAllBackgroundColors()
        {
            foreach (var monthView in GetViews())
            {
                monthView.UpdateAllBackgroundColors();
            }
        }

        protected override BareUICalendarMonthView CreateView()
        {
            return new BareUICalendarMonthView(FirstDayOfWeek);
        }

        protected override void OnMovedToNext()
        {
            _displayMonth = _displayMonth.AddMonths(1);
            DisplayMonthChanged?.Invoke(this, _displayMonth);
        }

        protected override void OnMovedToPrevious()
        {
            _displayMonth = _displayMonth.AddMonths(-1);
            DisplayMonthChanged?.Invoke(this, _displayMonth);
        }

        protected override void UpdateCurrView(BareUICalendarMonthView curr)
        {
            curr.Month = DisplayMonth;
            curr.SelectedDate = SelectedDate;
        }

        protected override void UpdateNextView(BareUICalendarMonthView next)
        {
            next.Month = DisplayMonth.AddMonths(1);
            next.SelectedDate = SelectedDate;
        }

        protected override void UpdatePrevView(BareUICalendarMonthView prev)
        {
            prev.Month = DisplayMonth.AddMonths(-1);
            prev.SelectedDate = SelectedDate;
        }
    }
}