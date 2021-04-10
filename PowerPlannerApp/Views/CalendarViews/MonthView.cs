using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Views.CalendarViews
{
    public class MonthView : VxComponent
    {
        public enum DayType
        {
            ThisMonth,
            Today,
            OtherMonth
        }

        public DateTime Month { get; set; } = DateTools.GetMonth(DateTime.Today);

        public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

        protected override View Render()
        {
            var grid = new Grid();

            for (int i = 0; i < 7; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1 / 7.0, GridUnitType.Star) });

            // Month name
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Day names
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Rows of days
            for (int i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1 / 6.0, GridUnitType.Star) });

            grid.Children.Add(RenderTitle().ColumnSpan(7));

            // Add the day headers
            DayOfWeek dayHeader = FirstDayOfWeek;
            for (int i = 0; i < 7; i++, dayHeader++)
            {
                grid.Children.Add(RenderDayHeader(dayHeader).Row(1).Column(i));
            }

            // Add the days
            DateTime[,] array = CalendarArray.Generate(Month, FirstDayOfWeek);
            for (int col = 0; col < 7; col++)
            {
                for (int row = 0; row < 6; row++)
                {
                    DateTime date = array[row, col];

                    var square = RenderDay(date, date.Date == DateTime.Today ? DayType.Today : date.Month == Month.Month ? DayType.ThisMonth : DayType.OtherMonth);


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
                    Grid.SetRow(square, row + 2); //first row is Month, then Mon, Tues, Wed...
                    grid.Children.Add(square);
                }
            }

            return grid;
        }

        protected virtual View RenderTitle()
        {
            return new Label
            {
                Text = Month.ToString("MMMM yyyy"),
                FontSize = 40,
                Margin = new Thickness(60, 6, 48, 6)
            };
        }

        protected virtual View RenderDayHeader(DayOfWeek dayOfWeek)
        {
            return new Label
            {
                Text = DateTools.ToLocalizedString(dayOfWeek),
                FontSize = 16,
                Margin = new Thickness(14, 2, 14, 2)
            };
        }

        protected virtual View RenderDay(DateTime date, DayType dayType)
        {
            return new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },

                Children =
                {
                    new Label
                    {
                        Text = date.Day.ToString(),
                        FontSize = 20,
                        Margin = new Thickness(8)
                    }
                }
            };
        }
    }
}
