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
            View.Lines = newView.WrapText ? 0 : 1;
            View.Font = View.Font.WithSize(newView.FontSize);

            switch (newView.FontWeight)
            {
                case FontWeights.Bold:
                case FontWeights.SemiBold:
                    View.Font = View.Font.Bold();
                    break;

                default:
                    View.Font = View.Font.WithTraits(0);
                    break;
            }
        }
    }
}