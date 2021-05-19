using InterfacesUWP.ArrowButtonFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.CalendarFolder
{
    public class DefaultCalendarGrid : TCalendarGrid
    {
        private ArrowButtonBase _rightButton;
        private ArrowButtonBase _leftButton;

        public DefaultCalendarGrid(TCalendarView calendarView, DateTime displayMonth, bool isMouseOver)
            : base(calendarView, displayMonth)
        {
            base.Background = CalendarView.GridBackground;

            if (isMouseOver)
                ShowArrows();
        }

        public void ShowArrows()
        {
            _rightButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            _leftButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public void HideArrows()
        {
            _rightButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _leftButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        protected override FrameworkElement GenerateDayHeader(DayOfWeek dayOfWeek)
        {
            Grid grid = new Grid()
            {
                Background = CalendarView.GridDayHeaderBackground,
                Margin = new Thickness(1.5,0,1.5,1.5)
            };

            grid.Children.Add(new TextBlock()
            {
                Text = DateTools.ToLocalizedString(dayOfWeek),
                Foreground = CalendarView.GridDayHeaderForeground,
                Margin = new Thickness(14, 2, 14, 2),
                FontSize = 16
            });

            return grid;
        }

        protected override FrameworkElement GenerateCalendarHeader(DateTime displayMonth)
        {
            Grid grid = new Grid()
            {
                Background = CalendarView.GridCalendarHeaderBackground
            };

            grid.Children.Add(new TextBlock()
            {
                Text = displayMonth.ToString("MMMM yyyy"),
                FontSize = 58,
                Foreground = CalendarView.GridCalendarHeaderForeground,
                Margin = new Thickness(60, 12, 48, 6),
                FontWeight = FontWeights.ExtraLight
            });

            _rightButton = new ArrowButtonBase()
            {
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                Margin = new Thickness(0, 24, 0, 0),
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
                Character = ">"
            };
            _rightButton.Click += rightButton_Click;
            grid.Children.Add(_rightButton);

            _leftButton = new ArrowButtonBase()
            {
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                Margin = new Thickness(0, 24, 0, 0),
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
                Character = "<"
            };
            _leftButton.Click += leftButton_Click;
            grid.Children.Add(_leftButton);

            return grid;
        }

        void leftButton_Click(object sender, EventArgs e)
        {
            CalendarView.Previous();
        }

        void rightButton_Click(object sender, EventArgs e)
        {
            CalendarView.Next();
        }

        protected override TCalendarSquare GenerateCalendarSquare(DateTime date)
        {
            return new DefaultCalendarSquare(this, date);
        }
    }
}
