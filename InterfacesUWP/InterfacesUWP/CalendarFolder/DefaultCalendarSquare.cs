using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.CalendarFolder
{
    public class DefaultCalendarSquare : TCalendarSquare
    {
        public DefaultCalendarSquare(DefaultCalendarGrid calendarGrid, DateTime date) : base(calendarGrid, date) { }

        private TextBlock _textBlock;

        private StackPanel _stackPanel;

        protected override void Render(DateTime date)
        {
            base.Margin = new Windows.UI.Xaml.Thickness(1.5);

            base.RowDefinitions.Add(new Windows.UI.Xaml.Controls.RowDefinition() { Height = GridLength.Auto });
            base.RowDefinitions.Add(new Windows.UI.Xaml.Controls.RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            _textBlock = new TextBlock()
            {
                Text = date.Day.ToString(),
                FontSize = 22,
                Margin = new Thickness(14),
                FontWeight = FontWeights.ExtraLight
            };

            Grid.SetRow(_textBlock, 0);

            base.Children.Add(_textBlock);

            _stackPanel = new StackPanel();

            Grid.SetRow(_stackPanel, 1);
            base.Children.Add(_stackPanel);
        }

        private static SolidColorBrush makeBrush(byte val)
        {
            return new SolidColorBrush(Color.FromArgb(255, val, val, val));
        }

        protected override void SetBackgroundColor(TCalendarSquare.DisplayType type, bool isMouseHovering, bool isMouseDown)
        {
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
            if (index > _stackPanel.Children.Count)
                index = _stackPanel.Children.Count;
            else if (index < 0)
                index = 0;

            _stackPanel.Children.Insert(index, UIGenerator.Render(el));
        }

        protected override UIElement removeItem(IRenderable el)
        {
            List<UIElement> list = UIGenerator.GetRenderedElements(el);
            for (int i = 0; i < list.Count; i++)
            {
                if (_stackPanel.Children.Remove(list[i]))
                    return list[i];
            }

            return null;
        }

        public override void ClearItems()
        {
            _stackPanel.Children.Clear();
        }
    }
}
