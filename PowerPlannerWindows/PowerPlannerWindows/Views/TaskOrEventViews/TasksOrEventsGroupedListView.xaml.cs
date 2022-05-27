using Microsoft.UI.Xaml.Controls;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable.Sql;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    public sealed partial class TasksOrEventsGroupedListView : UserControl
    {
        public TasksOrEventsGroupedListView()
        {
            this.InitializeComponent();

            VerticalAlignment = VerticalAlignment.Top;
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSourceProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TasksOrEventsGroupedListView), new PropertyMetadata(null));


    }

    public class GroupedLayout : VirtualizingLayout
    {
        public double MinColumnWidth => 300;

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            var state = new GroupedLayoutState(context);
            context.LayoutState = state;
            base.InitializeForContextCore(context);
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = null;
            base.UninitializeForContextCore(context);
        }

        protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            var state = (GroupedLayoutState)context.LayoutState;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    state.OnAdd(args.NewStartingIndex, args.NewItems.Count);
                    break;
                case NotifyCollectionChangedAction.Move:
                    state.OnMove(args.OldStartingIndex, args.NewStartingIndex, args.NewItems.Count);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    state.OnRemove(args.OldStartingIndex, args.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    state.OnReplace(args.NewStartingIndex, args.NewItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    state.Clear();
                    break;
            }

            base.OnItemsChangedCore(context, source, args);
        }

        private const double ColumnSpacing = 20;
        private const double AfterHeaderSpacing = 8;
        private const double ItemSpacing = 6;
        private const double AfterGroupSpacing = 37;

        private Size _currMeasuredSize;
        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            // Note that you MUST call GetOrCreateElement on any items that are in the realized region... that's how
            // the control tracks which elements it shouldn't dispose.

            // It's easier to not support infinite width, and none of my scenarios need it, so not worth spending time implementing it
            if (double.IsInfinity(availableSize.Width))
                throw new ArgumentException("This panel doesn't support infinite width");

            // If there's no width, there's nothing we can do.
            if (availableSize.Width == 0)
                return new Size(0, 0);

            var state = (GroupedLayoutState)context.LayoutState;
            bool widthChanged = state.AvailableWidth != availableSize.Width;
            state.AvailableWidth = availableSize.Width;

            var colInfo = GetColumnInfo(availableSize);

            Size availableSizeForElements = new Size(colInfo.ColumnWidth, double.PositiveInfinity);
            double y = 0;
            int i = 0;
            int col = 0;
            double currRowHeight = 0;

            while (i < context.ItemCount)
            {
                if (col >= colInfo.NumberOfColumns)
                {
                    // Start a new row
                    y += currRowHeight + AfterGroupSpacing;
                    col = 0;
                    currRowHeight = 0;
                }

                var colSize = MeasureColumn(
                    context,
                    state,
                    availableSizeForElements,
                    headerIndex: i,
                    startingY: y,
                    widthChanged: widthChanged,
                    nextHeaderIndex: out i);

                currRowHeight = Math.Max(currRowHeight, colSize.Height);

                col++;
            }

            y += currRowHeight;

            _currMeasuredSize = new Size(availableSize.Width, y);
            return _currMeasuredSize;
        }

        private Size MeasureColumn(VirtualizingLayoutContext context, GroupedLayoutState state, Size availableSizeForElements, int headerIndex, double startingY, bool widthChanged, out int nextHeaderIndex)
        {
            // Note that you MUST call GetOrCreateElement on any items that are in the realized region... that's how
            // the control tracks which elements it shouldn't dispose.

            double y = startingY;
            int i = headerIndex;

            var header = state.GetItemAt(headerIndex);
            //if (widthChanged || header.Height == null)
            {
                var headerEl = context.GetOrCreateElementAt(headerIndex);
                headerEl.Measure(availableSizeForElements);
                header.Height = headerEl.DesiredSize.Height;
            }

            y += header.Height.Value + AfterHeaderSpacing;

            i++;

            while (i < context.ItemCount)
            {
                if (!(context.GetItemAt(i) is ViewItemTaskOrEvent))
                {
                    // End of column
                    break;
                }

                var item = state.GetItemAt(i);

                // Logic technically ignores height of item, but heights are short enough it shouldn't matter
                if (y < context.RealizationRect.Top
                    || y > context.RealizationRect.Bottom)
                {
                    y += item.Height.GetValueOrDefault(40);
                }
                else
                {
                    // Theoretically since tasks/events are removed and then added back when edited, we know they can never change size...
                    // Therefore we can store their measured size, and only call measure if their width changed or height unknown...
                    // However this didn't work in reality, not sure why.

                    //if (widthChanged || item.Height == null)
                    {
                        var itemEl = context.GetOrCreateElementAt(i);
                        itemEl.Measure(availableSizeForElements);
                        item.Height = itemEl.DesiredSize.Height;
                    }

                    y += item.Height.Value;
                }

                // Include item spacing
                y += ItemSpacing;

                i++;
            }

            // Remove last item spacing (since no item after it)
            y -= ItemSpacing;

            nextHeaderIndex = i;
            return new Size(availableSizeForElements.Width, y - startingY);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            // Note that you MUST call GetOrCreateElement on any items that are in the realized region... that's how
            // the control tracks which elements it shouldn't dispose.

            // It's easier to not support infinite width, and none of my scenarios need it, so not worth spending time implementing it
            if (double.IsInfinity(finalSize.Width))
                throw new ArgumentException("This panel doesn't support infinite width");

            // If there's no width, there's nothing we can do.
            if (finalSize.Width == 0)
                return new Size(0, 0);

            var state = (GroupedLayoutState)context.LayoutState;
            var colInfo = GetColumnInfo(finalSize);
            bool widthChanged = state.AvailableWidth != finalSize.Width;

            double y = 0;
            int i = 0;
            int col = 0;
            double currRowHeight = 0;

            while (i < context.ItemCount)
            {
                if (col >= colInfo.NumberOfColumns)
                {
                    // Start a new row
                    y += currRowHeight + AfterGroupSpacing;
                    col = 0;
                    currRowHeight = 0;
                }

                var colSize = ArrangeColumn(
                    context,
                    state,
                    colInfo,
                    col,
                    headerIndex: i,
                    startingY: y,
                    widthChanged: widthChanged,
                    nextHeaderIndex: out i);

                currRowHeight = Math.Max(currRowHeight, colSize.Height);

                col++;
            }

            y += currRowHeight;

            var actualFinalSize = new Size(finalSize.Width, y);
            if (_currMeasuredSize != actualFinalSize)
            {
#if DEBUG
                Debugger.Break();
#endif
            }

            return actualFinalSize;
        }



        private Size ArrangeColumn(VirtualizingLayoutContext context, GroupedLayoutState state, ColumnInfo colInfo, int col, int headerIndex, double startingY, bool widthChanged, out int nextHeaderIndex)
        {
            // Note that you MUST call GetOrCreateElement on any items that are in the realized region... that's how
            // the control tracks which elements it shouldn't dispose.

            double y = startingY;
            double x = col * colInfo.ColumnWidth + ColumnSpacing * col;
            int i = headerIndex;

            var header = state.GetItemAt(headerIndex);
            var headerEl = context.GetOrCreateElementAt(headerIndex);
            headerEl.Arrange(new Rect(x, y, colInfo.ColumnWidth, headerEl.DesiredSize.Height));
            y += headerEl.DesiredSize.Height + AfterHeaderSpacing;

            i++;

            while (i < context.ItemCount)
            {
                if (!(context.GetItemAt(i) is ViewItemTaskOrEvent))
                {
                    // End of column
                    break;
                }

                var item = state.GetItemAt(i);

                // Logic technically ignores height of item, but heights are short enough it shouldn't matter
                if (y < context.RealizationRect.Top
                    || y > context.RealizationRect.Bottom)
                {
                    y += item.Height.GetValueOrDefault(40);
                }
                else
                {
                    var el = context.GetOrCreateElementAt(i);
                    //if (widthChanged || !item.Arranged)
                    {
                        el.Arrange(new Rect(x, y, colInfo.ColumnWidth, el.DesiredSize.Height));
                    }

                    y += el.DesiredSize.Height;
                }

                // Include item spacing
                y += ItemSpacing;

                i++;
            }

            // Remove last item spacing (since no item after it)
            y -= ItemSpacing;

            nextHeaderIndex = i;
            return new Size(colInfo.ColumnWidth, y - startingY);
        }

        private class ColumnInfo
        {
            public double ColumnWidth { get; set; }
            public int NumberOfColumns { get; set; }
        }

        private ColumnInfo GetColumnInfo(Size availableSize)
        {
            // EntireWidth = ColumnWidth * Columns + ColumnSpacing * (Columns - 1)
            // ColumnWidth = (ColumnSpacing + EntireWidth - Columns * ColumnSpacing) / Columns
            // Columns = (ColumnSpacing + EntireWidth) / (ColumnSpacing + ColumnWidth)

            // Say available size is 1,000 and min column width is 200, then this would be 5 columns
            // Say available size is 1,100 and min column width is 200, then this would be 5.5 truncated to 5 columns
            int numberOfColumns = (int)((ColumnSpacing + availableSize.Width) / (ColumnSpacing + MinColumnWidth));

            // If there's not enough space for one column, we'll still have one column
            if (numberOfColumns <= 0)
                numberOfColumns = 1;

            // And then figure out desired column width given how many columns we have
            double columnWidth = (ColumnSpacing + availableSize.Width - numberOfColumns * ColumnSpacing) / numberOfColumns;

            return new ColumnInfo()
            {
                NumberOfColumns = numberOfColumns,
                ColumnWidth = columnWidth
            };
        }

        public bool StretchIfOnlyOneChild
        {
            get { return (bool)GetValue(StretchIfOnlyOneChildProperty); }
            set { SetValue(StretchIfOnlyOneChildProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StretchIfOnlyOneChild.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StretchIfOnlyOneChildProperty =
            DependencyProperty.Register("StretchIfOnlyOneChild", typeof(bool), typeof(GroupedLayout), new PropertyMetadata(false, OnMeasureAffectingPropertyChanged));

        private static void OnMeasureAffectingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as GroupedLayout).OnMeasureAffectingPropertyChanged(e);
        }

        private void OnMeasureAffectingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.InvalidateMeasure();
        }
    }

    internal class GroupedItem
    {
        public double? Height { get; set; }

        public Rect? Position { get; internal set; }
    }

    internal class GroupedLayoutState
    {
        internal List<GroupedItem> _items = new List<GroupedItem>();
        private VirtualizingLayoutContext _context;

        public GroupedLayoutState(VirtualizingLayoutContext context)
        {
            this._context = context;
        }

        public double AvailableWidth { get; internal set; }

        internal GroupedItem GetItemAt(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (index > _items.Count)
            {
                throw new InvalidOperationException("An item must have been skipped");
            }

            if (index < _items.Count)
            {
                return _items[index];
            }
            else
            {
                GroupedItem item = new GroupedItem();
                _items.Add(item);
                return item;
            }
        }

        internal void Clear()
        {
            _items.Clear();
        }

        internal void ClearPosition(int index)
        {
            //if (index >= _items.Count)
            //{
            //    // Item was added/removed but we haven't realized that far yet
            //    return;
            //}

            //int numToRemove = _items.Count - index;
            //_items.RemoveRange(index, numToRemove);
        }

        internal void ClearPositions()
        {
        }

        internal void RecycleElementAt(int index)
        {
            if (index < _items.Count)
            {
                //var item = GetItemAt(index);
                //if (item.Element != null)
                //{
                //    _context.RecycleElement(item.Element);
                //}
            }
        }

        internal void OnAdd(int index, int count)
        {
            for (var i = 0; i < count; i++)
            {
                _items.Insert(index + i, new GroupedItem());
            }
        }

        internal void OnMove(int oldIndex, int newIndex, int count)
        {
            var movedItems = _items.Skip(oldIndex).Take(count).ToArray();
            _items.RemoveRange(oldIndex, count);
            _items.InsertRange(newIndex, movedItems);
        }

        internal void OnRemove(int index, int count)
        {
            for (var i = 0; i < count; i++)
            {
                RecycleElementAt(index + 1);
            }

            _items.RemoveRange(index, count);
        }

        internal void OnReplace(int index, int count)
        {
            OnRemove(index, count);
            OnAdd(index, count);
        }
    }

    public class GroupedListViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate HeaderTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ViewItemTaskOrEvent)
            {
                return ItemTemplate;
            }
            else
            {
                return HeaderTemplate;
            }
        }
    }
}
