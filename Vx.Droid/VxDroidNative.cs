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
using Vx.Droid.NativeViews;
using Vx.Views;

namespace Vx.Droid
{
    public static class VxDroidNative
    {
        internal static Activity MainActivity { get; private set; }
        internal static Context Context { get; private set; }

        public static void Initialize(Activity mainActivity)
        {
            MainActivity = mainActivity;
            Context = mainActivity.ApplicationContext;

            VxNativeView.Mappings[typeof(VxTextBlock)] = typeof(VxDroidTextBlock);

            VxDispatcher.Current = new VxDroidDispatcher();
        }
    }
}