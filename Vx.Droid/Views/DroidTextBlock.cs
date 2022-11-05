using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidTextBlock : DroidView<TextBlock, TextView>
    {
        public DroidTextBlock() : base(new TextView(VxDroidExtensions.ApplicationContext))
        {

        }

        protected override void ApplyProperties(TextBlock oldView, TextBlock newView)
        {
            base.ApplyProperties(oldView, newView);

            if (oldView == null && newView is HyperlinkTextBlock)
            {
                View.AutoLinkMask = Android.Text.Util.MatchOptions.All;
            }

            View.Text = newView.Text;
            View.SetTextColor(newView.TextColor.ToDroid());
            View.SetTextSize(Android.Util.ComplexUnitType.Dip, newView.FontSize);
            View.Gravity = newView.TextAlignment.ToDroid(ViewGroup.LayoutParams.WrapContent);

            switch (newView.FontWeight)
            {
                case FontWeights.Bold:
                case FontWeights.SemiBold:
                    View.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                    break;

                default:
                    View.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
                    break;
            }

            View.SetSingleLine(!newView.WrapText);
            View.SetMaxLines(newView.MaxLines == 0 ? int.MaxValue : newView.MaxLines);

            // Strikethrough: https://stackoverflow.com/a/52344500
            View.PaintFlags = newView.Strikethrough ? View.PaintFlags | Android.Graphics.PaintFlags.StrikeThruText : View.PaintFlags & ~Android.Graphics.PaintFlags.StrikeThruText;
        }
    }
}