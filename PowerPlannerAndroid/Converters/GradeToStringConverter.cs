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
using InterfacesDroid.Converters;
using PowerPlannerSending;

namespace PowerPlannerAndroid.Converters
{
    public class GradeToStringConverter : DoubleToStringConverter
    {
        /// <summary>
        /// If the value is the UNGRADED constant, it'll return "--". Otherwise it formats the number with the provided parameter.
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double && ((double)value == Grade.UNGRADED))
                return "--";

            return base.Convert(value, targetType, parameter, culture);
        }
    }
}