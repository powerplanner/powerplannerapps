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
    public class iOSScrollView : iOSView<Vx.Views.ScrollView, UIScrollView>
    {
        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            ReconcileContent(oldView?.Content, newView.Content, subview =>
            {
                ApplyMargins(subview, newView.Content);
            }, afterTransfer: subview =>
            {
                if (oldView.Content.Margin != newView.Content.Margin)
                {
                    ApplyMargins(subview, newView.Content, removeExisting: true);
                }
            });
        }

        private void ApplyMargins(UIView subview, View subVxView, bool removeExisting = false)
        {
            var modifiedMargin = subVxView.Margin.AsModified();
            subview.TranslatesAutoresizingMaskIntoConstraints = false;
            subview.ConfigureForVerticalScrolling(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom, removeExisting: removeExisting);
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