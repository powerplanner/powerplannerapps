using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Reconciler;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxUwpStackPanel : VxNativeView<VxStackPanel, StackPanel>, IVxStackPanel
    {
        public VxUwpStackPanel(VxStackPanel view) : base(view, new StackPanel())
        {
        }

        public VxView[] Children
        {
            set
            {
                SetListOfViews(value, (changeType, index, nativeView) =>
                {
                    switch (changeType)
                    {
                        case VxNativeViewListItemChange.Insert:
                            NativeView.Children.Insert(index, nativeView.NativeView as UIElement);
                            break;

                        case VxNativeViewListItemChange.Replace:
                            NativeView.Children[index] = nativeView.NativeView as UIElement;
                            break;

                        case VxNativeViewListItemChange.Remove:
                            NativeView.Children.RemoveAt(index);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });
            }
        }
    }
}
