using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class NumberToFormatConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string)
            {
                string p = parameter as string;

                if (value is double)
                    return ((double)value).ToString(p);

                if (value is int)
                    return ((int)value).ToString(p);

                if (value is decimal)
                    return ((decimal)value).ToString(p);

                if (value is float)
                    return ((float)value).ToString(p);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
