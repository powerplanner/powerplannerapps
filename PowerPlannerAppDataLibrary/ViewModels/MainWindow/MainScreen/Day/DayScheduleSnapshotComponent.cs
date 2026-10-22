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
    public partial class DayScheduleSnapshotComponent : VxComponent
    {
        private static readonly float TIME_INDICATOR_SIZE = 60;
        private static readonly float GAP_SIZE = 1;
        private const float COLUMN_GAP = 3;
        public static readonly float HEIGHT_OF_HOUR = TIME_INDICATOR_SIZE + GAP_SIZE;

        public DayScheduleItemsArranger ArrangedItems { get; set; }

        private VxState<DayScheduleItemsArranger.EventItem> ExpandedTaskOrEvent = new VxState<DayScheduleItemsArranger.EventItem>(null);

        protected override void RegisterPropertySubscriptions()
        {
            base.RegisterPropertySubscriptions();
            Subscribe(ArrangedItems);
        }

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
            var root = new FrameLayout
            {
                Height = (float)ArrangedItems.TotalHeight,
                Tapped = CollapseExpanded,
                BackgroundColor = Theme.Current.SubtleForegroundColor.Opacity(0.2)
            };

            for (TimeSpan time = ArrangedItems.StartTime; time <= ArrangedItems.EndTime; time = time.Add(TimeSpan.FromHours(1)))
            {
                string text = DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(time)).Split(':')[0];
                var fromTop = GetTopMarginAsPx(time, ArrangedItems.StartTime);

                root.Children.Add(new Border
                {
                    BackgroundColor = Theme.Current.BackgroundAlt1Color,
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
                    Margin = new Thickness(TIME_INDICATOR_SIZE, fromTop, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = TIME_INDICATOR_SIZE,
                    BackgroundColor = Theme.Current.BackgroundAlt1Color
                });
            }

            foreach (var s in ArrangedItems.ScheduleItems)
            {
                // The component is wrapped so that column sizing is applied to a plain layout, matching how ScheduleViewComponent does it
                AddVisualItem(root, s, new FrameLayout
                {
                    Children =
                    {
                        new ScheduleItemComponent
                        {
                            ScheduleItem = s,
                            Tapped = () => PowerPlannerApp.Current.GetMainScreenViewModel().ViewClass(s.Item.Class)
                        }
                    }
                });
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in ArrangedItems.EventItems.Reverse())
            {
                AddVisualItem(
                    root,
                    e,
                    ExpandableTaskOrEventComponent.CreateView(e, ExpandedTaskOrEvent.Value, exp => ExpandedTaskOrEvent.Value = exp),
                    isCollapsed: e.IsCollapsedMode && ExpandedTaskOrEvent.Value != e);
            }

            return root;
        }

        private void AddVisualItem(FrameLayout root, DayScheduleItemsArranger.BaseScheduleItem item, View view, bool isCollapsed = false)
        {
            View viewToAdd = view;

            float leftOffset = TIME_INDICATOR_SIZE + GAP_SIZE + (float)item.LeftOffset;
            float rightOffset = Theme.Current.PageMargin;

            if (item.NumOfColumns > 1 && Size.Width > leftOffset + rightOffset)
            {
                // Instead of using columns (linearlayouts) to achieve this, we use the raw width offset to ensure that items remain clickable.
                // Otherwise the overlapping transparent linearlayouts block click events from getting through.
                float colWidth = (Size.Width - leftOffset - rightOffset) / item.NumOfColumns;

                viewToAdd.HorizontalAlignment = HorizontalAlignment.Left;
                viewToAdd.Width = colWidth - (item.Column != item.NumOfColumns - 1 ? COLUMN_GAP : 0);
                leftOffset += colWidth * item.Column;
                rightOffset = 0;
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
