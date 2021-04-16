using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Vx.Uwp
{
    public static class VxUwpExtensions
    {
        static VxUwpExtensions()
        {
            Theme.Current = new VxUwpTheme();

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

                if (view is Vx.Views.ListItemButton)
                {
                    return new UwpListItemButton();
                }

                if (view is Vx.Views.Button)
                {
                    return new UwpButton();
                }

                if (view is Vx.Views.TextBox)
                {
                    return new UwpTextBox();
                }

                if (view is Vx.Views.FontIcon)
                {
                    return new UwpFontIcon();
                }

                throw new NotImplementedException("Unknown view. UWP hasn't implemented this.");
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

        internal static Windows.UI.Color ToUwpColor(this System.Drawing.Color color)
        {
            return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        internal static SolidColorBrush ToUwpBrush(this System.Drawing.Color color)
        {
            return new SolidColorBrush(color.ToUwpColor());
        }

        internal static Windows.UI.Text.FontWeight ToUwp(this FontWeights fontWeight)
        {
            switch (fontWeight)
            {
                case FontWeights.Bold:
                    return Windows.UI.Text.FontWeights.Bold;

                case FontWeights.SemiBold:
                    return Windows.UI.Text.FontWeights.SemiBold;

                default:
                    return Windows.UI.Text.FontWeights.Normal;
            }
        }
    }
}
