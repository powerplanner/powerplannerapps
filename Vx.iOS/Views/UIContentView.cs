using System;
using System.Linq;
using CoreGraphics;
using UIKit;

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
                }
            }
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                if (Content != null)
                {
                    return Content.IntrinsicContentSize;
                }

                return new CGSize(0, 0);
            }
        }

        public override void LayoutSubviews()
        {
            if (Content != null)
            {
                Content.Arrange(new CGRect(), Frame.Size);
            }
        }
    }
}

