using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSFontIcon : iOSView<Vx.Views.FontIcon, UILabel>
    {
        public iOSFontIcon()
        {
            // A FontIcon is a single glyph. Match UWP/Android by centering the glyph within the
            // label, so when the icon is stretched (e.g. as a Stretch child of a vertical
            // LinearLayout) the glyph stays centered rather than sitting on the left.
            View.TextAlignment = UITextAlignment.Center;
        }

        protected override void ApplyProperties(FontIcon oldView, FontIcon newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = newView.Glyph;
            View.Font = UIFont.FromName("Material Icons Outlined", newView.FontSize);
            View.TextColor = newView.Color.ToUI();
        }
    }
}
