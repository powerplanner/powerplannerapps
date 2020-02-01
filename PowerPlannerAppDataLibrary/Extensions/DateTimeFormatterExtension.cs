using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public class DateTimeFormatterExtension
    {
        public static DateTimeFormatterExtension Current { get; set; } = new DateTimeFormatterExtension();

        public virtual string FormatAsShortTime(DateTime time)
        {
            var s = time.ToString("t");
            return s;
        }

        public virtual string FormatAsShortTimeWithoutAmPm(DateTime time)
        {
            return FormatAsShortTime(time).TrimEnd('A', 'P', 'M', 'a', 'p', 'm', ' ');
        }
    }
}
