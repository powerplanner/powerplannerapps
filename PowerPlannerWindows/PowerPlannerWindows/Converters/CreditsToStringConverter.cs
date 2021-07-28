using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class CreditsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string answer;

            if (value is double && (double)value != -1)
                answer = ((double)value).ToString("0.##");

            else
            {
                answer = "--";

                if (parameter is string && (parameter as string).Equals("BlankIfNone"))
                {
                    return "";
                }
            }

            if (parameter is string)
            {
                switch (parameter as string)
                {
                    case "IncludeCredits":
                        return string.Format(LocalizedResources.GetString("String_Credits"), answer);
                }
            }

            return answer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double credits;
            if (string.IsNullOrWhiteSpace(value as string) || !double.TryParse(value as string, out credits))
            {
                return PowerPlannerSending.Grade.NO_CREDITS;
            }

            return credits;
        }
    }
}
