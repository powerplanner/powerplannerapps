using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class CreditsToTextBoxTextConverter
    {
        public static string Convert(double credits)
        {
            if (credits == -1)
            {
                return "";
            }

            return credits.ToString();
        }

        public static double ConvertBack(string creditsString)
        {
            double credits;

            if (string.IsNullOrWhiteSpace(creditsString) || !double.TryParse(creditsString, out credits))
            {
                return -1;
            }

            return credits;
        }
    }
}
