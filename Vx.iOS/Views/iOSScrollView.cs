using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSScrollView : iOSView<Vx.Views.ScrollView, UIScrollViewWithPreferredMax>
    {
        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.MaximumZoomScale = newView.MaximumZoomScale;
            View.MinimumZoomScale = newView.MinimumZoomScale;
            View.ConfigureZoom(newView.MaximumZoomScale != newView.MinimumZoomScale);

            ReconcileContentNew(oldView?.Content, newView.Content, changedView =>
            {
                View.Content = changedView.CreateUIView(VxView);
            });
        }
    }

    public class UIScrollViewWithPreferredMax : UIScrollView
    {
        private UIContentView _contentView;

        public UIScrollViewWithPreferredMax()
        {
            // Manual layout: the content view is positioned via its frame in LayoutSubviews,
            // so the autoresizing-mask -> constraints translation must be enabled and we never
            // configure Auto Layout scrolling constraints.
            _contentView = new UIContentView
            {
                TranslatesAutoresizingMaskIntoConstraints = true
            };
            AddSubview(_contentView);
        }

        private bool _zoomEnabled;
        public void ConfigureZoom(bool enabled)
        {
            if (_zoomEnabled == enabled)
            {
                return;
            }

            if (enabled)
            {
                this.ViewForZoomingInScrollView += delegate { return _contentView; };
            }
            else
            {
                this.ViewForZoomingInScrollView = null;
            }

            _zoomEnabled = enabled;
        }

        public UIViewWrapper Content
        {
            get => _contentView.Content;
            set => _contentView.Content = value;
        }

        private bool _horizontalScrollEnabled;
        public bool HorizontalScrollEnabled
        {
            get => _horizontalScrollEnabled;
            set
            {
                if (_horizontalScrollEnabled == value)
                {
                    return;
                }
                _horizontalScrollEnabled = value;
                InvalidateIntrinsicContentSize();
                SetNeedsLayout();
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (HorizontalScrollEnabled)
            {
                var desired = _contentView.MeasureContent(new CGSize(UIViewWrapper.UnboundedSize, UIViewWrapper.UnboundedSize));

                nfloat contentHeight = MaxF(desired.Height, Bounds.Height);
                nfloat contentWidth = MaxF(desired.Width, Bounds.Width);
                _contentView.Frame = new CGRect(0, 0, contentWidth, contentHeight);
                ContentSize = new CGSize(contentWidth, contentHeight);
            }

            else
            {
                // Vertical scrolling: bound the content's width to our viewport and let it grow
                // unbounded vertically, then size the scrollable content region to the result.
                nfloat width = Bounds.Width;
                var desired = _contentView.MeasureContent(new CGSize(width, UIViewWrapper.UnboundedSize));

                nfloat contentHeight = MaxF(desired.Height, Bounds.Height);
                _contentView.Frame = new CGRect(0, 0, width, contentHeight);
                ContentSize = new CGSize(width, contentHeight);
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            // Report the natural height of our content (measured at our width, growing
            // vertically) so that parents which want to hug their content - like an in-place
            // popup card - can size to the content instead of collapsing us to zero (a plain
            // UIScrollView has no intrinsic content size). When our height is bounded we cap at
            // that height and scroll internally.

            if (HorizontalScrollEnabled)
            {
                var desired = _contentView.MeasureContent(new CGSize(UIViewWrapper.UnboundedSize, UIViewWrapper.UnboundedSize));

                nfloat height = desired.Height;
                if (size.Height > 0 && size.Height < UIViewWrapper.UnboundedSize)
                {
                    height = MinF(height, size.Height);
                }

                nfloat width = desired.Width;
                if (size.Width > 0 && size.Width < UIViewWrapper.UnboundedSize)
                {
                    width = MinF(width, size.Width);
                }

                return new CGSize(width, height);
            }

            else
            {
                nfloat width = size.Width;
                var desired = _contentView.MeasureContent(new CGSize(width, UIViewWrapper.UnboundedSize));

                nfloat height = desired.Height;
                if (size.Height > 0 && size.Height < UIViewWrapper.UnboundedSize)
                {
                    height = MinF(height, size.Height);
                }

                return new CGSize(width, height);
            }
        }

        private static nfloat MaxF(nfloat a, nfloat b) => a > b ? a : b;
        private static nfloat MinF(nfloat a, nfloat b) => a < b ? a : b;
    }
}