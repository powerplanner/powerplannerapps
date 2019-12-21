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
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerAndroid.Converters
{
    public class SemesterToSemesterViewStartEndStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PowerPlannerAppDataLibrary.Converters.SemesterToSemesterViewStartEndStringConverter.Convert(value as ViewItemSemester);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}