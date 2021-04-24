using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSFontIcon : iOSView<Vx.Views.FontIcon, UILabel>
    {
        protected override void ApplyProperties(FontIcon oldView, FontIcon newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = newView.Glyph;
            View.Font = UIFont.FromName("Material Icons Outlined", newView.FontSize);
            View.TextColor = newView.Color.ToUI();
        }
    }
}
