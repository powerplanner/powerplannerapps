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

        /// <summary>
        /// Do not call this. Call AddArrangedSubview instead.
        /// </summary>
        /// <param name="view"></param>
        [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override void AddSubview(UIView view)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            // Note that cannot throw here since context menus add to the view
            base.AddSubview(view);
        }

        /// <summary>
        /// Do not call this. Call InsertArrangedSubview instead.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="atIndex"></param>
        [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override void InsertSubview(UIView view, nint atIndex)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            // Note that cannot throw here since context menus add to the view
            base.InsertSubview(view, atIndex);
        }

        public UIView View => this;

        /// <summary>
        /// Automatically called
        /// </summary>
        public virtual void ArrangeSubviews()
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
        /// Do NOT override this, override ArrangeSubviews
        /// </summary>
        public override void UpdateConstraints()
        {
            ArrangeSubviews();

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

