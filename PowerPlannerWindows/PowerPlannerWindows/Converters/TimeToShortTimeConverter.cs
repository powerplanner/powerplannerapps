using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using Microsoft.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class TimeToShortTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                return new DateTimeFormatter("{hour.integer}‎:‎{minute.integer(2)}").Format((DateTime)value);
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
