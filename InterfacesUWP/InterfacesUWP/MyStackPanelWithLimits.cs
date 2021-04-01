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
    public class MyStackPanelWithLimits : Panel
    {
        public static void SetRequireFit(UIElement el, bool value)
        {
            el.SetValue(RequireFitProperty, value);
        }

        public static bool GetRequireFit(UIElement el)
        {
            object obj = el.GetValue(RequireFitProperty);
            if (obj is bool)
            {
                return (bool)obj;
            }
            return false;
        }

        public static readonly DependencyProperty RequireFitProperty =
            DependencyProperty.Register("RequireFit", typeof(bool), typeof(UIElement), new PropertyMetadata(false));

        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize.Width == double.PositiveInfinity)
            {
                throw new InvalidOperationException("Cannot be placed in an infinite-width scenario");
            }

            double height = 0;
            double remainingWidth = availableSize.Width;

            var requiredFitChild = Children.FirstOrDefault(i => GetRequireFit(i));
            if (requiredFitChild != null)
            {
                // The required fit element gets up to the entire space if necessary
                requiredFitChild.Measure(availableSize);

                height = Math.Max(height, requiredFitChild.DesiredSize.Height);
                remainingWidth -= requiredFitChild.DesiredSize.Width;
            }

            if (remainingWidth <= 0)
            {
                remainingWidth = 0;
            }

            foreach (var child in Children)
            {
                if (child == requiredFitChild)
                {
                    continue;
                }
                else
                {
                    child.Measure(new Size(remainingWidth, availableSize.Height));
                }

                height = Math.Max(height, child.DesiredSize.Height);
                remainingWidth -= child.DesiredSize.Width;

                if (remainingWidth < 0)
                {
                    remainingWidth = 0;
                }
            }

            return new Size(availableSize.Width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            foreach (var child in Children)
            {
                child.Arrange(new Windows.Foundation.Rect(x, 0, child.DesiredSize.Width, finalSize.Height));
                x += child.DesiredSize.Width;
            }

            return finalSize;
        }
    }
}
