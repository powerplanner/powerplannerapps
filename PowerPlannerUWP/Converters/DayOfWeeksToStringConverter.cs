using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public partial class DayOfWeeksToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<DayOfWeek>)
            {
                var dayOfWeeks = value as IEnumerable<DayOfWeek>;

                return string.Join(", ", dayOfWeeks.Select(i => DateTools.ToLocalizedString(i)));
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
