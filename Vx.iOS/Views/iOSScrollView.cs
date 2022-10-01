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
            _contentView = new UIContentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            AddSubview(_contentView);
            _contentView.ConfigureForVerticalScrolling(this);
        }

        public UIViewWrapper Content
        {
            get => _contentView.Content;
            set => _contentView.Content = value;
        }
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