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

namespace Vx.Droid.Helpers
{
    public static class WindowInsetsHelper
    {
        /// <summary>
        /// https://developer.android.com/reference/androidx/core/view/WindowInsetsCompat#getInsets(int)
        /// 
        /// When running on devices with API Level 29 and before, the returned insets are an approximation based on the information available. This is especially true for the IME type, which currently only works when running on devices with SDK level 23 and above.
        /// </summary>
        public static readonly bool IsFullySupported = (int)Build.VERSION.SdkInt >= 30;
    }
}