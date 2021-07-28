using InterfacesUWP.ArrowButtonFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace InterfacesUWP.CalendarFolder
{
    public class DefaultSmallCalendarGrid : TCalendarGrid
    {
        private ArrowButtonBase _rightButton;
        private ArrowButtonBase _leftButton;

        private TextBlock _header;

        public DefaultSmallCalendarGrid(TCalendarView calendarView, DateTime displayMonth, bool isMouseOver)
            : base(calendarView, displayMonth)
        {
            base.Background = CalendarView.GridBackground;

            if (isMouseOver)
                ShowArrows();
        }

        protected override FrameworkElement GenerateDayHeader(DayOfWeek dayOfWeek)
        {
            Grid grid = new Grid()
            {
                Background = CalendarView.GridDayHeaderBackground,
                Margin = new Thickness(0,0,0,1)
            };

            grid.Children.Add(new TextBlock()
            {
                Text = DateTools.Last(dayOfWeek).ToString("ddd"),
                Foreground = CalendarView.GridDayHeaderForeground,
                Margin = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
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

            _header = new TextBlock()
            {
                Text = displayMonth.ToString("MMMM yyyy"),
                FontSize = 24,
                Foreground = CalendarView.GridCalendarHeaderForeground,
                Margin = new Thickness(24, 12, 24, 6),
                FontWeight = FontWeights.ExtraLight
            };

            grid.Children.Add(_header);

            _rightButton = new ArrowButtonBase()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed,
                Character = ">"
            };
            _rightButton.Click += rightButton_Click;
            grid.Children.Add(_rightButton);

            _leftButton = new ArrowButtonBase()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed,
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
            return new DefaultSmallCalendarSquare(this, date);
        }

        public void ShowArrows()
        {
            _rightButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            _leftButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

            _header.Margin = new Thickness(60, 12, 24, 6);
        }

        public void HideArrows()
        {
            _rightButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            _leftButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            _header.Margin = new Thickness(24, 12, 24, 6);
        }
    }
}
