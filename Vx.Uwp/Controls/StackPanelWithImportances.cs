using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP.Controls
{
    /// <summary>
    /// Only supports vertical alignment right now
    /// </summary>
    public class StackPanelWithImportances : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width))
            {
                throw new InvalidOperationException("Horizontal isn't supported");
            }

            double consumedHeight = Children.OfType<FrameworkElement>().Sum(i => i.MinHeight);

            foreach (var el in ChildrenByImportance())
            {
                double minHeight = (el as FrameworkElement)?.MinHeight ?? 0;

                double remainingHeight = availableSize.Height - consumedHeight + minHeight;
                if (remainingHeight > minHeight)
                {
                    el.Measure(new Size(availableSize.Width, remainingHeight));

                    consumedHeight += el.DesiredSize.Height - minHeight;
                }
            }

            return new Size(availableSize.Width, consumedHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double y = 0;
            foreach (var el in Children)
            {
                el.Arrange(new Rect(
                    x: 0,
                    y: y,
                    width: finalSize.Width,
                    height: el.DesiredSize.Height));

                y += el.DesiredSize.Height;
            }

            // We always consume whatever height the parent said
            return finalSize;
        }

        private IEnumerable<UIElement> ChildrenByImportance()
        {
            return Children.OrderBy(i => (int)i.GetValue(ImportanceProperty));
        }

        public static readonly DependencyProperty ImportanceProperty =
            DependencyProperty.RegisterAttached("Importance", typeof(int), typeof(StackPanelWithImportances), new PropertyMetadata(0));

        public static int GetImportance(UIElement el)
        {
            return (int)el.GetValue(ImportanceProperty);
        }

        public static void SetImportance(UIElement el, int value)
        {
            el.SetValue(ImportanceProperty, value);
        }
    }
}
