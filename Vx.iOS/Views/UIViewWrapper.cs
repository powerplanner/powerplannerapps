using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Vx.iOS.Views
{
    /// <summary>
    /// Wraps a single <see cref="UIView"/> and holds the layout properties (margin, fixed size,
    /// min/max width, alignment, attached values like weight) for the Vx layout engine.
    ///
    /// This is the layout node of the pure manual measure/arrange engine: it never creates
    /// <see cref="NSLayoutConstraint"/> objects. Parents call <see cref="Measure"/> to discover a
    /// child's desired size and <see cref="Arrange"/> to position it by setting its
    /// <see cref="UIView.Frame"/>. Measurement results are cached and invalidated via a dirty flag.
    /// </summary>
    public class UIViewWrapper
    {
        /// <summary>
        /// Represents an effectively-unbounded available size used during measurement
        /// (the manual layout equivalent of "Auto"). Large but finite so arithmetic stays safe.
        /// </summary>
        public const float UnboundedSize = 1000000f;

        public UIViewWrapper(UIView view)
        {
            ViewOrGuide = view ?? throw new ArgumentNullException(nameof(view));
        }

        public NSObject ViewOrGuide { get; }
        public UIView View => ViewOrGuide as UIView;
        public UIView Superview => View.Superview;

        public void RemoveFromSuperview()
        {
            View.RemoveFromSuperview();
        }

        #region Layout properties

        private float _width = float.NaN;
        public float Width
        {
            get => _width;
            set => SetValue(ref _width, value);
        }

        private float _height = float.NaN;
        public float Height
        {
            get => _height;
            set => SetValue(ref _height, value);
        }

        private float _minWidth = 0;
        public float MinWidth
        {
            get => _minWidth;
            set => SetValue(ref _minWidth, value);
        }

        private float _maxWidth = float.PositiveInfinity;
        public float MaxWidth
        {
            get => _maxWidth;
            set => SetValue(ref _maxWidth, value);
        }

        private Vx.Views.Thickness _margin;
        public Vx.Views.Thickness Margin
        {
            get => _margin;
            set => SetValue(ref _margin, value.AsModified());
        }

        private Vx.Views.HorizontalAlignment _horizontalAlignment;
        public Vx.Views.HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set => SetValue(ref _horizontalAlignment, value);
        }

        private Vx.Views.VerticalAlignment _verticalAlignment;
        public Vx.Views.VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set => SetValue(ref _verticalAlignment, value);
        }

        private Dictionary<string, object> _attachedValues;

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            if (_attachedValues == null)
            {
                return defaultValue;
            }

            return (T)_attachedValues.GetValueOrDefault(key, defaultValue);
        }

        public void SetValue(string key, object val)
        {
            if (_attachedValues == null)
            {
                _attachedValues = new Dictionary<string, object>();
            }

            if (!object.Equals(_attachedValues.GetValueOrDefault(key), val))
            {
                _attachedValues[key] = val;
                InvalidateMeasure();
            }
        }

        private void SetValue<T>(ref T storage, T value)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                InvalidateMeasure();
            }
        }

        #endregion

        #region Measure / invalidation

        private bool _measureDirty = true;
        private CGSize _cachedAvailable;
        private CGSize _cachedDesired;

        /// <summary>
        /// Marks this node's cached measurement stale and propagates the need for a new layout
        /// pass up the view hierarchy (UIKit's <see cref="UIView.SetNeedsLayout"/> alone does not
        /// notify ancestors that their measurement changed, so we walk up explicitly).
        /// </summary>
        public void InvalidateMeasure()
        {
            _measureDirty = true;
            UIPanel.PropagateLayoutDirty(View?.Superview);
            View?.SetNeedsLayout();
        }

        /// <summary>
        /// Measures the desired size of this view (excluding its own margin) within the
        /// given available size, honoring the wrapper's fixed Width/Height and Min/Max width.
        /// Results are cached keyed on the available size.
        /// </summary>
        public CGSize Measure(CGSize available)
        {
            bool viewDirty = (View as UIPanel)?.IsMeasureDirty ?? false;

            if (!_measureDirty && !viewDirty && _cachedAvailable == available)
            {
                return _cachedDesired;
            }

            var result = MeasureCore(available);

            _cachedAvailable = available;
            _cachedDesired = result;
            _measureDirty = false;
            (View as UIPanel)?.ClearMeasureDirty();

            return result;
        }

        private CGSize MeasureCore(CGSize available)
        {
            bool hasWidth = !float.IsNaN(Width);
            bool hasHeight = !float.IsNaN(Height);

            nfloat w;
            nfloat h;

            if (hasWidth && hasHeight)
            {
                w = Width;
                h = Height;
            }
            else if (View is UIImageView && (hasWidth ^ hasHeight))
            {
                // When only one dimension is fixed on an image, preserve its aspect ratio
                var intrinsic = View.IntrinsicContentSize;
                if (intrinsic.Width > 0 && intrinsic.Height > 0)
                {
                    if (hasWidth)
                    {
                        w = Width;
                        h = (nfloat)Width * intrinsic.Height / intrinsic.Width;
                    }
                    else
                    {
                        h = Height;
                        w = (nfloat)Height * intrinsic.Width / intrinsic.Height;
                    }
                }
                else
                {
                    w = hasWidth ? Width : 0;
                    h = hasHeight ? Height : 0;
                }
            }
            else
            {
                nfloat constraintWidth = hasWidth ? (nfloat)Width : (IsBounded(available.Width) ? available.Width : UnboundedSize);
                nfloat constraintHeight = hasHeight ? (nfloat)Height : (IsBounded(available.Height) ? available.Height : UnboundedSize);

                var constraint = new CGSize(constraintWidth, constraintHeight);

                // Both our own panels and native/leaf controls report their content size via
                // SizeThatFits (panels route it to their MeasureContent override). We never use
                // SystemLayoutSizeFittingSize on the tree, which would spin up an internal
                // constraint solver per node.
                CGSize fit = View.SizeThatFits(constraint);

                // UIView's default SizeThatFits simply echoes the size it was given. Controls that
                // don't implement it meaningfully (e.g. custom UIControls that rely solely on an
                // intrinsic content size) would otherwise report the unbounded sentinel as their
                // size. Detect that echo on an unbounded axis and fall back to the intrinsic size.
                bool echoedUnboundedWidth = constraintWidth >= UnboundedSize && Math.Abs((double)(fit.Width - constraintWidth)) < 1;
                bool echoedUnboundedHeight = constraintHeight >= UnboundedSize && Math.Abs((double)(fit.Height - constraintHeight)) < 1;

                if (echoedUnboundedWidth || echoedUnboundedHeight || (fit.Width <= 0 && fit.Height <= 0))
                {
                    var intrinsic = View.IntrinsicContentSize;
                    if (intrinsic.Width > 0 || intrinsic.Height > 0)
                    {
                        fit = intrinsic;
                    }
                    else
                    {
                        // The view implements neither SizeThatFits nor an intrinsic content size,
                        // but it may be a composite native control built from internal Auto Layout
                        // (e.g. a header + rounded text field stack, a combo/date picker). Ask Auto
                        // Layout for the size that fits its own internal constraints. This is a
                        // single, cached, NON-recursive measurement of a leaf - our own panels
                        // implement MeasureContent and never reach this path, so this does not
                        // reintroduce tree-wide constraint solving.
                        var autoFit = MeasureViaInternalAutoLayout(constraintWidth, constraintHeight);

                        fit = new CGSize(
                            echoedUnboundedWidth ? autoFit.Width : (fit.Width > 0 ? fit.Width : autoFit.Width),
                            echoedUnboundedHeight ? autoFit.Height : (fit.Height > 0 ? fit.Height : autoFit.Height));
                    }
                }

                w = hasWidth ? Width : fit.Width;
                h = hasHeight ? Height : fit.Height;
            }

            if (MinWidth > 0 && w < MinWidth)
            {
                w = MinWidth;
            }
            if (!float.IsPositiveInfinity(MaxWidth) && w > MaxWidth)
            {
                w = MaxWidth;
            }
            if (w < 0)
            {
                w = 0;
            }
            if (h < 0)
            {
                h = 0;
            }

            return new CGSize(w, h);
        }

        #endregion

        #region Arrange

        /// <summary>
        /// Positions this view within the given slot rectangle (which INCLUDES this view's margin),
        /// honoring margin, alignment, and fixed/measured size, by setting <see cref="UIView.Frame"/>.
        /// </summary>
        public void Arrange(CGRect slot)
        {
            var m = Margin;

            nfloat availW = slot.Width - m.Left - m.Right;
            nfloat availH = slot.Height - m.Top - m.Bottom;
            if (availW < 0) availW = 0;
            if (availH < 0) availH = 0;

            var desired = Measure(new CGSize(availW, availH));

            bool stretchH = HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch && float.IsNaN(Width);
            bool stretchV = VerticalAlignment == Vx.Views.VerticalAlignment.Stretch && float.IsNaN(Height);

            nfloat w = stretchH ? availW : MinF(desired.Width, availW);
            nfloat h = stretchV ? availH : MinF(desired.Height, availH);

            nfloat x;
            switch (HorizontalAlignment)
            {
                case Vx.Views.HorizontalAlignment.Left:
                    x = slot.X + m.Left;
                    break;
                case Vx.Views.HorizontalAlignment.Right:
                    x = slot.X + slot.Width - m.Right - w;
                    break;
                case Vx.Views.HorizontalAlignment.Center:
                    x = slot.X + m.Left + (availW - w) / 2f;
                    break;
                default: // Stretch: fills when it can, otherwise (fixed Width) centers, like WPF/UWP.
                    x = slot.X + m.Left + (availW - w) / 2f;
                    break;
            }

            nfloat y;
            switch (VerticalAlignment)
            {
                case Vx.Views.VerticalAlignment.Top:
                    y = slot.Y + m.Top;
                    break;
                case Vx.Views.VerticalAlignment.Bottom:
                    y = slot.Y + slot.Height - m.Bottom - h;
                    break;
                case Vx.Views.VerticalAlignment.Center:
                    y = slot.Y + m.Top + (availH - h) / 2f;
                    break;
                default: // Stretch: fills when it can, otherwise (fixed Height) centers, like WPF/UWP.
                    y = slot.Y + m.Top + (availH - h) / 2f;
                    break;
            }

            View.Frame = new CGRect(x, y, w, h);
        }

        #endregion

        /// <summary>
        /// Measures a leaf view that sizes itself purely via its own internal Auto Layout
        /// constraints (no SizeThatFits, no intrinsic content size). The bounded axis is pinned at
        /// required priority and the unbounded axis is solved at the fitting-size level so the
        /// view reports its natural extent on that axis.
        /// </summary>
        private CGSize MeasureViaInternalAutoLayout(nfloat constraintWidth, nfloat constraintHeight)
        {
            bool widthBounded = constraintWidth < UnboundedSize;
            bool heightBounded = constraintHeight < UnboundedSize;

            var target = new CGSize(
                widthBounded ? constraintWidth : 0,
                heightBounded ? constraintHeight : 0);

            float horizontalPriority = widthBounded
                ? (float)UILayoutPriority.Required
                : (float)UILayoutPriority.FittingSizeLevel;
            float verticalPriority = heightBounded
                ? (float)UILayoutPriority.Required
                : (float)UILayoutPriority.FittingSizeLevel;

            return View.SystemLayoutSizeFittingSize(target, horizontalPriority, verticalPriority);
        }

        private static bool IsBounded(nfloat value)
        {
            return value > 0 && value < UnboundedSize;
        }

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
