using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    class ToggleCompleteToStringConverter: IValueConverter
    {
        // If a task is not completed, ask user if they would like to complete it (and visa versa)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // TODO: Needs localization
            return value is double @double && @double < 1 ? "Mark Complete" : "Mark Incomplete";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
