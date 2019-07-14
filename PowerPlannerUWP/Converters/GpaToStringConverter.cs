using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class GpaToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string answer;

            if (value is double && (double)value != -1)
                answer = ((double)value).ToString("0.0##");

            else
                answer = "--";

            if (parameter is string)
            {
                switch (parameter as string)
                {
                    case "IncludeGPA":
                        return string.Format(LocalizedResources.GetString("String_GPA"), answer);
                }
            }

            return answer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
