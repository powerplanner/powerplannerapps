using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.Bindings;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Converters
{
    public class CreditsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string answer;

            if (value is double && (double)value != PowerPlannerSending.Grade.NO_CREDITS)
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
                        return string.Format(PowerPlannerResources.GetString("String_Credits"), answer);
                }
            }

            return answer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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