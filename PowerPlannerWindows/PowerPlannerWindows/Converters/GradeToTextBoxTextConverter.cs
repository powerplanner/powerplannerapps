using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class GradeToTextBoxTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                if ((double)value == PowerPlannerSending.Grade.UNGRADED)
                    return "";
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double parsed;
            if (value is string && double.TryParse(value as string, out parsed))
            {
                return parsed;
            }

            return PowerPlannerSending.Grade.UNGRADED;
        }
    }
}
