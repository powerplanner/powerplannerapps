using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUniversal;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class ColorArrayToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[])
            {
                byte[] arr = value as byte[];
                return ColorTools.GetColor(arr);
            }

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Color)
            {
                return ColorTools.GetArray((Windows.UI.Color)value, 3);
            }

            return value;
        }
    }
}
