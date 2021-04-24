using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Vx.iOS.Views;
using Vx.Views;

namespace Vx.iOS
{
    public static class VxiOSExtensions
    {
        static VxiOSExtensions()
        {
            Theme.Current = new VxiOSTheme();

            NativeView.CreateNativeView = view =>
            {
                if (view is Vx.Views.TextBlock)
                {
                    return new iOSTextBlock();
                }

                if (view is Vx.Views.LinearLayout)
                {
                    return new iOSLinearLayout();
                }

                if (view is Vx.Views.Button)
                {
                    return new iOSButton();
                }

                if (view is Vx.Views.TextBox)
                {
                    return new iOSTextBox();
                }

                if (view is Vx.Views.ScrollView)
                {
                    return new iOSScrollView();
                }

                if (view is Vx.Views.FontIcon)
                {
                    return new iOSFontIcon();
                }

#if DEBUG
                // Control not implemented
                System.Diagnostics.Debugger.Break();
#endif
                throw new NotImplementedException();
            };
        }

        public static UIView Render(this VxComponent component)
        {
            var nativeComponent = new iOSNativeComponent();
            component.InitializeForDisplay(nativeComponent);
            return nativeComponent;
        }

        internal static UIView CreateUIView(this Vx.Views.View view, Vx.Views.View parentView)
        {
            return view.CreateNativeView(parentView).View as UIView;
        }

        internal static UIColor ToUI(this Color color)
        {
            return UIColor.FromRGBA(color.R, color.G, color.B, color.A);
        }
    }
}