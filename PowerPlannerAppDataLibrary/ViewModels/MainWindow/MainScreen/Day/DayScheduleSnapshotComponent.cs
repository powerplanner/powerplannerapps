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

        private VxState<DayScheduleItemsArranger.EventItem> ExpandedTaskOrEvent = new VxState<DayScheduleItemsArranger.EventItem>(null);

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            // We render columns manually so we depend on the width
            // This is to ensure each column item is clickable
            if (size.Width != previousSize.Width)
            {
                MarkDirty();
            }
        }

        private void CollapseExpanded()
        {
            ExpandedTaskOrEvent.Value = null;
        }

        protected override View Render()
        {
            float totalHeight = ((int)(ArrangedItems.EndTime - ArrangedItems.StartTime).TotalHours + 1) * HEIGHT_OF_HOUR;

            var root = new FrameLayout
            {
                Height = totalHeight,
                Tapped = CollapseExpanded
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
                        TextAlignment = HorizontalAlignment.Center
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
                    ScheduleItem = s,
                    Tapped = () => PowerPlannerApp.Current.GetMainScreenViewModel().ViewClass(s.Item.Class)
                });
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in ArrangedItems.EventItems.Reverse())
            {
                if (e.IsCollapsedMode)
                {
                    AddVisualItem(root, e, new ExpandableTaskOrEventComponent
                    {
                        EventItem = e,
                        IsExpanded = ExpandedTaskOrEvent.Value == e,
                        SetExpanded = exp => ExpandedTaskOrEvent.Value = exp
                    }, isCollapsed: ExpandedTaskOrEvent.Value != e);
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
            bool hasAdditionalItems = e.AdditionalItems != null && e.AdditionalItems.Count > 0;

            if (!hasAdditionalItems)
            {
                return RenderFullEventItemStandard(e, true);
            }
            else
            {
                return new ExpandableTaskOrEventComponent
                {
                    EventItem = e,
                    IsExpanded = ExpandedTaskOrEvent.Value == e,
                    SetExpanded = exp => ExpandedTaskOrEvent.Value = exp
                };
            }
        }

        /// <summary>
        /// Renders without any additional items, just the full width event item
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static View RenderFullEventItemStandard(DayScheduleItemsArranger.EventItem e, bool isStandaloneView)
        {
            var b = new Border
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
                CornerRadius = 8
            };

            if (isStandaloneView)
            {
                b.Tapped = () => PowerPlannerApp.Current.GetMainScreenViewModel().ShowItem(e.Item);
                b.Height = (float)e.Height;
            }

            return b;
        }

        private class ExpandableTaskOrEventComponent : VxComponent
        {
            public DayScheduleItemsArranger.EventItem EventItem { get; set; }
            public bool IsExpanded { get; set; }
            public Action<DayScheduleItemsArranger.EventItem> SetExpanded { get; set; }
            public override bool SubscribeToIsMouseOver => true;
            private DateTime _ignoreMouseTill;
            public const float ADDITIONAL_CIRCLES_MARGIN = 2;
            public const float ADDITIONAL_CIRCLES_WIDTH = 8;

            protected override void OnMouseOverChanged(bool isMouseOver)
            {
                if (!isMouseOver && DateTime.Now < _ignoreMouseTill)
                {
                    return;
                }

                SetExpanded(isMouseOver ? EventItem : null);
            }

            protected override View Render()
            {
                if (IsExpanded)
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
                    View collapsed;

                    if (EventItem.IsCollapsedMode)
                    {
                        collapsed = new Border
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
                    }
                    else
                    {
                        collapsed = RenderFullEventItemStandard(EventItem, false);
                    }

                    if (EventItem.AdditionalItems != null)
                    {
                        var additionalCircles = new LinearLayout
                        {
                            Margin = new Thickness(ADDITIONAL_CIRCLES_MARGIN, 0, 0, 0)
                        };

                        foreach (var a in EventItem.AdditionalItems)
                        {
                            const int circleSize = 8;

                            additionalCircles.Children.Add(new Border
                            {
                                BackgroundColor = a.IsComplete ? Theme.Current.SubtleForegroundColor.Opacity(0.3) : a.Class.Color.ToColor(),
                                Width = ADDITIONAL_CIRCLES_WIDTH,
                                Height = ADDITIONAL_CIRCLES_WIDTH,
                                CornerRadius = circleSize,
                                Margin = new Thickness(0, 0, 0, 2)
                            });
                        }

                        return new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Height = (float)EventItem.Height,
                            Children =
                            {
                                collapsed.LinearLayoutWeight(1),
                                additionalCircles
                            },
                            Tapped = OnCollapsedTapped
                        };
                    }

                    collapsed.Tapped = OnCollapsedTapped;

                    return collapsed;
                }
            }

            private void OnCollapsedTapped()
            {
                _ignoreMouseTill = DateTime.Now.AddSeconds(0.5);
                SetExpanded(EventItem);
            }
        }

        private void AddVisualItem(FrameLayout root, DayScheduleItemsArranger.BaseScheduleItem item, View view, bool isCollapsed = false)
        {
            View viewToAdd = view;

            float leftOffset = TIME_INDICATOR_SIZE + GAP_SIZE + (float)item.LeftOffset;
            float rightOffset = 0;

            if (item.NumOfColumns > 1 && Size.Width > leftOffset)
            {
                // Instead of using columns (linearlayouts) to achieve this, we use the raw width offset to ensure that items remain clickable.
                // Otherwise the overlapping transparent linearlayouts block click events from getting through.
                float colWidth = (Size.Width - leftOffset) / item.NumOfColumns;

                // Note that the margins will only work for up to 2 columns, but that's all we support today anyways
                leftOffset += colWidth * item.Column + 3 * item.Column;
                rightOffset += colWidth * (item.NumOfColumns - item.Column - 1) + (item.Column != item.NumOfColumns - 1 ? 3 : 0);
            }

            else if (isCollapsed)
            {
                // We add right margin when item is collapsed so it doesn't steal the clicks of the class to the right
                rightOffset = Math.Max(0, Size.Width - leftOffset - ScheduleItemComponent.WIDTH_OF_COLLAPSED_ITEM - ExpandableTaskOrEventComponent.ADDITIONAL_CIRCLES_WIDTH - ExpandableTaskOrEventComponent.ADDITIONAL_CIRCLES_MARGIN);
            }

            viewToAdd.VerticalAlignment = VerticalAlignment.Top;
            viewToAdd.Margin = new Thickness(leftOffset, (float)item.TopOffset, rightOffset, 0);

            root.Children.Add(viewToAdd);
        }

        private float GetTopMarginAsPx(TimeSpan itemTime, TimeSpan baseTime)
        {
            return (float)Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0);
        }
    }
}
