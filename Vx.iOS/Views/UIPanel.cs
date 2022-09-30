using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class UIPanel : UIView
    {
        private List<UIViewWrapper> _arrangedSubviews = new List<UIViewWrapper>();
        public IReadOnlyList<UIViewWrapper> ArrangedSubviews => _arrangedSubviews as IReadOnlyList<UIViewWrapper>;

        public void AddArrangedSubview(UIViewWrapper subview)
        {
            _arrangedSubviews.Add(subview);
            base.AddSubview(subview.View);
            Invalidate();
        }

        public void RemoveArrangedSubview(UIViewWrapper subview)
        {
            _arrangedSubviews.Remove(subview);
            subview.View.RemoveFromSuperview();
            Invalidate();
        }

        public void RemoveArrangedSubviewAt(int index)
        {
            var subview = _arrangedSubviews[index];
            _arrangedSubviews.RemoveAt(index);
            subview.View.RemoveFromSuperview();
            Invalidate();
        }

        public void InsertArrangedSubview(UIViewWrapper subview, int index)
        {
            _arrangedSubviews.Insert(index, subview);
            base.InsertSubview(subview.View, index);
            Invalidate();
        }

        public void ClearArrangedSubviews()
        {
            foreach (var subview in _arrangedSubviews)
            {
                subview.View.RemoveFromSuperview();
            }
            _arrangedSubviews.Clear();
            Invalidate();
        }

        public override void AddSubview(UIView view)
        {
            throw new InvalidOperationException("Use AddArrangedSubview instead");
        }

        public override void InsertSubview(UIView view, nint atIndex)
        {
            throw new InvalidOperationException("Use InsertArrangedSubview instead");
        }

        public UIView View => this;

        public override CGSize IntrinsicContentSize => SizeThatFits(new CGSize(0, 0));

        public override CGSize SizeThatFits(CGSize size)
        {
            nfloat measuredWidth = 0, measuredHeight = 0;
            foreach (var child in ArrangedSubviews)
            {
                var childSize = child.SizeThatFits(size);

                measuredWidth = MaxF(measuredWidth, childSize.Width);
                measuredHeight = MaxF(measuredHeight, childSize.Height);
            }

            return new CGSize(measuredWidth, measuredHeight);
        }

        public override void LayoutSubviews()
        {
            foreach (var subview in ArrangedSubviews)
            {
                subview.Frame = new CGRect(0, 0, Frame.Width, Frame.Height);
            }
        }

        protected static nfloat MaxF(nfloat f1, nfloat f2)
        {
            if (f1 > f2)
            {
                return f1;
            }
            return f2;
        }

        private void Invalidate()
        {
            InvalidateIntrinsicContentSize();
            SetNeedsLayout();
        }
    }
}

