using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUniversal;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.Converters
{
    public class ColorArrayToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[])
            {
                byte[] arr = value as byte[];
                return ColorTools.GetBrush(arr);
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
