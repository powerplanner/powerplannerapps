using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public class VxLabel : VxView<Label>
    {
    }

    public static class VxLabelExtensions
    {
        //public static T Text<T>(this T label, string value) where T : VxLabel
        //{
        //    label.SetProperty(value);
        //    return label;
        //}
        public static VxView<T> Text<T>(this VxView<T> label, string value) where T : Label
        {
            label.SetProperty(value);
            return label;
        }
    }
}
