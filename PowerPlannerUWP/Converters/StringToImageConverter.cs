using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (!(value is string))
                    return value;

                return App.GetImage(value as string);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
