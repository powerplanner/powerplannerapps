using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class NumberMultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                if (parameter is int)
                    return (int)value * (int)parameter;

                if (parameter is double)
                    return (int)value * (double)parameter;
            }

            else if (value is double)
            {
                if (parameter is int)
                    return (double)value * (int)parameter;

                if (parameter is double)
                    return (double)value * (double)parameter;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
