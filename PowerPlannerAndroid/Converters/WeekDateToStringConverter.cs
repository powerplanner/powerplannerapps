using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.Bindings;

namespace PowerPlannerAndroid.Converters
{
    public class WeekDateToStringConverter
    {
        public static string Convert(DateTime value)
        {
            string formatString;

            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("en"))
            {
                formatString = "M/d";
            }
            else
            {
                formatString = "d";
            }

            return value.ToString(formatString);
        }
    }
}