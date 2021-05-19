using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InterfacesUWP.CalendarFolder
{
    public class DefaultSmallCalendarSquare : TCalendarSquare
    {
        public DefaultSmallCalendarSquare(DefaultSmallCalendarGrid calendarGrid, DateTime date) : base(calendarGrid, date) { }

        private TextBlock _textBlock;

        private Grid _right, _left, _top, _bottom;
        
        private Grid _container;

        //private StackPanel _stackPanel;

        public void SetContent(FrameworkElement el)
        {
            Grid.SetColumn(el, 0);
            Grid.SetRow(el, 1);

            ClearItems();

            _container.Children.Add(el);
        }

        public FrameworkElement GetContent()
        {
            if (_container.Children.Count == 2)
                return _container.Children[2] as FrameworkElement;

            return null;
        }

        protected override void Render(DateTime date)
        {
            base.Margin = new Windows.UI.Xaml.Thickness(1);

            _container = new Grid();
            base.Children.Add(_container);

            _container.RowDefinitions.Add(new Windows.UI.Xaml.Controls.RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            _container.RowDefinitions.Add(new Windows.UI.Xaml.Controls.RowDefinition() { Height = GridLength.Auto });

            _container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            _container.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            _textBlock = new TextBlock()
            {
                Text = date.Day.ToString(),
                FontSize = 14,
                Margin = new Thickness(6),
                FontWeight = FontWeights.ExtraLight,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom
            };

            Grid.SetRow(_textBlock, 1);
            Grid.SetColumn(_textBlock, 1);

            _container.Children.Add(_textBlock);

            //_stackPanel = new StackPanel();

            //Grid.SetRow(_stackPanel, 1);
            //base.Children.Add(_stackPanel);


            _right = new Grid()
            {
                Background = CalendarView.SelectedBrush,
                Width = 2,
                Margin = new Thickness(0,-2,-2,-2),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                Visibility = Windows.UI.Xaml.Visibility.Collapsed
            };

            _top = new Grid()
            {
                Background = CalendarView.SelectedBrush,
                Height = 2,
                Margin = new Thickness(0,-2,0,0),
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                Visibility = Windows.UI.Xaml.Visibility.Collapsed
            };

            _bottom = new Grid()
            {
                Background = CalendarView.SelectedBrush,
                Height = 2,
                Margin = new Thickness(0,0,0,-2),
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom,
                Visibility = Windows.UI.Xaml.Visibility.Collapsed
            };

            _left = new Grid()
            {
                Background = CalendarView.SelectedBrush,
                Width = 2,
                Margin = new Thickness(-2,-2,0,-2),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                Visibility = Windows.UI.Xaml.Visibility.Collapsed
            };

            base.Children.Add(_left);
            base.Children.Add(_top);
            base.Children.Add(_right);
            base.Children.Add(_bottom);
        }

        private static SolidColorBrush makeBrush(byte val)
        {
            return new SolidColorBrush(Color.FromArgb(255, val, val, val));
        }

        private void setHighlightVisibility(Visibility vis)
        {
            _left.Visibility = vis;
            _top.Visibility = vis;
            _right.Visibility = vis;
            _bottom.Visibility = vis;
        }

        protected override void SetBackgroundColor(TCalendarSquare.DisplayType type, bool isMouseHovering, bool isMouseDown)
        {
            if (type == DisplayType.ThisMonthSelected || type == DisplayType.OtherMonthSelected || type == DisplayType.TodaySelected)
                setHighlightVisibility(Windows.UI.Xaml.Visibility.Visible);
            else
                setHighlightVisibility(Windows.UI.Xaml.Visibility.Collapsed);

            //default calendar does not have a "selected" mode, and also doesn't have a mouse hovering mode
            switch (type)
            {
                case DisplayType.ThisMonth:
                case DisplayType.ThisMonthSelected:

                    if (isMouseDown)
                        base.Background = CalendarView.SquareBackgroundThisMonthMouseDown;

                    else
                        base.Background = CalendarView.SquareBackgroundThisMonth;

                    break;

                case DisplayType.Today:
                case DisplayType.TodaySelected:

                    if (isMouseDown)
                        base.Background = CalendarView.SquareBackgroundTodayMouseDown;

                    else
                        base.Background = CalendarView.SquareBackgroundToday;

                    break;

                case DisplayType.OtherMonth:
                case DisplayType.OtherMonthSelected:

                    if (isMouseDown)
                        base.Background = CalendarView.SquareBackgroundOtherMonthMouseDown;

                    else
                        base.Background = CalendarView.SquareBackgroundOtherMonth;

                    break;
            }
        }

        public void SetBackgroundOverlayColor(SolidColorBrush brush)
        {
            if (brush != null)
            {
                Rectangle overlay = Children.FirstOrDefault() as Rectangle;
                if (overlay != null)
                {
                    overlay.Fill = brush;
                }
                else
                {
                    overlay = new Rectangle()
                    {
                        Fill = brush
                    };
                    Children.Insert(0, overlay);
                }
            }
            else
            {
                Rectangle overlay = Children.FirstOrDefault() as Rectangle;
                if (overlay != null)
                {
                    Children.RemoveAt(0);
                }
            }
        }

        protected override void SetForegroundColor(TCalendarSquare.DisplayType type, bool isMouseHovering, bool isMouseDown)
        {
            //default calendar's foreground always stays same except on Today
            if (type == DisplayType.Today || type == DisplayType.TodaySelected)
                _textBlock.Foreground = CalendarView.SquareForegroundToday;
            else
                _textBlock.Foreground = CalendarView.SquareForegroundNormal;
        }

        public override void InsertItem(int index, IRenderable el)
        {
            throw new NotImplementedException();
        }

        protected override UIElement removeItem(IRenderable el)
        {
            throw new NotImplementedException();
        }

        public override void ClearItems()
        {
            if (_container.Children.Count != 1 || !_container.Children.Contains(_textBlock))
            {
                _container.Children.Clear();
                _container.Children.Add(_textBlock);
            }
        }
    }
}
