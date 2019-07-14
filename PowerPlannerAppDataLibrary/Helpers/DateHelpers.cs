using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class DateHelpers
    {
        public static string ToFriendlyShortDate(DateTime date)
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en")
            {
                var shortDate = date.ToString("d");
                var endingYear = date.ToString("/yyyy");
                if (shortDate.EndsWith(endingYear))
                {
                    shortDate = shortDate.Substring(0, shortDate.Length - endingYear.Length);
                }

                return date.ToString("ddd") + ", " + shortDate;
            }
            else
            {
                return date.ToString("d");
            }
        }
    }
}
