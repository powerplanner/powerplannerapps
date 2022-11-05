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

            // Note that to ensure clearing strikethrough works, we always use/set AttributedText rather than standard text
            View.AttributedText = new NSAttributedString(newView.Text, strikethroughStyle: newView.Strikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None);
            View.TextColor = newView.TextColor.ToUI();

            if (newView.MaxLines != 0)
            {
                View.Lines = newView.MaxLines;
            }
            else
            {
                View.Lines = newView.WrapText ? 0 : 1;
            }

            View.Font = View.Font.WithSize(newView.FontSize);
            View.TextAlignment = newView.TextAlignment.ToUITextAlignment();

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

    public class iOSHyperlinkTextBlock : iOSView<HyperlinkTextBlock, UITextView>
    {
        public iOSHyperlinkTextBlock()
        {
            View.Editable = false;
            View.ScrollEnabled = false;

            // Link detection: http://iosdevelopertips.com/user-interface/creating-clickable-hyperlinks-from-a-url-phone-number-or-address.html
            View.DataDetectorTypes = UIDataDetectorType.All;

            // Lose the padding: https://stackoverflow.com/questions/746670/how-to-lose-margin-padding-in-uitextview
            View.TextContainerInset = UIEdgeInsets.Zero;
            View.TextContainer.LineFragmentPadding = 0;
        }

        protected override void ApplyProperties(HyperlinkTextBlock oldView, HyperlinkTextBlock newView)
        {
            base.ApplyProperties(oldView, newView);

            //if (newView.Strikethrough)
            //{
            //    View.Text = null;
            //    View.AttributedText = new NSAttributedString(newView.Text, strikethroughStyle: NSUnderlineStyle.Single);
            //}
            //else
            //{
                //View.AttributedText = null;

                // We need to set to non-empty so the Font property gets initialized
                View.Text = string.IsNullOrEmpty(newView.Text) ? " " : newView.Text;
            //}

            View.TextColor = newView.TextColor.ToUI();
            //View.Lines = newView.WrapText ? 0 : 1;
            View.Font = View.Font.WithSize(newView.FontSize);
            View.TextAlignment = newView.TextAlignment.ToUITextAlignment();

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