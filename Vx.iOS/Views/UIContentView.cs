using System;
using CoreGraphics;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    /// <summary>
    /// A single-child panel with padding. Used by <see cref="Border"/>, the scroll view content
    /// host, button content hosts and the root component. Measures/arranges its one child within
    /// the panel's bounds minus padding.
    /// </summary>
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
                    base.ClearArrangedSubviews();

                    _content = value;

                    if (value != null)
                    {
                        base.AddArrangedSubview(value);
                    }

                    InvalidateMeasureSelf();
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
                    InvalidateMeasureSelf();
                }
            }
        }

        private void InvalidateMeasureSelf()
        {
            MarkMeasureDirty();
            SetNeedsLayout();
            // Invalidate our own intrinsic size as well, since when this content view is the
            // Auto-Layout-pinned Vx root (e.g. a self-sizing UITableView cell host), a change to
            // its content or padding must make the host re-query the height.
            InvalidateIntrinsicContentSize();
            PropagateLayoutDirty(Superview);
        }

        public override CGSize MeasureContent(CGSize available)
        {
            if (Content == null)
            {
                return CGSize.Empty;
            }

            nfloat availableWidth = available.Width;
            if (availableWidth > 0 && availableWidth < UIViewWrapper.UnboundedSize)
            {
                availableWidth -= Padding.Left + Padding.Right;
                if (availableWidth < 0) availableWidth = 0;
            }

            nfloat availableHeight = available.Height;
            if (availableHeight > 0 && availableHeight < UIViewWrapper.UnboundedSize)
            {
                availableHeight -= Padding.Top + Padding.Bottom;
                if (availableHeight < 0) availableHeight = 0;
            }

            var measured = Content.Measure(new CGSize(availableWidth, availableHeight));

            return new CGSize(
                measured.Width + Content.Margin.Left + Content.Margin.Right + Padding.Left + Padding.Right,
                measured.Height + Content.Margin.Top + Content.Margin.Bottom + Padding.Top + Padding.Bottom);
        }

        public override void ArrangeContent(CGSize size)
        {
            if (Content == null)
            {
                return;
            }

            nfloat width = size.Width - Padding.Left - Padding.Right;
            nfloat height = size.Height - Padding.Top - Padding.Bottom;
            if (width < 0) width = 0;
            if (height < 0) height = 0;

            Content.Arrange(new CGRect(Padding.Left, Padding.Top, width, height));
        }
    }
}
