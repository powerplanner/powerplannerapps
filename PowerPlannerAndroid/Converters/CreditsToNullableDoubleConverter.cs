using System;
using System.Globalization;
using BareMvvm.Core.Bindings;

namespace PowerPlannerAndroid.Converters
{
    public class CreditsToNullableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                if ((double)value == PowerPlannerSending.Grade.NO_CREDITS)
                {
                    return null;
                }

                return value;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return PowerPlannerSending.Grade.NO_CREDITS;
            }

            return value;
        }
    }
}