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
    public class WeekDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

            if (value is DateTime)
            {
                return ((DateTime)value).ToString(formatString);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}