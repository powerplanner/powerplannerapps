using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day
{
    public class DayScheduleSnapshotComponent : VxComponent
    {
        private static readonly float TIME_INDICATOR_SIZE = 60;
        private static readonly float GAP_SIZE = 2;
        public static readonly float HEIGHT_OF_HOUR = TIME_INDICATOR_SIZE + GAP_SIZE;

        [VxSubscribe]
        public DayScheduleItemsArranger ArrangedItems { get; set; }

        protected override View Render()
        {
            float totalHeight = ((int)(ArrangedItems.EndTime - ArrangedItems.StartTime).TotalHours + 1) * HEIGHT_OF_HOUR;

            var root = new FrameLayout
            {
                Height = totalHeight
            };

            for (TimeSpan time = ArrangedItems.StartTime; time <= ArrangedItems.EndTime; time = time.Add(TimeSpan.FromHours(1)))
            {
                string text = DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(time)).Split(':')[0];
                var fromTop = GetTopMarginAsPx(time, ArrangedItems.StartTime);

                root.Children.Add(new Border
                {
                    BackgroundColor = Theme.Current.BackgroundAlt2Color,
                    Height = TIME_INDICATOR_SIZE,
                    Width = TIME_INDICATOR_SIZE,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, fromTop, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Content = new TextBlock
                    {
                        Text = text,
                        FontSize = Theme.Current.TitleFontSize,
                        FontWeight = FontWeights.SemiLight,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                });

                root.Children.Add(new Border
                {
                    BackgroundColor = Theme.Current.BackgroundAlt2Color,
                    Margin = new Thickness(TIME_INDICATOR_SIZE + GAP_SIZE, fromTop, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = TIME_INDICATOR_SIZE
                });
            }

            foreach (var s in ArrangedItems.ScheduleItems)
            {
                AddVisualItem(root, s, new ScheduleItemComponent
                {
                    ScheduleItem = s
                });
            }

            return root;
        }

        private void AddVisualItem(FrameLayout root, DayScheduleItemsArranger.BaseScheduleItem item, View view)
        {
            View viewToAdd = view;

            if (item.NumOfColumns > 1)
            {
                var linLayout = new LinearLayout
                {
                    Orientation = Orientation.Horizontal
                };

                for (int i = 0; i < item.NumOfColumns; i++)
                {
                    if (i == item.Column)
                    {
                        linLayout.Children.Add(view.LinearLayoutWeight(1));
                    }
                    else
                    {
                        linLayout.Children.Add(new Border().LinearLayoutWeight(1));
                    }
                }

                viewToAdd = linLayout;
            }

            viewToAdd.VerticalAlignment = VerticalAlignment.Top;
            viewToAdd.Margin = new Thickness(TIME_INDICATOR_SIZE + GAP_SIZE + (float)item.LeftOffset, (float)item.TopOffset, 0, 0);
            root.Children.Add(viewToAdd);
        }

        private float GetTopMarginAsPx(TimeSpan itemTime, TimeSpan baseTime)
        {
            return (float)Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0);
        }
    }
}
