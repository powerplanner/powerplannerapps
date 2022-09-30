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

                    InvalidateIntrinsicContentSize();
                    SetNeedsLayout();
                }
            }
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                var answer = base.IntrinsicContentSize;
                return new CGSize(answer.Width + Padding.Width, answer.Height + Padding.Height);
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var baseWidth = size.Width;
            if (baseWidth != 0)
            {
                baseWidth = MaxF(0, baseWidth - Padding.Width);
            }
            var baseHeight = size.Height;
            if (baseHeight != 0)
            {
                baseHeight = MaxF(0, baseHeight - Padding.Height);
            }

            var answer = base.SizeThatFits(new CGSize(baseWidth, baseHeight));

            return new CGSize(answer.Width + Padding.Width, answer.Height + Padding.Height);
        }

        public override void LayoutSubviews()
        {
            if (Content != null)
            {
                Content.Frame = new CGRect(new CGPoint(Padding.Left, Padding.Top), new CGSize(Frame.Width - Padding.Width, Frame.Height - Padding.Height));
            }
        }
    }
}

