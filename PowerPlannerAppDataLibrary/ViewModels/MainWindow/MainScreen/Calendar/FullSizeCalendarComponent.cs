using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar
{
    public class FullSizeCalendarComponent : VxComponent
    {
        [VxSubscribe]
        private CalendarViewModel _viewModel;

        private DateTime _thisMonth;
        private Func<int, View> _itemTemplate;

        public FullSizeCalendarComponent(CalendarViewModel viewModel)
        {
            _viewModel = viewModel;
            _thisMonth = DateTools.GetMonth(DateTime.Today);
            _itemTemplate = RenderContent;
        }

        protected override View Render()
        {
            return new SlideView
            {
                Position = VxValue.Create(DateTools.DifferenceInMonths(_viewModel.DisplayMonth, _thisMonth), i => _viewModel.DisplayMonth = _thisMonth.AddMonths(i)),
                ItemTemplate = _itemTemplate
            };

            //return new SlideView
            //{
            //    CurrentContent = RenderContent(_viewModel.DisplayMonth),
            //    PreviousContent = RenderContent(_viewModel.DisplayMonth.AddMonths(-1)),
            //    NextContent = RenderContent(_viewModel.DisplayMonth.AddMonths(1)),
            //    OnMovedNext = () => _viewModel.DisplayMonth = _viewModel.DisplayMonth.AddMonths(1),
            //    OnMovedPrevious = () => _viewModel.DisplayMonth = _viewModel.DisplayMonth.AddMonths(-1)
            //};
        }

        private View RenderContent(int position)
        {
            var month = _thisMonth.AddMonths(position);

            return new FullSizeCalendarMonthComponent
            {
                Month = month,
                FirstDayOfWeek = _viewModel.FirstDayOfWeek,
                Items = _viewModel.SemesterItemsViewGroup.Items
            };
        }

        private class FullSizeCalendarMonthComponent : VxComponent
        {
            public DateTime Month { get; set; }
            public DayOfWeek FirstDayOfWeek { get; set; }
            public MyObservableList<BaseViewItemMegaItem> Items { get; set; }

            protected override View Render()
            {
                SubscribeToCollection(Items);

                DateTime[,] array = CalendarArray.Generate(Month, FirstDayOfWeek);

                var dayHeaders = new LinearLayout
                {
                    Orientation = Orientation.Horizontal
                };

                for (int i = 0; i < 7; i++)
                {
                    dayHeaders.Children.Add(new TextBlock
                    {
                        Text = DateTools.ToLocalizedString(FirstDayOfWeek + i)
                    }.LinearLayoutWeight(1));
                }

                var grid = new LinearLayout();

                for (int r = 0; r < 6; r++)
                {
                    var row = new LinearLayout
                    {
                        Orientation = Orientation.Horizontal
                    };

                    for (int c = 0; c < 7; c++)
                    {
                        var date = array[r, c];
                        DayType dayType;

                        if (DateTools.SameMonth(date, Month))
                        {
                            dayType = DayType.ThisMonth;
                        }
                        else if (date < Month)
                        {
                            dayType = DayType.PrevMonth;
                        }
                        else
                        {
                            dayType = DayType.NextMonth;
                        }

                        var itemsOnDay = Items.Where(i => i.Date.Date == date).ToArray();

                        row.Children.Add(new Border
                        {
                            Content = new TextBlock
                            {
                                Text = date.Day.ToString() + " - " + itemsOnDay.Length
                            }
                        }.LinearLayoutWeight(1));
                    }

                    grid.Children.Add(row.LinearLayoutWeight(1));
                }

                return new LinearLayout
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = Month.ToString(),
                            Margin = new Thickness(24)
                        }.TitleStyle(),

                        dayHeaders,

                        grid.LinearLayoutWeight(1)
                    }
                };
            }

            public enum DayType
            {
                ThisMonth,
                PrevMonth,
                NextMonth
            }
        }
    }
}
