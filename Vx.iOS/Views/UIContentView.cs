using System;
using System.Linq;
using CoreGraphics;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class UIContentView : UIView
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
                    Subviews.FirstOrDefault()?.RemoveFromSuperview();

                    if (value != null)
                    {
                        base.AddSubview(value.View);
                    }

                    InvalidateIntrinsicContentSize();
                    SetNeedsLayout();
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
                if (Content != null)
                {
                    var contentSize = Content.IntrinsicContentSize;
                    return new CGSize(contentSize.Width + Padding.Width, contentSize.Height + Padding.Height);
                }

                return new CGSize(Padding.Width, Padding.Height);
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            if (Content == null)
            {
                return new CGSize(Padding.Width, Padding.Height);
            }

            size = new CGSize(size.Width - Padding.Width, size.Height - Padding.Height);
            var measured = Content.Measure(size);
            return new CGSize(measured.Width + Padding.Width, measured.Height + Padding.Height);
        }

        public override void LayoutSubviews()
        {
            if (Content != null)
            {
                Content.Arrange(new CGPoint(Padding.Left, Padding.Top), new CGSize(Frame.Width - Padding.Width, Frame.Height - Padding.Height));
            }
        }
    }
}

