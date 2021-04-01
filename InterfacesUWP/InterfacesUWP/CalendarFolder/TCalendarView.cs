using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.CalendarFolder
{
    public abstract class TCalendarView : SlideView
    {
        private class CalendarGenerator : SlideViewContentGenerator
        {
            private DateTime? _selectedDate;
            private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            private DayOfWeek _firstDayOfWeek;

            private TCalendarView _picker;

            public CalendarGenerator(TCalendarView picker, DateTime displayMonth, DateTime? selectedDate, DayOfWeek firstDayOfWeek)
            {
                _picker = picker;
                _selectedDate = selectedDate;
                _displayMonth = new DateTime(displayMonth.Year, displayMonth.Month, 1);
                _firstDayOfWeek = firstDayOfWeek;
            }

            public FrameworkElement GetCurrent()
            {
                TCalendarGrid grid = _picker.GenerateCalendarGrid(_displayMonth);
                _picker.GenerateEventsOnGrid(grid, _displayMonth);
                grid.SelectedDate = _selectedDate;

                return grid;
            }

            public DateTime GetDisplayMonth()
            {
                return DateTools.GetMonth(_displayMonth); //just in case, use the GetMonth method
            }

            public void MoveNext()
            {
                _displayMonth = _displayMonth.AddMonths(1);
            }

            public void MovePrevious()
            {
                _displayMonth = _displayMonth.AddMonths(-1);
            }

            public void SetSelectedDate(DateTime? date)
            {
                _selectedDate = date;
            }
        }

        public event EventHandler<EventArgsCalendar> SelectionChanged;
        public event EventHandler<EventArgs> DisplayMonthChanged;



        public DayOfWeek FirstDayOfWeek
        {
            get { return (DayOfWeek)GetValue(FirstDayOfWeekProperty); }
            set { SetValue(FirstDayOfWeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstDayOfWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstDayOfWeekProperty =
            DependencyProperty.Register("FirstDayOfWeek", typeof(DayOfWeek), typeof(TCalendarView), new PropertyMetadata(CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek, OnFirstDayOfWeekChanged));

        private static void OnFirstDayOfWeekChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TCalendarView).OnFirstDayOfWeekChanged(e);
        }

        private void OnFirstDayOfWeekChanged(DependencyPropertyChangedEventArgs e)
        {
            base.ContentGenerator = new CalendarGenerator(this, DisplayMonth, SelectedDate, FirstDayOfWeek);
        }

        protected List<TCalendarGrid> _calendars = new List<TCalendarGrid>();





        public Brush SquareBackgroundThisMonth
        {
            get { return (Brush)GetValue(SquareBackgroundThisMonthProperty); }
            set { SetValue(SquareBackgroundThisMonthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareBackgroundThisMonth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareBackgroundThisMonthProperty =
            DependencyProperty.Register("SquareBackgroundThisMonth", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SquareBackgroundThisMonthMouseDown
        {
            get { return (Brush)GetValue(SquareBackgroundThisMonthMouseDownProperty); }
            set { SetValue(SquareBackgroundThisMonthMouseDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareBackgroundThisMonthMouseDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareBackgroundThisMonthMouseDownProperty =
            DependencyProperty.Register("SquareBackgroundThisMonthMouseDown", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));





        public Brush SquareBackgroundToday
        {
            get { return (Brush)GetValue(SquareBackgroundTodayProperty); }
            set { SetValue(SquareBackgroundTodayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareBackgroundToday.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareBackgroundTodayProperty =
            DependencyProperty.Register("SquareBackgroundToday", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SquareBackgroundTodayMouseDown
        {
            get { return (Brush)GetValue(SquareBackgroundTodayMouseDownProperty); }
            set { SetValue(SquareBackgroundTodayMouseDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareBackgroundTodayMouseDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareBackgroundTodayMouseDownProperty =
            DependencyProperty.Register("SquareBackgroundTodayMouseDown", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SquareBackgroundOtherMonth
        {
            get { return (Brush)GetValue(SquareBackgroundOtherMonthProperty); }
            set { SetValue(SquareBackgroundOtherMonthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareBackgroundOtherMonth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareBackgroundOtherMonthProperty =
            DependencyProperty.Register("SquareBackgroundOtherMonth", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SquareBackgroundOtherMonthMouseDown
        {
            get { return (Brush)GetValue(SquareBackgroundOtherMonthMouseDownProperty); }
            set { SetValue(SquareBackgroundOtherMonthMouseDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareBackgroundOtherMonthMouseDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareBackgroundOtherMonthMouseDownProperty =
            DependencyProperty.Register("SquareBackgroundOtherMonthMouseDown", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SquareForegroundNormal
        {
            get { return (Brush)GetValue(SquareForegroundNormalProperty); }
            set { SetValue(SquareForegroundNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareForegroundNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareForegroundNormalProperty =
            DependencyProperty.Register("SquareForegroundNormal", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SquareForegroundToday
        {
            get { return (Brush)GetValue(SquareForegroundTodayProperty); }
            set { SetValue(SquareForegroundTodayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareForegroundToday.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareForegroundTodayProperty =
            DependencyProperty.Register("SquareForegroundToday", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush GridBackground
        {
            get { return (Brush)GetValue(GridBackgroundProperty); }
            set { SetValue(GridBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridBackgroundProperty =
            DependencyProperty.Register("GridBackground", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));




        public Brush GridDayHeaderBackground
        {
            get { return (Brush)GetValue(GridDayHeaderBackgroundProperty); }
            set { SetValue(GridDayHeaderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridDayHeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridDayHeaderBackgroundProperty =
            DependencyProperty.Register("GridDayHeaderBackground", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));



        public Brush GridDayHeaderForeground
        {
            get { return (Brush)GetValue(GridDayHeaderForegroundProperty); }
            set { SetValue(GridDayHeaderForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridDayHeaderForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridDayHeaderForegroundProperty =
            DependencyProperty.Register("GridDayHeaderForeground", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));





        public Brush GridCalendarHeaderBackground
        {
            get { return (Brush)GetValue(GridCalendarHeaderBackgroundProperty); }
            set { SetValue(GridCalendarHeaderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridCalendarHeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridCalendarHeaderBackgroundProperty =
            DependencyProperty.Register("GridCalendarHeaderBackground", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));



        public Brush GridCalendarHeaderForeground
        {
            get { return (Brush)GetValue(GridCalendarHeaderForegroundProperty); }
            set { SetValue(GridCalendarHeaderForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridCalendarHeaderForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridCalendarHeaderForegroundProperty =
            DependencyProperty.Register("GridCalendarHeaderForeground", typeof(Brush), typeof(TCalendarView), new PropertyMetadata(null));








        public virtual bool AutoInitialize
        {
            get { return true; }
        }

        public TCalendarView()
        {
            base.DefaultStyleKey = typeof(TCalendarView);
        }

        protected override void OnApplyTemplate()
        {
            if (AutoInitialize)
                Initialize();
        }

        protected void Initialize()
        {
            base.ContentGenerator = new CalendarGenerator(this, DisplayMonth, SelectedDate, FirstDayOfWeek);
        }

        /// <summary>
        /// Hide the content generator control.
        /// </summary>
        private new SlideViewContentGenerator ContentGenerator
        {
            get { return base.ContentGenerator; }
        }

        protected override void OnRemoving(FrameworkElement el)
        {
            _calendars.Remove(el as TCalendarGrid);
        }

        protected override void OnCreating(FrameworkElement el)
        {
            _calendars.Add(el as TCalendarGrid);
        }

        protected override void OnChangedCurrent(FrameworkElement oldEl, FrameworkElement newEl)
        {
            if (oldEl != null)
                (oldEl as TCalendarGrid).SelectionChanged -= CalendarPicker_SelectionChanged;

            if (newEl != null)
                (newEl as TCalendarGrid).SelectionChanged += CalendarPicker_SelectionChanged;

            DisplayMonthChanged?.Invoke(this, new EventArgs());
            DisplayMonth = this.GetCurrentDisplayMonth();
        }

        void CalendarPicker_SelectionChanged(object sender, EventArgsCalendar e)
        {
            SelectedDate = e.SelectedDate;
        }

        private DateTime? lastDate;
        /// <summary>
        /// Gets or sets the selected date. The default is today.
        /// </summary>
        public DateTime? SelectedDate
        {
            get
            {
                if (_calendars.Count == 0)
                    return DateTime.Today;

                return _calendars.First().SelectedDate;
            }

            set
            {
                if (object.Equals(value, lastDate))
                    return;

                if (value != null)
                    value = DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Unspecified);

                lastDate = value;

                foreach (var c in _calendars)
                    c.SetSelectedDateSilent(value);

                if (ContentGenerator is CalendarGenerator)
                {
                    (ContentGenerator as CalendarGenerator).SetSelectedDate(value);

                    DateTime displayMonth = (ContentGenerator as CalendarGenerator).GetDisplayMonth();


                    if (value != null)
                    {
                        bool selectedIsVisible = false;

                        foreach (var c in _calendars)
                            if (DateTools.SameMonth(displayMonth, c.DisplayMonth) && c.IsDisplayed(value.Value))
                            {
                                selectedIsVisible = true;
                                break;
                            }

                        if (!selectedIsVisible)
                        {
                            base.ContentGenerator = new CalendarGenerator(this, DateTools.GetMonth(value.Value), value, FirstDayOfWeek);
                        }
                    }
                }

                if (SelectionChanged != null)
                    SelectionChanged(this, new EventArgsCalendar(value));
            }
        }



        public DateTime DisplayMonth
        {
            get { return (DateTime)GetValue(DisplayMonthProperty); }
            set { SetValue(DisplayMonthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMonth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMonthProperty =
            DependencyProperty.Register("DisplayMonth", typeof(DateTime), typeof(TCalendarView), new PropertyMetadata(DateTools.GetMonth(DateTime.Today), OnDisplayMonthChanged));

        private static void OnDisplayMonthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TCalendarView).OnDisplayMonthChanged(e);
        }

        private void OnDisplayMonthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!DateTools.SameMonth(GetCurrentDisplayMonth(), DisplayMonth))
            {
                base.ContentGenerator = new CalendarGenerator(this, DisplayMonth, SelectedDate, FirstDayOfWeek);
            }
        }

        private DateTime GetCurrentDisplayMonth()
        {
            if (ContentGenerator == null)
                return DateTools.GetMonth(DateTime.Today);

            return (ContentGenerator as CalendarGenerator).GetDisplayMonth();
        }

        protected abstract void GenerateEventsOnGrid(TCalendarGrid grid, DateTime displayMonth);

        protected abstract TCalendarGrid GenerateCalendarGrid(DateTime displayMonth);

        public void AddItem(IRenderable el, DateTime date)
        {
            InsertItem(int.MaxValue, el, date);
        }

        public void InsertItem(int index, IRenderable el, DateTime date)
        {
            DateTime month = new DateTime(date.Year, date.Month, 1);

            foreach (TCalendarGrid g in _calendars)
                if (g.IsDisplayed(date))
                    g.InsertItem(index, el, date);
        }

        public void RemoveItem(IRenderable el)
        {
            foreach (TCalendarGrid g in _calendars)
                g.RemoveItem(el);
        }

        public void ClearItems()
        {
            foreach (TCalendarGrid g in _calendars)
                g.ClearItems();
        }

        /// <summary>
        /// Returns true if the date is currently displayed somewhere in the calendar (including on the sides)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsDisplayed(DateTime date)
        {
            foreach (TCalendarGrid g in _calendars)
                if (g.IsDisplayed(date))
                    return true;

            return false;
        }

        public bool ClearItems(DateTime date)
        {
            bool cleared = false;

            foreach (TCalendarGrid g in _calendars)
                if (g.ClearItems(date))
                    cleared = true;

            return cleared;
        }

        public List<TCalendarSquare> GetSquares(DateTime date)
        {
            List<TCalendarSquare> list = new List<TCalendarSquare>();

            foreach (TCalendarGrid g in _calendars)
                if (g.IsDisplayed(date))
                    list.Add(g.GetSquare(date));

            return list;
        }
    }
}
