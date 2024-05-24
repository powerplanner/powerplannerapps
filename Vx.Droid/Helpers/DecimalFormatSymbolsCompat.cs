using Android.App;
using Android.Content;
using Android.Icu.Text;
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
    public static class DecimalFormatSymbolsCompat
    {
        public static char DecimalSeparator
        {
            get
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(24))
                {
                    return DecimalFormatSymbols.Instance.DecimalSeparator;
                }
                else
                {
                    return (1.1).ToString()[1];
                }
            }
        }
    }
}