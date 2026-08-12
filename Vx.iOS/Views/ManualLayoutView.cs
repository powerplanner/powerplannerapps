using System;
using CoreGraphics;
using UIKit;

namespace Vx.iOS.Views
{
    public class ManualLayoutView : UIView
    {
        public Action LayoutAction { get; set; }
        public Func<CGSize, CGSize> MeasureAction { get; set; }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            LayoutAction?.Invoke();
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return MeasureAction?.Invoke(size) ?? base.SizeThatFits(size);
        }

        public override CGSize IntrinsicContentSize => SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, UIViewWrapper.UnboundedSize));
    }

    public class ManualLayoutControl : UIControl
    {
        public Action LayoutAction { get; set; }
        public Func<CGSize, CGSize> MeasureAction { get; set; }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            LayoutAction?.Invoke();
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return MeasureAction?.Invoke(size) ?? base.SizeThatFits(size);
        }

        public override CGSize IntrinsicContentSize => SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, UIViewWrapper.UnboundedSize));
    }
}
