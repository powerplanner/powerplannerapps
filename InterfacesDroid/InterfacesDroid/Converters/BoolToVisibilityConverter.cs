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

namespace InterfacesDroid.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string && ((parameter as string).Equals("Invert", StringComparison.CurrentCultureIgnoreCase)))
            {
                value = !(value is bool && (bool)value);
            }

            if (value is bool && (bool)value)
                return ViewStates.Visible;

            return ViewStates.Gone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}