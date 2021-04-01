using InterfacesUWP.CalendarFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace InterfacesUWP
{
    public class CalendarPicker : SlideView
    {
        private class CalendarGenerator : SlideViewContentGenerator
        {
            private DateTime? _selectedDate = DateTime.Today;
            private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            public CalendarGenerator() { }
            public CalendarGenerator(DateTime displayMonth, DateTime selectedDate) { _selectedDate = selectedDate; _displayMonth = new DateTime(displayMonth.Year, displayMonth.Month, 1); }

            public FrameworkElement GetCurrent()
            {
                return new CalendarGrid(_displayMonth, _selectedDate);
            }

            public void MoveNext()
            {
                try
                {
                    _displayMonth = _displayMonth.AddMonths(1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // TODO: Should disable moving next
                }
            }

            public void MovePrevious()
            {
                try
                {
                    _displayMonth = _displayMonth.AddMonths(-1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // TODO: Should disable moving previous
                }
            }

            public void SetSelectedDate(DateTime? date)
            {
                _selectedDate = date;
            }
        }

        public event EventHandler<EventArgsCalendar> SelectionChanged;

        private List<CalendarGrid> _calendars = new List<CalendarGrid>();

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
                foreach (var c in _calendars)
                    c.SelectedDate = value;

                (ContentGenerator as CalendarGenerator).SetSelectedDate(value);

                if (SelectionChanged != null)
                    SelectionChanged(this, new EventArgsCalendar(value));
            }
        }

        public CalendarPicker()
        {
            base.ContentGenerator = new CalendarGenerator();
        }

        /// <summary>
        /// Hide the content generator control.
        /// </summary>
        private new SlideViewContentGenerator ContentGenerator
        {
            get { return base.ContentGenerator; }
            set { base.ContentGenerator = value; }
        }

        protected override void OnRemoving(FrameworkElement el)
        {
            _calendars.Remove(el as CalendarGrid);
        }

        protected override void OnCreating(FrameworkElement el)
        {
            _calendars.Add(el as CalendarGrid);
        }

        protected override void OnChangedCurrent(FrameworkElement oldEl, FrameworkElement newEl)
        {
            if (oldEl != null)
                (oldEl as CalendarGrid).SelectionChanged -= CalendarPicker_SelectionChanged;

            if (newEl != null)
                (newEl as CalendarGrid).SelectionChanged += CalendarPicker_SelectionChanged;
        }

        void CalendarPicker_SelectionChanged(object sender, EventArgsCalendar e)
        {
            SelectedDate = e.SelectedDate;
        }
        
        private class CalendarGrid : Grid
        {
            public event EventHandler LeftClicked, RightClicked;

            public event EventHandler<EventArgsCalendar> SelectionChanged;

            private CalendarSquare currSelectedGrid;

            private DateTime month;
            public DateTime Month { get { return month; } }

            private DateTime? selectedDate;
            public DateTime? SelectedDate
            {
                get { return selectedDate; }

                set
                {
                    if (currSelectedGrid != null)
                        currSelectedGrid.Selected = false;

                    selectedDate = value;

                    if (selectedDate != null)
                        for (int i = 0; i < squares.Length; i++)
                            if (squares[i].Date.Date.Equals(selectedDate.Value.Date))
                            {
                                squares[i].Selected = true;
                                currSelectedGrid = squares[i];
                                break;
                            }
                }
            }

            private class CalendarSquare : Grid
            {
                protected virtual Brush ThisMonth { get { return new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)); } }
                protected virtual Brush ThisMonthMouse { get { return new SolidColorBrush(Colors.White); } }
                protected virtual Brush ThisMonthText { get { return new SolidColorBrush(Colors.Purple); } }
                protected virtual Brush ThisMonthTextMouse { get { return ThisMonthText; } }

                protected virtual Brush ThisMonthSelected { get { return new SolidColorBrush(Colors.White) { Opacity = 0.3 }; } }
                protected virtual Brush ThisMonthSelectedMouse { get { return new SolidColorBrush(Colors.White) { Opacity = 0.45 }; } }
                protected virtual Brush ThisMonthSelectedText { get { return new SolidColorBrush(Colors.White); } }
                protected virtual Brush ThisMonthSelectedTextMouse { get { return ThisMonthSelectedText; } }

                protected virtual Brush OtherMonth { get { return new SolidColorBrush(Color.FromArgb(255, 190, 190, 190)); } }
                protected virtual Brush OtherMonthMouse { get { return new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)); } }
                protected virtual Brush OtherMonthText { get { return ThisMonthText; } }
                protected virtual Brush OtherMonthTextMouse { get { return ThisMonthText; } }

                protected virtual Brush OtherMonthSelected { get { return new SolidColorBrush(Color.FromArgb(255, 190, 190, 190)) { Opacity = 0.65 }; } }
                protected virtual Brush OtherMonthSelectedMouse { get { return new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)) { Opacity = 0.7 }; } }
                protected virtual Brush OtherMonthSelectedText { get { return ThisMonthText; } }
                protected virtual Brush OtherMonthSelectedTextMouse { get { return ThisMonthText; } }

                protected virtual Brush Today { get { return new SolidColorBrush(Colors.White) { Opacity = 0.5 }; } }
                protected virtual Brush TodayMouse { get { return new SolidColorBrush(Colors.White) { Opacity = 0.65 }; } }
                protected virtual Brush TodayText { get { return ThisMonthText; } }
                protected virtual Brush TodayTextMouse { get { return TodayText; } }

                protected virtual Brush TodaySelected { get { return new SolidColorBrush(Colors.White) { Opacity = 0.6 }; } }
                protected virtual Brush TodaySelectedMouse { get { return new SolidColorBrush(Colors.White) { Opacity = 0.7 }; } }
                protected virtual Brush TodaySelectedText { get { return TodayText; } }
                protected virtual Brush TodaySelectedTextMouse { get { return TodayText; } }

                public enum DisplayType { ThisMonth, ThisMonthSelected, OtherMonth, OtherMonthSelected, Today, TodaySelected }

                private Brush normalBrush, mouseOverBrush, normalTextBrush, mouseOverTextBrush;

                public bool Selected
                {
                    set
                    {
                        if (value)
                        {
                            switch (type)
                            {
                                case DisplayType.OtherMonth:
                                    Type = DisplayType.OtherMonthSelected;
                                    break;

                                case DisplayType.ThisMonth:
                                    Type = DisplayType.ThisMonthSelected;
                                    break;

                                case DisplayType.Today:
                                    Type = DisplayType.TodaySelected;
                                    break;
                            }
                        }

                        else
                        {
                            switch (type)
                            {
                                case DisplayType.TodaySelected:
                                    Type = DisplayType.Today;
                                    break;

                                case DisplayType.ThisMonthSelected:
                                    Type = DisplayType.ThisMonth;
                                    break;

                                case DisplayType.OtherMonthSelected:
                                    Type = DisplayType.OtherMonth;
                                    break;
                            }
                        }
                    }
                }

                private DisplayType type;
                public DisplayType Type
                {
                    set
                    {
                        type = value;

                        switch (value)
                        {
                            case DisplayType.ThisMonth:
                                setColors(ThisMonth, ThisMonthMouse, ThisMonthText, ThisMonthTextMouse);
                                break;

                            case DisplayType.ThisMonthSelected:
                                setColors(ThisMonthSelected, ThisMonthSelectedMouse, ThisMonthSelectedText, ThisMonthSelectedTextMouse);
                                break;

                            case DisplayType.Today:
                                setColors(Today, TodayMouse, TodayText, TodayTextMouse);
                                break;

                            case DisplayType.TodaySelected:
                                setColors(TodaySelected, TodaySelectedMouse, TodaySelectedText, TodaySelectedTextMouse);
                                break;

                            case DisplayType.OtherMonth:
                                setColors(OtherMonth, OtherMonthMouse, OtherMonthText, OtherMonthTextMouse);
                                break;

                            case DisplayType.OtherMonthSelected:
                                setColors(OtherMonthSelected, OtherMonthSelectedMouse, OtherMonthSelectedText, OtherMonthSelectedTextMouse);
                                break;
                        }
                    }
                }

                private void setColors(Brush normal, Brush mouse, Brush normalText, Brush mouseText)
                {
                    normalBrush = normal;
                    mouseOverBrush = mouse;
                    normalTextBrush = normalText;
                    mouseOverTextBrush = mouseText;

                    if (isMouseIn)
                    {
                        base.Background = mouseOverBrush;
                        textBlockDayNum.Foreground = mouseOverTextBrush;
                    }

                    else
                    {
                        base.Background = normalBrush;
                        textBlockDayNum.Foreground = normalTextBrush;
                    }
                }

                private DateTime date;
                public DateTime Date { get { return date; } }

                private bool isMouseIn;
                private TextBlock textBlockDayNum;
                protected virtual TextBlock TextBlockDayNum
                {
                    get
                    {
                        return new TextBlock()
                            {
                                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom,
                                Margin = new Thickness(1)
                            };
                    }
                }

                public CalendarSquare(DateTime date, DisplayType type)
                {
                    this.date = date;
                    this.textBlockDayNum = TextBlockDayNum;

                    this.textBlockDayNum.Text = date.Day + "";
                    base.Children.Add(this.textBlockDayNum);

                    //will assign correct brushes
                    Type = type;

                    base.Margin = new Thickness(1);
                    base.PointerEntered += delegate { base.Background = mouseOverBrush; textBlockDayNum.Foreground = mouseOverTextBrush; isMouseIn = true; };
                    base.PointerExited += delegate { base.Background = normalBrush; textBlockDayNum.Foreground = normalTextBrush; isMouseIn = false; };
                }
            }

            protected virtual Grid GetGridForDay(DateTime day)
            {
                Grid g = new Grid();

                return g;
            }

            private CalendarSquare[] squares = new CalendarSquare[6 * 7];

            public CalendarGrid(DateTime month, DateTime? selectedDate)
            {
                this.month = month;

                Color backgroundColor = Colors.Purple;

                base.Background = new SolidColorBrush(backgroundColor);

                Grid inner = new Grid();

                for (int i = 0; i < 7; i++)
                    inner.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1 / 7.0, GridUnitType.Star) });

                inner.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                for (int i = 0; i < 6; i++)
                    inner.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1 / 6.0, GridUnitType.Star) });

                base.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                TextBlock textBlockHeader = new TextBlock()
                {
                    Text = month.ToString("MMMM yyyy"),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    Margin = new Thickness(0,4,0,4)
                };

                Grid headerBackground = new Grid() { Background = new SolidColorBrush(Colors.Purple) };
                Grid.SetColumn(headerBackground, 0);
                Grid.SetRow(headerBackground, 0);
                base.Children.Add(headerBackground);

                //Grid.SetColumn(textBlockHeader, 0);
                //Grid.SetRow(textBlockHeader, 0);
                //base.Children.Add(textBlockHeader);
                headerBackground.Children.Add(textBlockHeader);

                TextBlock left = new TextBlock()
                {
                    Text = "<",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 16, FontWeight = FontWeights.Bold,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    Margin = new Thickness(8,4,0,4)
                };

                left.Tapped += delegate
                {
                    if (LeftClicked != null)
                        LeftClicked(this, new EventArgs());
                };

                headerBackground.Children.Add(left);

                TextBlock right = new TextBlock()
                {
                    Text = ">",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 16, FontWeight = FontWeights.Bold,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    Margin = new Thickness(0,4,8,4)
                };

                right.Tapped += delegate
                {
                    if (RightClicked != null)
                        RightClicked(this, new EventArgs());
                };

                headerBackground.Children.Add(right);

                Grid.SetColumn(inner, 0);
                Grid.SetRow(inner, 1);
                base.Children.Add(inner);

                SolidColorBrush brushOtherMonth = new SolidColorBrush(Color.FromArgb(255, 190, 190, 190));
                SolidColorBrush brushOtherMonthMouse = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
                SolidColorBrush brushToday = new SolidColorBrush(Colors.White) { Opacity = 0.3 };
                SolidColorBrush brushTodayMouse = new SolidColorBrush(Colors.White) { Opacity = 0.5 };


                addDay("Sun", inner, 0);
                addDay("Mon", inner, 1);
                addDay("Tue", inner, 2);
                addDay("Wed", inner, 3);
                addDay("Thu", inner, 4);
                addDay("Fri", inner, 5);
                addDay("Sat", inner, 6);

                DateTime[,] array = CalendarArray.Generate(month, DayOfWeek.Sunday);

                int index = 0;
                for (int col = 0; col < 7; col++)
                    for (int row = 0; row < 6; row++)
                    {
                        CalendarSquare g = null;

                        if (array[row, col].Date == DateTime.Today)
                        {
                            if (selectedDate != null && array[row, col].Date == selectedDate.Value.Date)
                                g = new CalendarSquare(array[row, col], CalendarSquare.DisplayType.TodaySelected);
                            else
                                g = new CalendarSquare(array[row, col], CalendarSquare.DisplayType.Today);
                        }

                        else if (array[row, col].Month == month.Month)
                        {
                            if (selectedDate != null && array[row, col].Date == selectedDate.Value.Date)
                                g = new CalendarSquare(array[row, col], CalendarSquare.DisplayType.ThisMonthSelected);
                            else
                                g = new CalendarSquare(array[row, col], CalendarSquare.DisplayType.ThisMonth);
                        }

                        else
                        {
                            if (selectedDate != null && array[row, col].Date == selectedDate.Value.Date)
                                g = new CalendarSquare(array[row, col], CalendarSquare.DisplayType.OtherMonthSelected);
                            else
                                g = new CalendarSquare(array[row, col], CalendarSquare.DisplayType.OtherMonth);
                        }

                        decorateDayGrid(g, array[row, col]);

                        Grid.SetColumn(g, col);
                        Grid.SetRow(g, row + 1); //first row is Mon, Tues, Wed...
                        inner.Children.Add(g);

                        g.Tapped += delegate
                        {
                            if (currSelectedGrid != null)
                            {
                                currSelectedGrid.Selected = false;
                            }

                            currSelectedGrid = g;
                            g.Selected = true;

                            if (SelectionChanged != null)
                                SelectionChanged(this, new EventArgsCalendar(g.Date));
                        };

                        squares[index] = g;
                        index++;
                    }

                SelectedDate = selectedDate;
            }

            protected virtual void decorateDayGrid(Grid g, DateTime date)
            {

            }

            private void addDay(string day, Grid inner, int column)
            {
                Grid g = new Grid();
                g.Background = new SolidColorBrush(Color.FromArgb(200, 230, 230, 230));
                g.Margin = new Thickness(1);

                g.Children.Add(new TextBlock()
                {
                    Text = day,
                    FontSize = 15,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom,
                    Foreground = new SolidColorBrush(Colors.Purple),
                    Margin = new Thickness(0,6,0,0)
                });

                Grid.SetRow(g, 0);
                Grid.SetColumn(g, column);
                inner.Children.Add(g);
            }
        }
    }
}
