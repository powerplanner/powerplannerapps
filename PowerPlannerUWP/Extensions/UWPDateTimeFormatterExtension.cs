using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Extensions;
using Windows.Globalization.DateTimeFormatting;

namespace PowerPlannerUWP.Extensions
{
    public class UWPDateTimeFormatterExtension : DateTimeFormatterExtension
    {
        private string[] _cachedShortTimes = new string[1440];

        public override string FormatAsShortTime(DateTime time)
        {
            // We cache these answers, since UWP's DateTimeFormatter is ridiculously slow for some reason,
            // taking about 5 milliseconds per each call, which adds up when generating a lot of times in a loop.
            // Our cache uses an array instead of a dictionary, since 1,440 items isn't that many, and an array
            // is more efficient than using a dictionary of variable size.

            int minutes = (int)time.TimeOfDay.TotalMinutes;

            string answer = _cachedShortTimes[minutes];

            if (answer == null)
            {
                answer = DateTimeFormatter.ShortTime.Format(time).Replace("\u200E", "");
                _cachedShortTimes[minutes] = answer;
            }

            return answer;
        }

        public override string FormatAsShortTimeWithoutAmPm(DateTime time)
        {
            return FormatAsShortTime(time).TrimEnd('A', 'P', 'M', 'a', 'p', 'm', ' ');
        }

        public override void ClearCache()
        {
            _cachedShortTimes = new string[1440];
        }
    }
}
