using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class ImageUploadOptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ImageUploadOptions)
            {
                switch ((ImageUploadOptions)value)
                {
                    case ImageUploadOptions.Always:
                        return LocalizedResources.GetString("String_Always");

                    case ImageUploadOptions.WifiOnly:
                        return LocalizedResources.GetString("ImageUploadOptions_WifiOnly");

                    case ImageUploadOptions.Never:
                        return LocalizedResources.GetString("String_Never");

                    default:
                        return value.ToString();
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
