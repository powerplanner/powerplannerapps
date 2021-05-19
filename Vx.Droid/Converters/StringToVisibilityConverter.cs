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
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool notEmpty = value is string && !string.IsNullOrWhiteSpace(value as string);

            if (parameter is string && (parameter as string).Equals("Inverse", StringComparison.CurrentCultureIgnoreCase))
                notEmpty = !notEmpty;

            if (notEmpty)
            {
                return ViewStates.Visible;
            }

            return ViewStates.Gone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}