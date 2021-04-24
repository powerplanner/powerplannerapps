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
    public class MySquaresGrid : Panel
    {
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(int), typeof(MySquaresGrid), new PropertyMetadata(1, OnVisualPropertyChanged));

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(MySquaresGrid), new PropertyMetadata(1, OnVisualPropertyChanged));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty SpacingBetweenSquaresProperty = DependencyProperty.Register("SpacingBetweenSquares", typeof(double), typeof(MySquaresGrid), new PropertyMetadata(0, OnVisualPropertyChanged));

        public double SpacingBetweenSquares
        {
            get { return (double)GetValue(SpacingBetweenSquaresProperty); }
            set { SetValue(SpacingBetweenSquaresProperty, value); }
        }

        public static readonly DependencyProperty DesiredFitToSizeProperty = DependencyProperty.Register("DesiredFitToSize", typeof(Size?), typeof(MySquaresGrid), new PropertyMetadata(null, OnVisualPropertyChanged));

        public Size? DesiredFitToSize
        {
            get { return GetValue(DesiredFitToSizeProperty) as Size?; }
            set { SetValue(DesiredFitToSizeProperty, value); }
        }

        private static void OnVisualPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MySquaresGrid).OnVisualPropertyChanged(e);
        }

        private void OnVisualPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double cellSize = calculateCellSize(GetSizeToUse(availableSize));

            foreach (FrameworkElement child in base.Children.OfType<FrameworkElement>())
            {
                Size childSize = CalculateChildSize(cellSize, child);

                child.Measure(childSize);
            }

            return calculateTotalSize(cellSize);
        }

        private Size GetSizeToUse(Size providedSize)
        {
            if (DesiredFitToSize != null)
            {
                Size answer = new Size(DesiredFitToSize.Value.Width, DesiredFitToSize.Value.Height);

                if (answer.Width > base.MaxWidth)
                    answer.Width = base.MaxWidth;

                if (answer.Height > base.MaxHeight)
                    answer.Height = base.MaxHeight;

                return answer;
            }

            return providedSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double cellSize = calculateCellSize(GetSizeToUse(finalSize));

            foreach (FrameworkElement child in base.Children.OfType<FrameworkElement>())
            {
                Size childSize = CalculateChildSize(cellSize, child);
                Point childLocation = this.CalculateChildLocation(cellSize, child);

                child.Arrange(new Rect(childLocation, childSize));
            }

            return calculateTotalSize(cellSize);
        }

        private Point CalculateChildLocation(double cellSize, FrameworkElement child)
        {
            int row = Grid.GetRow(child);
            int col = Grid.GetColumn(child);

            double x = cellSize * col + this.SpacingBetweenSquares * col;
            double y = cellSize * row + this.SpacingBetweenSquares * row;

            return new Point(x, y);
        }

        private Size CalculateChildSize(double cellSize, FrameworkElement child)
        {
            int rowSpan = Grid.GetRowSpan(child);
            int colSpan = Grid.GetColumnSpan(child);

            double width = cellSize * colSpan + this.SpacingBetweenSquares * (colSpan - 1);
            double height = cellSize * rowSpan + this.SpacingBetweenSquares * (rowSpan - 1);

            return new Size(width, height);
        }

        private Size calculateTotalSize(double cellSize)
        {
            double totalWidth = cellSize * this.Columns + this.SpacingBetweenSquares * (this.Columns - 1);
            double totalHeight = cellSize * this.Rows + this.SpacingBetweenSquares * (this.Rows - 1);

            return new Size(totalWidth, totalHeight);
        }

        private double calculateCellSizeBasedOnWidth(double width)
        {
            return (width - this.SpacingBetweenSquares * (this.Columns - 1)) / this.Columns;
        }

        private double calculateCellSizeBasedOnHeight(double height)
        {
            return (height - this.SpacingBetweenSquares * (this.Rows - 1)) / this.Rows;
        }
        
        private double calculateCellSize(Size size)
        {
            if (!double.IsInfinity(size.Height))
            {
                // If height is the only one that's set, then we have to fit height
                if (double.IsInfinity(size.Width))
                    return calculateCellSizeBasedOnHeight(size.Height);

                // Otherwise, find out which one is the lowest denominator
                double cellWidth = calculateCellSizeBasedOnWidth(size.Width);
                double cellHeight = calculateCellSizeBasedOnHeight(size.Height);

                // If we have less width available, we fit width
                if (cellWidth < cellHeight)
                {
                    // We have to make sure we're not violating the MinHeight
                    Size resultingSize = calculateTotalSize(cellWidth);
                    if (resultingSize.Height < base.MinHeight)
                        return calculateCellSizeBasedOnHeight(base.MinHeight);

                    return cellWidth;
                }

                // Otherwise we fit height
                else
                {
                    // We have to make sure we're not violating the MinWidth
                    Size resultingSize = calculateTotalSize(cellHeight);
                    if (resultingSize.Width < base.MinWidth)
                        return calculateCellSizeBasedOnWidth(base.MinWidth);

                    return cellHeight;
                }
            }

            else if (double.IsInfinity(size.Width))
                throw new ArgumentException("Both Width and Height cannot be infinity");

            // Otherwise we'll fit by width
            else
                return calculateCellSizeBasedOnWidth(size.Width);
        }
    }
}
