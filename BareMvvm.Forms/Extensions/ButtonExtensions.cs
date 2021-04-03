using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BareMvvm.Forms.Extensions
{
    public static class ButtonExtensions
    {
        public static T Click<T>(this T button, Action value) where T : Button
        {
            button.Clicked += delegate
            {
                value();
            };

            return button;
        }
    }
    public static class ImageButtonExtensions
    {
        public static T Click<T>(this T button, Action value) where T : ImageButton
        {
            button.Clicked += delegate
            {
                value();
            };

            return button;
        }
    }
}
