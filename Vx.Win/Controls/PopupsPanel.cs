using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

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
                    Children[i].Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }

                if (Children.Count > 0)
                {
                    Children.Last().Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            }

            return base.MeasureOverride(availableSize);
        }
    }
}
