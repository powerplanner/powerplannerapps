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
            return DateTimeFormatter.ShortTime.Format(time);
        }

        public override string FormatAsShortTimeWithoutAmPm(DateTime time)
        {
            return DateTimeFormatter.ShortTime.Format(time).TrimEnd('A', 'P', 'M', 'a', 'p', 'm', ' ');
        }
    }
}
