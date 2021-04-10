using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Views.CalendarViews
{
    public class CalendarView : VxComponent
    {
        private VxState<DateTime> _month = new VxState<DateTime>(DateTools.GetMonth(DateTime.Today));
        private VxState<int> _position = new VxState<int>(500);


        private DateTime[] _months;
        protected override void Initialize()
        {
            base.Initialize();

            var month = DateTools.GetMonth(DateTime.Today);

            _months = new DateTime[1000];
            int middle = 500;
            for (int i = 0; i < _months.Length; i++)
            {
                _months[i] = DateTools.GetMonth(month.AddMonths(i - middle));
            }
            //_months = new DateTime[]
            //{
            //    DateTools.GetMonth(DateTime.Today),
            //    DateTools.GetMonth(DateTime.Today.AddMonths(1)),
            //    DateTools.GetMonth(DateTime.Today.AddMonths(2)),
            //    DateTools.GetMonth(DateTime.Today.AddMonths(3)),
            //    DateTools.GetMonth(DateTime.Today.AddMonths(4))
            //};
        }

        protected override View Render()
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
                    new Label { Text = "Position: " + _position.Value, Margin = new Thickness(12) },
                    new CarouselView()
                    {
                        ItemsSource = _months,
                        Loop = false,
                        IsScrollAnimated = false,
                        ItemTemplate = CreateItemTemplate<DateTime>("monthTemplate", month =>
                        {
                            return new MonthView { Month = month };
                        })
                    }.BindPosition(_position)
                }
            };
        }
    }
}
