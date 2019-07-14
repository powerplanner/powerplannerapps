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
using PowerPlannerSending;

namespace PowerPlannerAndroid.Converters
{
    public class GpaTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GpaType gpaType)
            {
                return PowerPlannerAppDataLibrary.Converters.GpaTypeToStringConverter.Convert(gpaType);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}