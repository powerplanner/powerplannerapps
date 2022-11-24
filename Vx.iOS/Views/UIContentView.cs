using System;
using System.Linq;
using CoreGraphics;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class UIContentView : UIPanel
    {
        public UIContentView()
        {
        }

        private UIViewWrapper _content;
        public UIViewWrapper Content
        {
            get => _content;
            set
            {
                if (value != Content)
                {
                    _content = value;
                    base.ClearArrangedSubviews();

                    if (value != null)
                    {
                        base.AddArrangedSubview(value);
                    }
                }
            }
        }

        private Thickness _padding = new Thickness();
        public Thickness Padding
        {
            get => _padding;
            set
            {
                if (value != _padding)
                {
                    _padding = value;

                    SetNeedsUpdateConstraints();
                }
            }
        }

        public override void ArrangeSubviews()
        {
            Content?.SetConstraints(
                new WrapperConstraint(this, NSLayoutAttribute.Left, 1, Padding.Left),
                new WrapperConstraint(this, NSLayoutAttribute.Top, 1, Padding.Top),
                new WrapperConstraint(this, NSLayoutAttribute.Right, 1, Padding.Right),
                new WrapperConstraint(this, NSLayoutAttribute.Bottom, 1, Padding.Bottom),
                this,
                this);

            // Ensures auto widths/heights within linear layouts don't end up consuming full space
            Content?.View.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Horizontal);
            Content?.View.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Vertical);
        }
    }
}

