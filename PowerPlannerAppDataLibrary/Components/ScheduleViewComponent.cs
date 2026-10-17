using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using System;
using System.Drawing;
using System.Linq;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public partial class ScheduleViewComponent : VxComponent
    {
        private const float COL_WIDTH = 200;
        private const float COL_PADDING = 8;
        private const float INITIAL_MARGIN = 6;
        private const float HEIGHT_OF_HOUR = 80;
        private const float TIMES_COLUMN_WIDTH = 50;
        private const float DAYS_ROW_HEIGHT = 32;
        private const float ALL_DAY_ITEM_HEIGHT = 30;
        private const float ALL_DAY_ITEM_SPACING = 1;

        private bool _arrangersInitialized;

        public ScheduleViewModel ViewModel { get; set; }

        public Thickness NookInsets { get; set; }

        protected override View Render()
        {
            Subscribe(ViewModel);

            if (!_arrangersInitialized)
            {
                ViewModel.InitializeArrangers(HEIGHT_OF_HOUR, ScheduleItemComponent.SPACING_WITH_NO_ADDITIONAL, ScheduleItemComponent.SPACING_WITH_ADDITIONAL, ScheduleItemComponent.WIDTH_OF_COLLAPSED_ITEM);
                _arrangersInitialized = true;
            }

            var frame = new FrameLayout
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            DateTime today = DateTime.Today;
            DateTime startTime = today.Add(ViewModel.StartTime);
            DateTime endTime = today.Add(ViewModel.EndTime);

            int maxAllDayItems = 0;
            for (int i = 0; i < 7; i++)
            {
                maxAllDayItems = Math.Max(maxAllDayItems, ViewModel.Items[ViewModel.StartDate.AddDays(i).DayOfWeek].HolidayAndAllDayItems.Count);
            }
            float allDayHeight = maxAllDayItems * (ALL_DAY_ITEM_HEIGHT + ALL_DAY_ITEM_SPACING);
            float scheduleTop = DAYS_ROW_HEIGHT + allDayHeight;

            float contentHeight = INITIAL_MARGIN + ((endTime - startTime).Hours + 1) * HEIGHT_OF_HOUR + 55;

            for (DateTime time = startTime; endTime >= time; time = time.AddHours(1))
            {
                frame.Children.Add(new TextBlock
                {
                    Text = DateTimeFormatterExtension.Current.FormatAsShortTimeWithoutAmPm(time),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = TIMES_COLUMN_WIDTH,
                    Margin = new Thickness(4, scheduleTop + GetTopMargin(time.TimeOfDay, startTime.TimeOfDay), 0, 0)
                });
            }

            int visibleCol = 0;
            for (int i = 0; i < 7; i++)
            {
                DateTime date = ViewModel.StartDate.AddDays(i);
                var arranger = ViewModel.Items[date.DayOfWeek];

                Subscribe(arranger);

                bool hideDay = !arranger.HolidayAndAllDayItems.Any() && !arranger.EventItems.Any() && !arranger.ScheduleItems.Any()
                    && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
                if (hideDay)
                {
                    continue;
                }

                float left = TIMES_COLUMN_WIDTH + visibleCol * COL_WIDTH;
                visibleCol++;

                if (visibleCol % 2 == 0)
                {
                    frame.Children.Add(new Border
                    {
                        BackgroundColor = Theme.Current.BackgroundAlt1Color,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = COL_WIDTH,
                        Margin = new Thickness(left, 0, 0, 0)
                    });
                }

                frame.Children.Add(new TextBlock
                {
                    Text = date.ToString("dddd"),
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = COL_WIDTH - COL_PADDING,
                    Margin = new Thickness(left + COL_PADDING, 6, 0, 0)
                });

                float allDayTop = DAYS_ROW_HEIGHT;
                foreach (var obj in arranger.HolidayAndAllDayItems)
                {
                    View allDayView = null;
                    if (obj is ViewItemHoliday holiday)
                    {
                        allDayView = new Border
                        {
                            BackgroundColor = Color.FromArgb(228, 0, 137),
                            CornerRadius = 6,
                            Tapped = () => ViewModel.MainScreenViewModel?.ViewHoliday(holiday),
                            Content = new TextBlock
                            {
                                Text = holiday.Name,
                                Margin = new Thickness(6),
                                WrapText = false,
                                TextColor = Color.White,
                                FontSize = Theme.Current.CaptionFontSize,
                                FontWeight = FontWeights.SemiBold
                            }
                        };
                    }
                    else if (obj is ViewItemTaskOrEvent taskOrEvent)
                    {
                        allDayView = new MainCalendarItemComponent
                        {
                            Item = taskOrEvent,
                            ViewModel = ViewModel,
                            ShowItem = i => ViewModel.MainScreenViewModel?.ShowItem(i)
                        };
                    }

                    if (allDayView == null)
                    {
                        continue;
                    }

                    frame.Children.Add(new FrameLayout
                    {
                        Width = COL_WIDTH - COL_PADDING * 2,
                        Height = ALL_DAY_ITEM_HEIGHT,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(left + COL_PADDING, allDayTop, 0, 0),
                        Children = { allDayView }
                    });
                    allDayTop += ALL_DAY_ITEM_HEIGHT + ALL_DAY_ITEM_SPACING;
                }

                foreach (var s in arranger.ScheduleItems)
                {
                    float width = (COL_WIDTH - COL_PADDING * 2) / s.NumOfColumns;
                    float itemLeft = left + COL_PADDING + width * s.Column;
                    var item = s;

                    frame.Children.Add(new FrameLayout
                    {
                        Width = width - (s.NumOfColumns > 1 ? 2 : 0),
                        Height = (float)s.Height,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(itemLeft, scheduleTop + INITIAL_MARGIN + (float)s.TopOffset, 0, 0),
                        Tapped = () => ViewModel.ViewClass(item.Item.Class),
                        Opacity = arranger.HasHolidays ? 0.7f : 1.0f,
                        Children =
                        {
                            new ScheduleItemComponent
                            {
                                ScheduleItem = item
                            }
                        }
                    });
                }

                if (arranger.HasHolidays)
                {
                    frame.Children.Add(new Border
                    {
                        BackgroundColor = Color.FromArgb(80, 228, 0, 137),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = COL_WIDTH,
                        Margin = new Thickness(left, scheduleTop + INITIAL_MARGIN, 0, 0)
                    });
                }

                if (arranger.IsDifferentSemester)
                {
                    frame.Children.Add(new DifferentSemesterComponent
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = COL_WIDTH,
                        Margin = new Thickness(left, 0, 0, 0),
                        OnOpenSemester = ViewModel.MainScreenViewModel.OpenOrShowYears
                    });
                }
            }

            frame.Width = TIMES_COLUMN_WIDTH + Math.Max(visibleCol, 1) * COL_WIDTH;
            frame.Height = scheduleTop + contentHeight;

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundColor,
                CornerRadius = Theme.Current.CornerRadius,
                Content = new ScrollView(frame)
                {
                    HorizontalScrollEnabled = true,
                    MaximumZoomScale = 1.0f,
                    MinimumZoomScale = 0.3f
                }
            };
        }

        private static float GetTopMargin(TimeSpan itemTime, TimeSpan baseTime)
        {
            return INITIAL_MARGIN + (float)Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0);
        }
    }
}
