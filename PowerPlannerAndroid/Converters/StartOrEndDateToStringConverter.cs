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
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Converters
{
    public class StartDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? val = value as DateTime?;

            if (val != null && val.HasValue)
            {
                return val.Value.ToString("d");
            }
            else
            {
                return PowerPlannerResources.GetString("String_StartOfSemester");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EndDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? val = value as DateTime?;

            if (val != null && val.HasValue)
            {
                return val.Value.ToString("d");
            }
            else
            {
                return PowerPlannerResources.GetString("String_EndOfSemester");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}