using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
        }

        private View RenderContent(int position)
        {
            var month = _thisMonth.AddMonths(position);

            return new FullSizeCalendarMonthComponent
            {
                Month = month,
                Items = _viewModel.SemesterItemsViewGroup.Items,
                ViewModel = _viewModel
            };
        }

        private class FullSizeCalendarMonthComponent : VxComponent
        {
            public DateTime Month { get; set; }
            public MyObservableList<BaseViewItemMegaItem> Items { get; set; }

            // Note that subsequent properties on the ViewModel are NOT observed
            public CalendarViewModel ViewModel { get; set; }

            protected override void Initialize()
            {
                ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            }

            protected override View Render()
            {
                SubscribeToCollection(Items);

                DateTime[,] array = CalendarArray.Generate(Month, ViewModel.FirstDayOfWeek);

                var dayHeaders = new LinearLayout
                {
                    Orientation = Orientation.Horizontal
                };

                for (int i = 0; i < 7; i++)
                {
                    dayHeaders.Children.Add(new TextBlock
                    {
                        Text = DateTools.ToLocalizedString(ViewModel.FirstDayOfWeek + i),
                        WrapText = false,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        FontSize = 13,
                        Margin = new Thickness(12, 6, 12, 6)
                    }.LinearLayoutWeight(1));
                }

                var grid = new LinearLayout();

                for (int r = 0; r < 6; r++)
                {
                    grid.Children.Add(new Border
                    {
                        BackgroundColor = Theme.Current.SubtleForegroundColor,
                        Opacity = 0.3f,
                        Height = 1
                    });

                    var row = new LinearLayout
                    {
                        Orientation = Orientation.Horizontal
                    };

                    for (int c = 0; c < 7; c++)
                    {
                        var date = array[r, c];

                        row.Children.Add(RenderDay(date).LinearLayoutWeight(1));

                        if (c != 6)
                        {
                            row.Children.Add(new Border
                            {
                                BackgroundColor = Theme.Current.SubtleForegroundColor,
                                Opacity = 0.3f,
                                Width = 1
                            });
                        }
                    }

                    grid.Children.Add(row.LinearLayoutWeight(1));
                }

                return new LinearLayout
                {
                    BackgroundColor = Theme.Current.BackgroundAlt2Color,
                    Children =
                    {
                        dayHeaders,

                        grid.LinearLayoutWeight(1)
                    }
                };
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.ShowPastCompleteItemsOnFullCalendar):
                    case nameof(ViewModel.FirstDayOfWeek):
                        MarkDirty();
                        break;
                }
            }

            private View RenderDay(DateTime date)
            {
                bool isToday = date == ViewModel.Today;
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

                var itemsOnDay = TasksOrEventsOnDay.Get(ViewModel.MainScreenViewModel.CurrentAccount, Items, date, ViewModel.Today, activeOnly: !ViewModel.ShowPastCompleteItemsOnFullCalendar);
                var holidays = HolidaysOnDay.Create(Items, date);

                var tbDay = new TextBlock
                {
                    Text = date.Day.ToString(),
                    Margin = new Thickness(12),
                    FontSize = Theme.Current.SubtitleFontSize,
                    FontWeight = FontWeights.SemiLight,
                    TextColor = isToday ? Theme.Current.ForegroundColor.Invert() : Theme.Current.SubtleForegroundColor
                };

                var dayBackgroundColor = isToday ? Theme.Current.SubtleForegroundColor : dayType == DayType.ThisMonth ? Color.Transparent : Theme.Current.BackgroundAlt1Color;

                if (holidays.Any())
                {
                    dayBackgroundColor = dayBackgroundColor.Overlay(Color.Red, 0.3);
                }

                var linearLayout = new LinearLayout
                {
                    BackgroundColor = dayBackgroundColor,
                    Tapped = () => ViewModel.OpenDay(date),
                    Children =
                    {
                        holidays.Any() ? (View)new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                tbDay,

                                new TextBlock
                                {
                                    Text = holidays.First().Name,
                                    WrapText = false,
                                    FontSize = 10,
                                    TextColor = tbDay.TextColor
                                }.LinearLayoutWeight(1)
                            }
                        } : (View)tbDay
                    }
                };

                foreach (var item in itemsOnDay.OfType<ViewItemTaskOrEvent>())
                {
                    linearLayout.Children.Add(RenderDayItem(item));
                }

                return linearLayout;
            }

            private View RenderDayItem(ViewItemTaskOrEvent item)
            {
                View content = new Border
                {
                    BackgroundColor = item.Class.Color.ToColor(),
                    Content = new TextBlock
                    {
                        Text = item.Name,
                        Margin = new Thickness(6),
                        WrapText = false,
                        TextColor = item.IsComplete ? Color.LightGray : Color.White,
                        Strikethrough = item.IsComplete,
                        FontSize = Theme.Current.CaptionFontSize,
                        FontWeight = FontWeights.SemiBold
                    }
                };

                if (item.IsComplete)
                {
                    content = new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        BackgroundColor = item.Class.Color.ToColor(),
                        Opacity = 0.7f,
                        Children =
                        {
                            new Border
                            {
                                BackgroundColor = Color.Black,
                                Opacity = 0.3f,
                                Width = 12
                            },

                            content.LinearLayoutWeight(1)
                        }
                    };
                }

                content.Tapped = () => ViewModel.ShowItem(item);
                content.Margin = new Thickness(0, 0, 0, 3);

                return content;
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
