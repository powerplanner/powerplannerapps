using System;
using Vx.Extensions;

namespace PowerPlannerUWP.Extensions
{
    public class UWPDateTimeFormatterExtension : DateTimeFormatterExtension
    {
        public override string FormatAsShortTime(DateTime time)
        {
            // Use the user's regional format culture captured at startup (before
            // CultureOverride replaced CultureInfo.CurrentCulture). This respects
            // the user's exact Short time selection from Windows settings, including
            // 12/24-hour clock and leading zeros.
            return time.ToString("t", CultureOverride.UserRegionalCulture);
        }

        public override string FormatAsShortTimeWithoutAmPm(DateTime time)
        {
            return FormatAsShortTime(time).TrimEnd('A', 'P', 'M', 'a', 'p', 'm', ' ');
        }
    }
}
