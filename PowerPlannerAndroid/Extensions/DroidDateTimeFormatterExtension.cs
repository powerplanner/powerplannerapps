using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Helpers;
using Vx.Extensions;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidDateTimeFormatterExtension : DateTimeFormatterExtension
    {
        public override string FormatAsShortTime(DateTime time)
        {
            return DateHelper.ToShortTimeString(time);
        }

        public override string FormatAsShortTimeWithoutAmPm(DateTime time)
        {
            if (CultureInfo.CurrentCulture.Name == "en-US")
            {
                return FormatAsShortTime(time).TrimEnd('A', 'P', 'M', 'a', 'p', 'm', ' ');
            }
            else
            {
                return FormatAsShortTime(time);
            }
        }
    }
}