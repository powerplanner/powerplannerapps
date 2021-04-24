using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.CalendarFolder
{
    public abstract class TCalendarGrid : MyGrid
    {
        public event EventHandler<EventArgsCalendar> SelectionChanged;

        public DateTime DisplayMonth { get; private set; }

        public virtual bool AutoInitialize
        {
            get { return true; }
        }

        private Dictionary<DateTime, TCalendarSquare> _calendarSquares = new Dictionary<DateTime, TCalendarSquare>();

        public Dictionary<DateTime, TCalendarSquare>.Enumerator GetSquareEnumerator()
        {
            return _calendarSquares.GetEnumerator();
        }

        public TCalendarView CalendarView { get; private set; }

        public readonly DayOfWeek FirstDayOfWeek;

        public TCalendarGrid(TCalendarView calendarView, DateTime displayMonth)
        {
            CalendarView = calendarView;
            FirstDayOfWeek = calendarView.FirstDayOfWeek;

            displayMonth = new DateTime(displayMonth.Year, displayMonth.Month, 1);
            DisplayMonth = displayMonth;

            if (AutoInitialize)
                Initialize();
        }

        protected void Initialize()
        {
            base.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            base.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;

            Grid inner = new Grid();

            for (int i = 0; i < 7; i++)
                inner.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1 / 7.0, GridUnitType.Star) });

            inner.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            for (int i = 0; i < 6; i++)
                inner.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1 / 6.0, GridUnitType.Star) });

            base.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });


            //add header
            FrameworkElement header = GenerateCalendarHeader(DisplayMonth);
            Grid.SetRow(header, 0);
            base.Children.Add(header);


            //add grid for calendar
            Grid.SetRow(inner, 1);
            base.Children.Add(inner);


            //add the day headers
            DayOfWeek dayHeader = FirstDayOfWeek;
            for (int i = 0; i < 7; i++, dayHeader++)
            {
                addDay(dayHeader, inner, i);
            }


            //add the days
            DateTime[,] array = CalendarArray.Generate(DisplayMonth, FirstDayOfWeek);

            for (int col = 0; col < 7; col++)
                for (int row = 0; row < 6; row++)
                {
                    DateTime date = array[row, col];

                    TCalendarSquare square = GenerateCalendarSquare(date);

                    _calendarSquares[date] = square;

                    if (date.Date == DateTime.Today)
                        square.Type = TCalendarSquare.DisplayType.Today;

                    else if (date.Month == DisplayMonth.Month)
                        square.Type = TCalendarSquare.DisplayType.ThisMonth;

                    else
                        square.Type = TCalendarSquare.DisplayType.OtherMonth;


                    //add extra margin for side squares

                    //left
                    if (col == 0)
                    {
                        if (row == 5)
                            square.Margin = new Thickness(square.Margin.Left * 2, square.Margin.Top, square.Margin.Right, square.Margin.Bottom * 2);

                        else
                            square.Margin = new Thickness(square.Margin.Left * 2, square.Margin.Top, square.Margin.Right, square.Margin.Bottom);
                    }

                    //right
                    else if (col == 6)
                    {
                        if (row == 5)
                            square.Margin = new Thickness(square.Margin.Left, square.Margin.Top, square.Margin.Right * 2, square.Margin.Bottom * 2);

                        else
                            square.Margin = new Thickness(square.Margin.Left, square.Margin.Top, square.Margin.Right * 2, square.Margin.Bottom);
                    }

                    //bottom
                    else if (row == 5)
                        square.Margin = new Thickness(square.Margin.Left, square.Margin.Top, square.Margin.Right, square.Margin.Bottom * 2);


                    Grid.SetColumn(square, col);
                    Grid.SetRow(square, row + 1); //first row is Mon, Tues, Wed...
                    inner.Children.Add(square);

                    square.Tapped += square_Tapped;
                }



            FrameworkElement overlay = GenerateOverlay();
            if (overlay != null)
            {
                Grid.SetRowSpan(overlay, 2);
                base.Children.Add(overlay);
            }
        }

        void square_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SelectedDate = (sender as TCalendarSquare).Date;
        }

        private void addDay(DayOfWeek day, Grid inner, int column)
        {
            FrameworkElement el = GenerateDayHeader(day);

            //pad sides extra
            //if left
            if (column == 0)
                el.Margin = new Thickness(el.Margin.Left * 1.5, el.Margin.Top, el.Margin.Right, el.Margin.Bottom);
            else if (column == 6)
                el.Margin = new Thickness(el.Margin.Left, el.Margin.Top, el.Margin.Right * 2, el.Margin.Bottom);

            Grid.SetRow(el, 0);
            Grid.SetColumn(el, column);
            inner.Children.Add(el);
        }

        private bool ignoreDateChange;
        /// <summary>
        /// Sets the date without triggering events
        /// </summary>
        /// <param name="date"></param>
        public void SetSelectedDateSilent(DateTime? date)
        {
            ignoreDateChange = true;
            SelectedDate = date;
        }

        protected abstract FrameworkElement GenerateDayHeader(DayOfWeek dayOfWeek);

        protected abstract FrameworkElement GenerateCalendarHeader(DateTime displayMonth);

        protected abstract TCalendarSquare GenerateCalendarSquare(DateTime date);

        protected virtual FrameworkElement GenerateOverlay()
        {
            return null;
        }

        # region SelectedDate

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(TCalendarGrid), new PropertyMetadata(null, onSelectedDateChanged));

        private static void onSelectedDateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as TCalendarGrid).onSelectedDateChanged(args);
        }

        private void onSelectedDateChanged(DependencyPropertyChangedEventArgs args)
        {
            TCalendarSquare sqr;

            DateTime? oldSelectedDate = (DateTime?)args.OldValue;
            DateTime? newSelectedDate = (DateTime?)args.NewValue;

            if (oldSelectedDate != null)
                if (_calendarSquares.TryGetValue(oldSelectedDate.Value, out sqr))
                    sqr.IsSelected = false;

            if (newSelectedDate != null)
                if (_calendarSquares.TryGetValue(newSelectedDate.Value, out sqr))
                    sqr.IsSelected = true;


            if (ignoreDateChange)
            {
                ignoreDateChange = false;
                return;
            }

            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgsCalendar(newSelectedDate));
        }

        # endregion

        public void AddItem(IRenderable el, DateTime date)
        {
            InsertItem(int.MaxValue, el, date);
        }

        public void InsertItem(int index, IRenderable el, DateTime date)
        {
            date = date.Date;

            if (_calendarSquares.ContainsKey(date))
                _calendarSquares[date].InsertItem(index, el);
        }

        public void RemoveItem(IRenderable el)
        {
            foreach (var v in _calendarSquares)
                v.Value.RemoveItem(el);
        }

        public void ClearItems()
        {
            foreach (var v in _calendarSquares)
                v.Value.ClearItems();
        }

        /// <summary>
        /// Clears the items on a given date, returns true if the square was cleared.
        /// </summary>
        /// <param name="date"></param>
        public bool ClearItems(DateTime date)
        {
            if (_calendarSquares.ContainsKey(date.Date))
            {
                _calendarSquares[date.Date].ClearItems();
                return true;
            }

            return false;
        }

        public bool IsDisplayed(DateTime date)
        {
            return _calendarSquares.ContainsKey(date.Date);
        }

        /// <summary>
        /// Should check if IsDisplayed(date) is true first, otherwise an exception will be thrown
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public TCalendarSquare GetSquare(DateTime date)
        {
            return _calendarSquares[date.Date];
        }
    }
}
