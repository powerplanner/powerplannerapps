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
    public class PercentToEditViewTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is double)
                {
                    if ((double)value == PowerPlannerSending.Grade.UNGRADED)
                        return "";

                    return (((double)value) * 100).ToString();
                }

                return value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CONVERT: " + ex.ToString());
                return "77";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double parsed;
                if (value is string && double.TryParse(value as string, out parsed))
                {
                    return parsed / 100;
                }

                return PowerPlannerSending.Grade.UNGRADED;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BACK: " + ex.ToString());
                return 77;
            }
        }
    }
}