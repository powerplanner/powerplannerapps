using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPDateTimeFormatterExtension : DateTimeFormatterExtension
    {
        public override string FormatAsShortTime(DateTime time)
        {
            return DateTimeFormatter.ShortTime.Format(time).Replace("\u200E", "");
        }

        public override string FormatAsShortTimeWithoutAmPm(DateTime time)
        {
            return FormatAsShortTime(time).TrimEnd('A', 'P', 'M', 'a', 'p', 'm', ' ');
        }
    }
}
