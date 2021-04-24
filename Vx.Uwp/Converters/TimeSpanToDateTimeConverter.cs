using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class TimeSpanToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
                return DateTime.Today.Add((TimeSpan)value);

            return DateTime.Today;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
                return ((DateTime)value).TimeOfDay;

            return new TimeSpan();
        }
    }
}
