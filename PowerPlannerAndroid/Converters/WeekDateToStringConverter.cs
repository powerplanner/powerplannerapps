using System;
using System.Globalization;

namespace PowerPlannerAndroid.Converters
{
    public class WeekDateToStringConverter
    {
        public static string Convert(DateTime value)
        {
            string formatString;

            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("en"))
            {
                formatString = "M/d";
            }
            else
            {
                formatString = "d";
            }

            return value.ToString(formatString);
        }
    }
}