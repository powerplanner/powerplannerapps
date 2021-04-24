using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP.PivotFolder
{
    public class PivotItemsPanel : Panel
    {
        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
        {
            return availableSize;
        }

        protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
        {
            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(new Point(), finalSize));
            }

            return finalSize;
        }
    }
}
