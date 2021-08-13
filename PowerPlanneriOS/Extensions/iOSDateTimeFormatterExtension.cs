using System;
using Foundation;
using Vx.Extensions;

namespace PowerPlanneriOS.Extensions
{
    public class iOSDateTimeFormatterExtension : DateTimeFormatterExtension
    {
        private static NSDateFormatter _shortTimeFormatter = new NSDateFormatter()
        {
            DateStyle = NSDateFormatterStyle.None,
            TimeStyle = NSDateFormatterStyle.Short
        };

        public override string FormatAsShortTime(DateTime time)
        {
            return _shortTimeFormatter.StringFor(InterfacesiOS.Views.BareUIHelper.DateTimeToNSDate(time));
        }
    }
}
