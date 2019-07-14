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
    public class GpaToEditViewTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                if ((double)value == PowerPlannerSending.Grade.UNGRADED)
                    return "";
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double parsed;
            if (value is string && double.TryParse(value as string, out parsed))
            {
                return parsed;
            }

            return PowerPlannerSending.Grade.UNGRADED;
        }
    }
}