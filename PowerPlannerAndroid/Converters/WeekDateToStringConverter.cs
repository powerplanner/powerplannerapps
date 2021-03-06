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

            return (value).ToString(formatString);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return Convert(dateTime);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}