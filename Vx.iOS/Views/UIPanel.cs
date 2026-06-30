using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    /// <summary>
    /// Base class for all Vx-managed iOS panels. Implements a pure manual two-phase
    /// (measure / arrange) layout: children are positioned by setting their frames, never via
    /// <see cref="NSLayoutConstraint"/>. The default implementation is an overlay (FrameLayout):
    /// every child is measured against, and arranged into, the full available space.
    /// Subclasses override <see cref="MeasureContent"/> and <see cref="ArrangeContent"/>.
    /// </summary>
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
            // Manual layout: we set frames directly, so the autoresizing-mask -> constraints
            // translation must be ENABLED (otherwise the view would require explicit constraints).
            subview.View.TranslatesAutoresizingMaskIntoConstraints = true;
            _arrangedSubviews.Add(subview);
            base.AddSubview(subview.View);
            Invalidate();
        }

        public void RemoveArrangedSubview(UIViewWrapper subview)
        {
            _arrangedSubviews.Remove(subview);
            subview.RemoveFromSuperview();
            Invalidate();
        }

        public void RemoveArrangedSubviewAt(int index)
        {
            var subview = _arrangedSubviews[index];
            _arrangedSubviews.RemoveAt(index);
            subview.RemoveFromSuperview();
            Invalidate();
        }

        public void InsertArrangedSubview(UIViewWrapper subview, int index)
        {
            subview.View.TranslatesAutoresizingMaskIntoConstraints = true;
            _arrangedSubviews.Insert(index, subview);
            base.InsertSubview(subview.View, index);
            Invalidate();
        }

        public void ClearArrangedSubviews()
        {
            foreach (var subview in _arrangedSubviews)
            {
                subview.RemoveFromSuperview();
            }
            _arrangedSubviews.Clear();
            Invalidate();
        }

        /// <summary>
        /// Do not call this. Call AddArrangedSubview instead.
        /// </summary>
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
        [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override void InsertSubview(UIView view, nint atIndex)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            // Note that cannot throw here since context menus add to the view
            base.InsertSubview(view, atIndex);
        }

        public UIView View => this;

        #region Measure / arrange

        /// <summary>
        /// Measures the desired size of this panel's content for the given available size.
        /// The default (overlay) implementation returns the size needed to contain the largest child.
        /// </summary>
        public virtual CGSize MeasureContent(CGSize available)
        {
            nfloat width = 0;
            nfloat height = 0;

            foreach (var child in ArrangedSubviews)
            {
                var measured = child.Measure(available);
                width = MaxF(width, measured.Width + child.Margin.Left + child.Margin.Right);
                height = MaxF(height, measured.Height + child.Margin.Top + child.Margin.Bottom);
            }

            return new CGSize(width, height);
        }

        /// <summary>
        /// Arranges this panel's content within a rect of the given size (origin 0,0).
        /// The default (overlay) implementation arranges every child into the full rect,
        /// honoring each child's own alignment.
        /// </summary>
        public virtual void ArrangeContent(CGSize size)
        {
            var slot = new CGRect(0, 0, size.Width, size.Height);
            foreach (var child in ArrangedSubviews)
            {
                child.Arrange(slot);
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return MeasureContent(size);
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                // Only relevant when this panel is the single Vx root pinned by Auto Layout (e.g.
                // inside a scroll view or a self-sizing table cell). Internally nested panels are
                // sized by their parent via frames and never consult this.
                nfloat width = Bounds.Width > 0 ? Bounds.Width : UIViewWrapper.UnboundedSize;
                return MeasureContent(new CGSize(width, UIViewWrapper.UnboundedSize));
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (_holdingOffApplyingChanges)
            {
                _hasHeldOffChanges = true;
                return;
            }

            ArrangeContent(Bounds.Size);
            _measureDirty = false;

            // A panel's intrinsic height can depend on its width (e.g. wrapping text, or a
            // self-sizing UITableView cell whose width is only known after layout). When this panel
            // is the Auto-Layout-pinned Vx root (a table cell host or scroll content), its
            // IntrinsicContentSize was first computed before a real width was available; once the
            // real width arrives we must invalidate so the host re-queries the correct height.
            if (_lastLaidOutWidth != Bounds.Width)
            {
                _lastLaidOutWidth = Bounds.Width;
                InvalidateIntrinsicContentSize();
            }
        }

        private nfloat _lastLaidOutWidth = -1;

        #endregion

        #region Dirty tracking

        private bool _measureDirty = true;
        public bool IsMeasureDirty => _measureDirty;
        public void ClearMeasureDirty() => _measureDirty = false;

        /// <summary>
        /// Marks this panel (and its measurement cache) dirty and propagates the dirtiness up the
        /// hierarchy so ancestors re-measure on the next layout pass. Returns true if it was
        /// already dirty.
        /// </summary>
        internal bool MarkMeasureDirty()
        {
            bool was = _measureDirty;
            _measureDirty = true;
            return was;
        }

        /// <summary>
        /// Walks up from the given view marking every ancestor panel's measurement cache dirty and
        /// scheduling a layout pass, stopping early once an already-dirty panel is reached.
        /// </summary>
        internal static void PropagateLayoutDirty(UIView start)
        {
            var v = start;
            while (v != null)
            {
                v.SetNeedsLayout();
                if (v is UIPanel p)
                {
                    // Invalidate UIKit's intrinsic-size cache too: when this panel is the
                    // Auto-Layout-pinned Vx root (a self-sizing UITableView cell host or scroll
                    // content), a content change must make the host re-query the new height. This
                    // is essential for recycled cells, whose width is unchanged but content differs.
                    p.InvalidateIntrinsicContentSize();
                    if (p.MarkMeasureDirty())
                    {
                        break;
                    }
                }
                v = v.Superview;
            }
        }

        private void Invalidate()
        {
            if (_holdingOffApplyingChanges)
            {
                _hasHeldOffChanges = true;
                return;
            }

            _measureDirty = true;
            SetNeedsLayout();
            InvalidateIntrinsicContentSize();
            PropagateLayoutDirty(Superview);
        }

        private bool _holdingOffApplyingChanges;
        private bool _hasHeldOffChanges;
        public void HoldOffApplyingChanges()
        {
            _holdingOffApplyingChanges = true;
            _hasHeldOffChanges = false;
        }

        public void ApplyAnyHeldChanges()
        {
            var shouldUpdate = _hasHeldOffChanges;

            _holdingOffApplyingChanges = false;
            _hasHeldOffChanges = false;

            if (shouldUpdate)
            {
                Invalidate();
            }
        }

        #endregion

        protected static nfloat MaxF(nfloat f1, nfloat f2)
        {
            return f1 > f2 ? f1 : f2;
        }

        protected static nfloat MinF(nfloat f1, nfloat f2)
        {
            return f1 < f2 ? f1 : f2;
        }
    }
}
