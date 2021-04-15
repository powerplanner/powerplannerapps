using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.Xaml;

namespace Vx.Uwp
{
    public static class VxUwpExtensions
    {
        static VxUwpExtensions()
        {
            NativeView.CreateNativeView = view =>
            {
                if (view is Vx.Views.TextBlock)
                {
                    return new UwpTextBlock();
                }

                if (view is Vx.Views.LinearLayout)
                {
                    return new UwpLinearLayout();
                }

                if (view is Vx.Views.Button)
                {
                    return new UwpButton();
                }

                throw new NotImplementedException();
            };
        }

        public static FrameworkElement Render(this VxComponent component)
        {
            var nativeComponent = new UwpNativeComponent();
            component.InitializeForDisplay(nativeComponent);
            return nativeComponent;
        }

        internal static FrameworkElement CreateFrameworkElement(this View view)
        {
            return view.CreateNativeView(null).View as FrameworkElement;
        }
    }
}
