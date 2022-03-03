using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Vx.Uwp.Views
{
    public class UwpLinearLayout : UwpView<Vx.Views.LinearLayout, LinearLayout>
    {
        private static int count = 0;
        protected override void ApplyProperties(Vx.Views.LinearLayout oldView, Vx.Views.LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Orientation = newView.Orientation;
            View.Background = newView.BackgroundColor.ToUwpBrush();

            ReconcileList(oldView?.Children, newView.Children, View.Children);
        }
    }

    public class LinearLayout : Panel
    {
        private Vx.Views.Orientation _orientation;
        public Vx.Views.Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    InvalidateMeasure();
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // MeasureOverride is supposed to report the SMALLEST possible size that will fit the content. If availableSize.Height is 700
            // but content could fit in 200, then should return 200. This effectively means that the weighted heights are treated as
            // auto. This ensures that for the ViewTask dialog, even though it uses a weighted height, if the content is smaller, the
            // window will stay smaller. And I've matched this to the behavior of UWP grids, where when there's less content, the Star row
            // definitions behave as Auto. If the area can't fit all of the content, the Auto columns get first priority.

            // However this does NOT work for calendar grid when each square has an excess amount of items, since the earlier rows will
            // report they need more space and the later rows will not get enough space...
            // We could measure all weighted with infinite height, to see how little they need?
            // Think I solved it below...

            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            double consumed = 0;
            double maxOtherDimension = 0;
            float totalWeight = GetTotalWeight();

            bool autoHeightsForAll = double.IsPositiveInfinity(isVert ? availableSize.Height : availableSize.Width) || totalWeight == 0;

            // StackPanel essentially
            if (autoHeightsForAll)
            {
                foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible))
                {
                    child.Measure(isVert ? new Size(availableSize.Width, double.PositiveInfinity) : new Size(double.PositiveInfinity, availableSize.Height));

                    consumed += isVert ? child.DesiredSize.Height : child.DesiredSize.Width;
                    maxOtherDimension = Math.Max(maxOtherDimension, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
                }

                return isVert ? new Size(maxOtherDimension, consumed) : new Size(consumed, maxOtherDimension);
            }

            // We measure autos FIRST, since those get priority
            foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible && GetWeight(i) == 0))
            {
                child.Measure(isVert ? new Size(availableSize.Width, Math.Max(0, availableSize.Height - consumed)) : new Size(Math.Max(0, availableSize.Width - consumed), availableSize.Height));

                consumed += isVert ? child.DesiredSize.Height : child.DesiredSize.Width;
                maxOtherDimension = Math.Max(maxOtherDimension, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
            }

            double weightedAvailable = Math.Max((isVert ? availableSize.Height : availableSize.Width) - consumed, 0);

            if (totalWeight > 0)
            {
                foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible && GetWeight(i) != 0))
                {
                    var weight = GetWeight(child);
                    var childConsumed = (weight / totalWeight) * weightedAvailable;

                    child.Measure(isVert ? new Size(availableSize.Width, childConsumed) : new Size(childConsumed, availableSize.Height));

                    consumed += isVert ? child.DesiredSize.Height : child.DesiredSize.Width;
                    maxOtherDimension = Math.Max(maxOtherDimension, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
                }
            }

            return isVert ? new Size(maxOtherDimension, consumed) : new Size(consumed, maxOtherDimension);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            float totalWeight = GetTotalWeight();
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            bool autoHeightsForAll = double.IsPositiveInfinity(isVert ? finalSize.Height : finalSize.Width) || totalWeight == 0;

            double pos = 0;

            if (autoHeightsForAll)
            {
                foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible))
                {
                    child.Arrange(new Rect(isVert ? new Point(0, pos) : new Point(pos, 0), isVert ? new Size(finalSize.Width, child.DesiredSize.Height) : new Size(child.DesiredSize.Width, finalSize.Height)));
                    pos += isVert ? child.DesiredSize.Height : child.DesiredSize.Width;
                }
            }

            else
            {
                double consumedByAuto = Children.Where(i => i.Visibility == Visibility.Visible && GetWeight(i) == 0).Sum(i => isVert ? i.DesiredSize.Height : i.DesiredSize.Width);
                double weightedAvailable = Math.Max(0, (isVert ? finalSize.Height : finalSize.Width) - consumedByAuto);

                foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible))
                {
                    var weight = GetWeight(child);

                    double consumed;
                    if (weight == 0)
                    {
                        consumed = isVert ? child.DesiredSize.Height : child.DesiredSize.Width;
                    }
                    else
                    {
                        consumed = (weight / totalWeight) * weightedAvailable;
                    }

                    child.Arrange(new Rect(isVert ? new Point(0, pos) : new Point(pos, 0), isVert ? new Size(finalSize.Width, consumed) : new Size(consumed, finalSize.Height)));
                    pos += consumed;
                }
            }

            return finalSize;
        }

        private float GetTotalWeight()
        {
            return Children.Where(i => i.Visibility == Visibility.Visible).Sum(i => GetWeight(i));
        }

        public static float GetWeight(DependencyObject obj)
        {
            return (float)obj.GetValue(WeightProperty);
        }

        public static void SetWeight(DependencyObject obj, float value)
        {
            obj.SetValue(WeightProperty, value);
        }

        public static readonly DependencyProperty WeightProperty = DependencyProperty.Register("Weight", typeof(float), typeof(LinearLayout), new PropertyMetadata(0f));
    }
}
