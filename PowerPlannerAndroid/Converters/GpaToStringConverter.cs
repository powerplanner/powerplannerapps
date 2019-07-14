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
using BareMvvm.Core.Bindings;
using PowerPlannerSending;
using InterfacesDroid.Converters;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Converters
{
    public class GpaToStringConverter : DoubleToStringConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string answer;

            if (value is double && (double)value != -1)
                answer = ((double)value).ToString("0.0##");

            else
                answer = "--";

            if (parameter is string)
            {
                switch (parameter as string)
                {
                    case "IncludeGPA":
                        return string.Format(PowerPlannerResources.GetString("String_GPA"), answer);
                }
            }

            return answer;
        }
    }
}