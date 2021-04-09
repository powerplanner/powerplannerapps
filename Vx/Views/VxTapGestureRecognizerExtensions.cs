using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public static class VxTapGestureRecognizerExtensions
    {
        public static T Tap<T>(this T view, Action onTap) where T : View
        {
            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += delegate
            {
                onTap();
            };
            view.GestureRecognizers.Add(tapRecognizer);
            return view;
        }

        public static T Tap<T>(this T view, Action<T> onTap) where T : View
        {
            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += delegate
            {
                onTap(view);
            };
            view.GestureRecognizers.Add(tapRecognizer);
            return view;
        }
    }
}
