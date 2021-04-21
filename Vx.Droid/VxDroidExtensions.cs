using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Droid.Views;
using Vx.Views;

namespace Vx.Droid
{
    public static class VxDroidExtensions
    {
        /// <summary>
        /// Parent app must initialize this
        /// </summary>
        public static Context ApplicationContext { get; set; }

        static VxDroidExtensions()
        {
            Theme.Current = new VxDroidTheme();

            NativeView.CreateNativeView = view =>
            {
                if (view is Vx.Views.TextBlock)
                {
                    return new DroidTextBlock();
                }

                if (view is Vx.Views.LinearLayout)
                {
                    return new DroidLinearLayout();
                }

                if (view is Vx.Views.Button)
                {
                    return new DroidButton();
                }

                if (view is Vx.Views.TextBox)
                {
                    return new DroidTextBox();
                }

                throw new NotImplementedException();
            };
        }

        public static Android.Views.View Render(this VxComponent component)
        {
            var nativeComponent = new DroidNativeComponent(ApplicationContext);
            component.InitializeForDisplay(nativeComponent);
            return nativeComponent;
        }

        internal static Android.Views.View CreateDroidView(this Vx.Views.View view, Vx.Views.View parentView)
        {
            return view.CreateNativeView(parentView).View as Android.Views.View;
        }

        internal static Android.Graphics.Color ToDroid(this Color color)
        {
            return new Android.Graphics.Color(color.ToArgb());
        }
    }
}