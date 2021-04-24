using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class DayOfWeekToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DayOfWeek)
                return DateTools.ToLocalizedString((DayOfWeek)value);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
