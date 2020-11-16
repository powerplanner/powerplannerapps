using System;
using System.Globalization;
using BareMvvm.Core.Bindings;

namespace PowerPlannerAndroid.Converters
{
    public class GradeToNullableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                if ((double)value == PowerPlannerSending.Grade.UNGRADED)
                    return null;

                return value;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return value;
            }

            return PowerPlannerSending.Grade.UNGRADED;
        }
    }
}