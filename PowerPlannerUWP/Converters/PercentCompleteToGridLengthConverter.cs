using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class PercentCompleteToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double percentComplete = 0;

            if (value is double)
                percentComplete = (double)value;

            if (percentComplete < 0)
                percentComplete = 0;

            if (percentComplete > 1)
                percentComplete = 1;

            if (parameter is string && (parameter as string).Equals("reverse", StringComparison.CurrentCultureIgnoreCase))
                percentComplete = 1 - percentComplete;

            return new GridLength(percentComplete, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
