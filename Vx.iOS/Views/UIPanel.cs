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
        private IReadOnlyList<UIViewWrapper> _readOnlyArrangedSubviews;
        public IReadOnlyList<UIViewWrapper> ArrangedSubviews
        {
            get
            {
                if (_readOnlyArrangedSubviews == null)
                {
                    _readOnlyArrangedSubviews = _arrangedSubviews as IReadOnlyList<UIViewWrapper>;
                }
                return _readOnlyArrangedSubviews;
            }
        }

        public void AddArrangedSubview(UIViewWrapper subview)
        {
            subview.View.TranslatesAutoresizingMaskIntoConstraints = false;
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
            subview.View.TranslatesAutoresizingMaskIntoConstraints = false;
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

        protected virtual void CustomUpdateConstraints()
        {
            foreach (var subview in ArrangedSubviews)
            {
                subview.SetConstraints(
                    new WrapperConstraint(this, NSLayoutAttribute.Left),
                    new WrapperConstraint(this, NSLayoutAttribute.Top),
                    new WrapperConstraint(this, NSLayoutAttribute.Right),
                    new WrapperConstraint(this, NSLayoutAttribute.Bottom),
                    this,
                    this);
            }
        }

        /// <summary>
        /// Do NOT override this, override the CustomUpdateConstraints instead.
        /// </summary>
        public override void UpdateConstraints()
        {
            CustomUpdateConstraints();

            base.UpdateConstraints();
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
            SetNeedsUpdateConstraints();
        }
    }
}

