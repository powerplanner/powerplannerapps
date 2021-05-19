using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidFontIcon : DroidView<Vx.Views.FontIcon, TextView>
    {
        private static Typeface _typeface = ResourcesCompat.GetFont(VxDroidExtensions.ApplicationContext, Resource.Font.materialiconsoutlined);
        public DroidFontIcon() : base(new TextView(VxDroidExtensions.ApplicationContext))
        {
            View.SetTypeface(_typeface, TypefaceStyle.Normal);
        }

        protected override void ApplyProperties(FontIcon oldView, FontIcon newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = newView.Glyph;
            View.SetTextColor(newView.Color.ToDroid());
            View.SetTextSize(Android.Util.ComplexUnitType.Dip, newView.FontSize);
        }
    }
}