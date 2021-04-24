using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace InterfacesUWP
{
    public class MyEqualColumnsPanel : Panel, IScrollSnapPointsInfo
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children == null || Children.Count == 0)
                return new Size();

            var childrenThatCount = Children.Where(c => c.Visibility == Visibility.Visible).ToList();

            Size childSize = new Size(ColumnWidth, availableSize.Height);

            foreach (var child in Children)
            {
                child.Measure(childSize);
            }

            return new Size(ColumnWidth * Children.Count + ColumnSpacing * (Children.Count + 1), availableSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                if (Children == null || Children.Count == 0)
                    return new Size();

                Size childSize = new Size(ColumnWidth, finalSize.Height);
                double x = ColumnSpacing;

                foreach (var child in Children)
                {
                    child.Arrange(new Rect(new Point(x, 0), childSize));

                    // We consumed a column and a spacing
                    x += ColumnWidth + ColumnSpacing;
                }

                return new Size(ColumnWidth * Children.Count + ColumnSpacing * (Children.Count + 1), finalSize.Height);
            }

            finally
            {
                HorizontalSnapPointsChanged?.Invoke(this, null);
            }
        }

        public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(double), typeof(MyEqualColumnsPanel), new PropertyMetadata(0.0, OnColumnWidthChanged));

        public double ColumnWidth
        {
            get { return (double)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        private static void OnColumnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyEqualColumnsPanel).OnColumnWidthChanged();
        }

        private void OnColumnWidthChanged()
        {
            base.InvalidateMeasure();
        }

        public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register("ColumnSpacing", typeof(double), typeof(MyEqualColumnsPanel), new PropertyMetadata(0.0, OnColumnSpacingChanged));

        public event EventHandler<object> HorizontalSnapPointsChanged;

#pragma warning disable 0067
        /// <summary>
        /// Note that this does NOT function. It's required for implementing the interface, but not used.
        /// </summary>
        public event EventHandler<object> VerticalSnapPointsChanged;
#pragma warning restore 0067

        /// <summary>
        /// Spacing that is applied to left and right of each column (adjacent columns merge their spacing so it isn't doubled)
        /// </summary>
        public double ColumnSpacing
        {
            get { return (double)GetValue(ColumnSpacingProperty); }
            set { SetValue(ColumnSpacingProperty, value); }
        }

        public bool AreHorizontalSnapPointsRegular
        {
            get
            {
                return true;
            }
        }

        public bool AreVerticalSnapPointsRegular
        {
            get
            {
                return true;
            }
        }

        private static void OnColumnSpacingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyEqualColumnsPanel).OnColumnSpacingChanged();
        }

        private void OnColumnSpacingChanged()
        {
            base.InvalidateMeasure();
        }

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            // Never has irregular points, so this should never be called
            throw new NotImplementedException();
        }

        /// <summary>
        /// The distance between the equidistant snap points. Returns 0 when no snap points are present.
        /// </summary>
        /// <param name="orientation">The orientation/dimension for the desired snap point set.</param>
        /// <param name="alignment">The alignment to use when applying the snap points.</param>
        /// <param name="offset">Out parameter. The offset of the first snap point.</param>
        /// <returns>The distance between the equidistant snap points. Returns 0 when no snap points are present.</returns>
        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            // Vertical doesn't have snap points
            if (orientation == Orientation.Vertical)
            {
                offset = 0;
                return 0;
            }

            offset = 0;
            return (float)(ColumnWidth + ColumnSpacing);
        }
    }
}
