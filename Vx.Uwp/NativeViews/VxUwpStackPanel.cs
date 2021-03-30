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
    public class VxUwpStackPanel : VxUwpNativeView<VxStackPanel, StackPanel>, IVxStackPanel
    {
        public VxView[] Children { set => SetListOfViewsOnCollection(value, NativeView.Children); }
        public VxOrientation Orientation { set => NativeView.Orientation = (Orientation)value; }
    }
}
