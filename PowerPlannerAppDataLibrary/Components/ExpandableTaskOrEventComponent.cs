using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.Drawing;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    /// <summary>
    /// Displays a task/event positioned at a specific time on a day's visual schedule. When there are
    /// collisions it collapses and expands on hover/tap to reveal all items.
    /// </summary>
    public partial class ExpandableTaskOrEventComponent : VxComponent
    {
        public DayScheduleItemsArranger.EventItem EventItem { get; set; }
        public bool IsExpanded { get; set; }
        public Action<DayScheduleItemsArranger.EventItem> SetExpanded { get; set; }
        public override bool SubscribeToMouseEvents => true;
        private DateTime _ignoreMouseTill;
        public const float ADDITIONAL_CIRCLES_MARGIN = 2;
        public const float ADDITIONAL_CIRCLES_WIDTH = 8;

        /// <summary>
        /// Renders the appropriate view for the event item, which is either the expandable component or the plain full width item.
        /// </summary>
        public static View CreateView(DayScheduleItemsArranger.EventItem e, DayScheduleItemsArranger.EventItem expandedItem, Action<DayScheduleItemsArranger.EventItem> setExpanded)
        {
            bool hasAdditionalItems = e.AdditionalItems != null && e.AdditionalItems.Count > 0;

            if (e.IsCollapsedMode || hasAdditionalItems)
            {
                return new ExpandableTaskOrEventComponent
                {
                    EventItem = e,
                    IsExpanded = expandedItem == e,
                    SetExpanded = setExpanded
                };
            }

            return RenderFullEventItemStandard(e, true);
        }

        /// <summary>
        /// Renders without any additional items, just the full width event item
        /// </summary>
        public static View RenderFullEventItemStandard(DayScheduleItemsArranger.EventItem e, bool isStandaloneView)
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
                        additionalCircles.Children.Add(new Border
                        {
                            BackgroundColor = a.IsComplete ? Theme.Current.SubtleForegroundColor.Opacity(0.3) : a.Class.Color.ToColor(),
                            Width = ADDITIONAL_CIRCLES_WIDTH,
                            Height = ADDITIONAL_CIRCLES_WIDTH,
                            CornerRadius = ADDITIONAL_CIRCLES_WIDTH / 2f,
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
}
