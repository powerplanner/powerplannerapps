using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp
{
    public class UwpNativeComponent : ContentControl, INativeComponent
    {
        public UwpNativeComponent(VxComponent component)
        {
            Component = component;
            HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
        }

        public VxComponent Component { get; private set; }

        public void ChangeView(View view)
        {
            Content = view.CreateFrameworkElement();
        }
    }
}
