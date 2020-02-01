using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class CreditsToStringConverter
    {
        public static string ConvertWithCredits(double credits)
        {
            return Convert(credits, blankIfNone: false, includeCredits: true);
        }

        public static string Convert(double credits, bool blankIfNone = false, bool includeCredits = false)
        {
            string answer;

            if (credits != -1)
                answer = credits.ToString("0.##");

            else
            {
                answer = "--";

                if (blankIfNone)
                {
                    return "";
                }
            }

            if (includeCredits)
            {
                return string.Format(PowerPlannerResources.GetString("String_Credits"), answer);
            }

            return answer;
        }

        public static double ConvertBack(string value)
        {
            double credits;
            if (string.IsNullOrWhiteSpace(value) || !double.TryParse(value, out credits))
            {
                return PowerPlannerSending.Grade.NO_CREDITS;
            }

            return credits;
        }
    }
}
