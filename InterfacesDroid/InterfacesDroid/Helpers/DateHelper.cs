using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Helpers
{
    public static class DateHelper
    {
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromMilliseconds(long milliseconds)
        {
            return EPOCH.AddMilliseconds(milliseconds).ToLocalTime();
        }

        public static long ToMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - EPOCH).TotalMilliseconds;
        }

        public static string ToShortTimeString(DateTime date)
        {
            var timeFormat = Android.Text.Format.DateFormat.GetTimeFormat(Application.Context);
            var epochDateTime = ConvertDateTimeToUnixTime(date, true);

            using (var javaDate = new Java.Util.Date(epochDateTime))
            {
                return timeFormat.Format(javaDate);
            }
        }

        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ConvertDateTimeToUnixTime(DateTime date, bool isDatarequiredInMilliSeconds = true, DateTimeKind dateTimeKind = DateTimeKind.Local) => 
            Convert.ToInt64((DateTime.SpecifyKind(date, dateTimeKind).ToUniversalTime() - EpochDateTime).TotalSeconds) * (isDatarequiredInMilliSeconds ? 1000 : 1);
    }
}