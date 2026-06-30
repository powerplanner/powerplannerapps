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
        // TODO: Maybe I just need to implement my own scroll view? Can't figure out how to get the width passed to the child with auto layout...
        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

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

        public UIViewWrapper Content
        {
            get => _contentView.Content;
            set => _contentView.Content = value;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Vertical scrolling: bound the content's width to our viewport and let it grow
            // unbounded vertically, then size the scrollable content region to the result.
            nfloat width = Bounds.Width;
            var desired = _contentView.MeasureContent(new CGSize(width, UIViewWrapper.UnboundedSize));

            nfloat contentHeight = MaxF(desired.Height, Bounds.Height);
            _contentView.Frame = new CGRect(0, 0, width, contentHeight);
            ContentSize = new CGSize(width, contentHeight);
        }

        private static nfloat MaxF(nfloat a, nfloat b) => a > b ? a : b;
    }

    public class UIVxScrollView : UIScrollView
    {
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
    }
}