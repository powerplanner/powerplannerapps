using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.NativeViews
{
    public abstract class VxDroidNativeView<V, N> : VxNativeView<V, N>
        where V : VxView
        where N : View
    {
        protected override object CreateNativeView()
        {
            return Activator.CreateInstance(typeof(N), new object[] { VxDroidNative.Context });
        }
    }
}