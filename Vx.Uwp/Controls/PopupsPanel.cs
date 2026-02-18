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
    public partial class PopupsPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            // Collapse all children except the last one
            for (int i = 0; i < Children.Count - 1; i++)
            {
                Children[i].Visibility = Visibility.Collapsed;
            }

            if (Children.Count > 0)
            {
                var last = Children[Children.Count - 1];
                last.Visibility = Visibility.Visible;
                last.Measure(availableSize);
                return last.DesiredSize;
            }

            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count > 0)
            {
                Children[Children.Count - 1].Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }

            return finalSize;
        }
    }
}
