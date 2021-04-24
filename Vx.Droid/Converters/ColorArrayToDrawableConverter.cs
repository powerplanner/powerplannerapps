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
using Android.Graphics.Drawables;
using Android.Graphics;
using InterfacesDroid.Helpers;

namespace InterfacesDroid.Converters
{
    public class ColorArrayToDrawableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[])
            {
                Color color = ColorTools.GetColor(value as byte[]);

                return new ColorDrawable(color);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}