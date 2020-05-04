using PowerPlannerAppDataLibrary.DataLayer;
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

        /// <summary>
        /// Something like "Monday, Feb 10"
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToMediumDateString(DateTime date)
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "pt")
            {
                // We use short day since it's slightly longer, and it outputs "segunda-feira" for Monday, which is crazy long
                // Returns "seg, 10 de fev"
                return date.ToString("ddd, d \\de MMM");
            }
            else
            {
                // Returns "Monday, Feb 10
                return date.ToString("dddd, MMM d");
            }
        }

        public static DateTime ToViewItemTime(AccountDataItem account, DateTime rawDateTime)
        {
            if (account.SchoolTimeZone == null)
            {
                return DateTime.SpecifyKind(rawDateTime, DateTimeKind.Local);
            }

            var currentTimeZone = TimeZoneInfo.Local;

            return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(rawDateTime, DateTimeKind.Unspecified), sourceTimeZone: account.SchoolTimeZone, destinationTimeZone: currentTimeZone);
        }
    }
}
