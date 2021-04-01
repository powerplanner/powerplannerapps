using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP.Controls
{
    public class PopupsPanel : Grid
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children != null)
            {
                for (int i = Children.Count - 2; i >= 0; i--)
                {
                    Children[i].Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }

                if (Children.Count > 0)
                {
                    Children.Last().Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }

            return base.MeasureOverride(availableSize);
        }
    }
}
