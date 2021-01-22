using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerUWP.Converters
{
    // This converter may be able to be simplified using the methods in ViewTaskOrEventViewModel
    class ConvertTaskOrEventTypeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskOrEventType && (TaskOrEventType)value == TaskOrEventType.Task)
                return LocalizedResources.GetString("String_ConvertToEvent");

            return LocalizedResources.GetString("String_ConvertToTask");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
