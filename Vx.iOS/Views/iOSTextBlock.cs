using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTextBlock : iOSView<TextBlock, UILabel>
    {
        protected override void ApplyProperties(TextBlock oldView, TextBlock newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = newView.Text;
            View.TextColor = newView.TextColor.ToUI();
        }
    }
}