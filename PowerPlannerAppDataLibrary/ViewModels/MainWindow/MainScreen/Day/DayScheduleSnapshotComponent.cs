using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in ArrangedItems.EventItems.Reverse())
            {
                if (e.IsCollapsedMode)
                {
                    AddVisualItem(root, e, new CollapsedTaskOrEventComponent
                    {
                        EventItem = e
                    });
                }
                else
                {
                    AddVisualItem(root, e, RenderFullEventItem(e));
                }
            }

            return root;
        }

        private View RenderFullEventItem(DayScheduleItemsArranger.EventItem e)
        {
            return new Border
            {
                BackgroundColor = e.Item.Class.Color.ToColor(),
                Content = new TextBlock
                {
                    Text = e.Item.Name,
                    WrapText = false,
                    TextColor = Color.White,
                    Margin = new Thickness(6),
                    VerticalAlignment = VerticalAlignment.Top
                },
                CornerRadius = 8,
                Tapped = () => PowerPlannerApp.Current.GetMainScreenViewModel().ShowItem(e.Item),
                Height = (float)e.Height
            };
        }

        private class CollapsedTaskOrEventComponent : VxComponent
        {
            public override bool SubscribeToIsMouseOver => true;
            public DayScheduleItemsArranger.EventItem EventItem { get; set; }

            protected override View Render()
            {
                if (IsMouseOver)
                {
                    var allItems = new List<ViewItemTaskOrEvent>() { EventItem.Item };
                    if (EventItem.AdditionalItems != null)
                    {
                        allItems.AddRange(EventItem.AdditionalItems);
                    }

                    var expandedItems = new LinearLayout
                    {
                        Margin = new Thickness(0, 4, 0, 4)
                    };

                    foreach (var i in allItems)
                    {
                        expandedItems.Children.Add(new MainCalendarItemComponent
                        {
                            Item = i,
                            ShowItem = item => PowerPlannerApp.Current.GetMainScreenViewModel().ShowItem(item),
                            Margin = new Thickness(0, expandedItems.Children.Count == 0 ? 0 : 1, 0, 0)
                        });
                    }

                    return new Border
                    {
                        BackgroundColor = Theme.Current.BackgroundColor,
                        CornerRadius = 8,
                        BorderColor = Theme.Current.SubtleForegroundColor.Opacity(0.3),
                        BorderThickness = new Thickness(3),
                        Content = expandedItems
                    };
                }
                else
                {
                    var collapsed = new Border
                    {
                        BackgroundColor = EventItem.Item.Class.Color.ToColor(),
                        Content = new TextBlock
                        {
                            Text = EventItem.Item.Name.Length > 0 ? EventItem.Item.Name.Substring(0, 1) : "",
                            WrapText = false,
                            TextColor = Color.White,
                            Margin = new Thickness(6),
                            VerticalAlignment = VerticalAlignment.Top,
                            TextAlignment = HorizontalAlignment.Center,
                            FontSize = 16
                        },
                        CornerRadius = 8,
                        Width = ScheduleItemComponent.WIDTH_OF_COLLAPSED_ITEM,
                        Height = (float)EventItem.Height
                    };

                    if (EventItem.AdditionalItems != null)
                    {
                        var additionalCircles = new LinearLayout
                        {
                            Margin = new Thickness(2, 0, 0, 0)
                        };

                        foreach (var a in EventItem.AdditionalItems)
                        {
                            const int circleSize = 8;

                            additionalCircles.Children.Add(new Border
                            {
                                BackgroundColor = a.IsComplete ? Theme.Current.SubtleForegroundColor.Opacity(0.3) : a.Class.Color.ToColor(),
                                Width = circleSize,
                                Height = circleSize,
                                CornerRadius = circleSize,
                                Margin = new Thickness(0, 0, 0, 2)
                            });
                        }

                        return new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Height = (float)EventItem.Height,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Children =
                            {
                                collapsed,
                                additionalCircles
                            }
                        };
                    }

                    collapsed.HorizontalAlignment = HorizontalAlignment.Left;

                    return collapsed;
                }
            }
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
