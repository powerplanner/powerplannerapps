using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class DoubleToBlankStringConverter : IValueConverter
    {
        private double getBlank(object parameter)
        {
            double blank = -1;

            if (parameter != null)
            {
                if (parameter is string)
                {
                    if ((parameter as string).Contains("Min"))
                        return double.MaxValue;
                    else if ((parameter as string).Contains("Max"))
                        return double.MaxValue;
                    else if (double.TryParse((parameter as string), out blank))
                        return blank;
                }

                else if (parameter is double)
                    return (double)parameter;
            }

            return -1;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double blank = getBlank(parameter);

            if (value is double)
            {
                if ((double)value == blank)
                    return "";
                return value.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
