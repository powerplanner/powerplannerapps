﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpLinearLayout : UwpView<Vx.Views.LinearLayout, LinearLayout>
    {
        protected override void ApplyProperties(Vx.Views.LinearLayout oldView, Vx.Views.LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Orientation = newView.Orientation;

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
            float totalWeight = GetTotalWeight();
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            bool autoHeightsForAll = double.IsPositiveInfinity(isVert ? availableSize.Height : availableSize.Width) || totalWeight == 0;

            if (autoHeightsForAll)
            {
                double consumed = 0;
                double maxOtherDimension = 0;

                foreach (var child in Children)
                {
                    if (child.Visibility == Windows.UI.Xaml.Visibility.Visible)
                    {
                        child.Measure(isVert ? new Size(availableSize.Width, double.PositiveInfinity) : new Size(double.PositiveInfinity, availableSize.Height));
                        consumed += isVert ? child.DesiredSize.Height : child.DesiredSize.Width;
                        maxOtherDimension = Math.Max(maxOtherDimension, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
                    }
                }

                return isVert ? new Size(double.IsPositiveInfinity(availableSize.Width) ? maxOtherDimension : availableSize.Width, consumed) : new Size(consumed, double.IsPositiveInfinity(availableSize.Height) ? maxOtherDimension : availableSize.Height);
            }

            else
            {
                double remainingAvailable = isVert ? availableSize.Height : availableSize.Width;
                double maxOtherDimension = 0;

                // First measure the autos
                foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible))
                {
                    var weight = GetWeight(child);
                    if (weight == 0)
                    {
                        child.Measure(isVert ? new Size(availableSize.Width, remainingAvailable) : new Size(remainingAvailable, availableSize.Height));

                        remainingAvailable = Math.Max(0, remainingAvailable - (isVert ? child.DesiredSize.Height : child.DesiredSize.Width));
                        maxOtherDimension = Math.Max(maxOtherDimension, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
                    }
                }

                // Then measure the weighted
                foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible))
                {
                    var weight = GetWeight(child);
                    if (weight > 0)
                    {
                        var amount = remainingAvailable * (weight / totalWeight);
                        child.Measure(isVert ? new Size(availableSize.Width, amount) : new Size(amount, availableSize.Height));
                        maxOtherDimension = Math.Max(maxOtherDimension, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
                    }
                }

                return isVert ? new Size(maxOtherDimension, availableSize.Height) : new Size(availableSize.Width, maxOtherDimension);
            }
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
