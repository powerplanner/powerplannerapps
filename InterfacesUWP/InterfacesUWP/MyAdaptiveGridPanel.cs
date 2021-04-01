using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    /// <summary>
    /// A grid that displays columns and automatically adapts width-wise. You provide a min-width for columns, and the control
    /// automatically populates how many columns there can be, and then wraps next elements down to the next line.
    /// </summary>
    public class MyAdaptiveGridPanel : Panel
    {
        public static readonly DependencyProperty MinColumnWidthProperty = DependencyProperty.Register("MinColumnWidth", typeof(double), typeof(MyAdaptiveGridPanel), new PropertyMetadata(100.0));

        /// <summary>
        /// Must be positive number.
        /// </summary>
        public double MinColumnWidth
        {
            get { return (double)GetValue(MinColumnWidthProperty); }
            set { SetValue(MinColumnWidthProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // It's easier to not support infinite width, and none of my scenarios need it, so not worth spending time implementing it
            if (double.IsInfinity(availableSize.Width))
                throw new ArgumentException("This panel doesn't support infinite width");

            // If there's no width, there's nothing we can do.
            if (availableSize.Width == 0)
                return new Size(0, 0);

            var colInfo = GetColumnInfo(availableSize);

            Size availableSizeForElements = new Size(colInfo.ColumnWidth, double.PositiveInfinity);

            double totalHeight = 0;
            int i = 0;

            foreach (var invisibleChildren in Children.Where(c => c.Visibility == Visibility.Collapsed))
            {
                invisibleChildren.Measure(availableSize);
            }

            var childrenThatCount = Children.Where(c => c.Visibility == Visibility.Visible).ToList();

            if (childrenThatCount.Count == 1 && StretchIfOnlyOneChild)
            {
                childrenThatCount[0].Measure(availableSize);
                return new Size(availableSize.Width, childrenThatCount[0].DesiredSize.Height);
            }

            // While we still have items left to process
            while (i < childrenThatCount.Count)
            {
                // Start calculating row height
                double rowHeight = 0;

                // Enter the row, loop through all the columns (while we still have items left), increment both column and item
                for (int colIndex = 0; colIndex < colInfo.NumberOfColumns && i < childrenThatCount.Count; colIndex++, i++)
                {
                    var el = childrenThatCount[i];

                    el.Measure(availableSizeForElements);

                    // See if this overrides the max row height
                    rowHeight = Math.Max(rowHeight, el.DesiredSize.Height);
                }

                // And add this row's height to total height
                totalHeight += rowHeight;
            }

            return new Size(availableSize.Width, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // It's easier to not support infinite width, and none of my scenarios need it, so not worth spending time implementing it
            if (double.IsInfinity(finalSize.Width))
                throw new ArgumentException("This panel doesn't support infinite width");

            // If there's no width, there's nothing we can do.
            if (finalSize.Width == 0)
                return new Size(0, 0);

            var colInfo = GetColumnInfo(finalSize);

            double totalHeight = 0;
            int i = 0;

            var childrenThatCount = Children.Where(c => c.Visibility == Visibility.Visible).ToList();

            if (childrenThatCount.Count == 1 && StretchIfOnlyOneChild)
            {
                childrenThatCount[0].Arrange(new Rect(0, 0, finalSize.Width, childrenThatCount[0].DesiredSize.Height));
                return new Size(finalSize.Width, childrenThatCount[0].DesiredSize.Height);
            }

            // While we still have items left to process
            while (i < childrenThatCount.Count)
            {
                // Start calculating row height
                double rowHeight = 0;
                
                // Calculate max row height
                for (int x = 0; x < colInfo.NumberOfColumns && i + x < childrenThatCount.Count; x++)
                {
                    var el = childrenThatCount[i + x];

                    double height = el.DesiredSize.Height;

                    // See if this overrides the max row height
                    rowHeight = Math.Max(rowHeight, el.DesiredSize.Height);
                }

                // Enter the row, loop through all the columns (while we still have items left), increment both column and item
                for (int colIndex = 0; colIndex < colInfo.NumberOfColumns && i < childrenThatCount.Count; colIndex++, i++)
                {
                    var el = childrenThatCount[i];

                    double x = colIndex * colInfo.ColumnWidth;

                    // y is just totalHeight
                    // width is just colInfo.ColumnWidth

                    // I have a bug in Power Planner where this stretching of vertical height causes an infinite layout loop
                    // Repro is open Power Planner and add 4 years, then add 1 semester to the second year. Then delete the semester. Crash occurs.
                    // I tried everything to fix it, including making some changes in Measure to tell the items the final measure height, didn't make a difference.
                    // Basically, arranging with the higher row height seems to cause the child element to re-measure and then decide that it in fact wants a different height too, so it
                    // re-measures and this re-arranges, and then the child changes its mind again.
                    // Having Measure pass in the final row height causes the items not to update when the new item comes in.
                    //el.Arrange(new Rect(x, totalHeight, colInfo.ColumnWidth, rowHeight));
                    el.Arrange(new Rect(x, totalHeight, colInfo.ColumnWidth, el.DesiredSize.Height));
                }

                // And add this row's height to total height
                totalHeight += rowHeight;
            }

            return new Size(finalSize.Width, totalHeight);
        }


        private class ColumnInfo
        {
            public double ColumnWidth { get; set; }
            public int NumberOfColumns { get; set; }
        }

        private ColumnInfo GetColumnInfo(Size availableSize)
        {
            // Say available size is 1,000 and min column width is 200, then this would be 5 columns
            // Say available size is 1,100 and min column width is 200, then this would be 5.5 truncated to 5 columns
            int numberOfColumns = (int)(availableSize.Width / MinColumnWidth);

            // If there's not enough space for one column, we'll still have one column
            if (numberOfColumns <= 0)
                numberOfColumns = 1;

            // And then figure out desired column width given how many columns we have
            double columnWidth = availableSize.Width / numberOfColumns;

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
            DependencyProperty.Register("StretchIfOnlyOneChild", typeof(bool), typeof(MyAdaptiveGridPanel), new PropertyMetadata(false, OnMeasureAffectingPropertyChanged));

        private static void OnMeasureAffectingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyAdaptiveGridPanel).OnMeasureAffectingPropertyChanged(e);
        }

        private void OnMeasureAffectingPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.InvalidateMeasure();
        }
    }
}
