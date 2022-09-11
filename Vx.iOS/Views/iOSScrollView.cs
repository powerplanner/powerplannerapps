﻿using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSScrollView : iOSView<Vx.Views.ScrollView, UIVxScrollView>
    {
        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            ReconcileContentNew(oldView?.Content, newView.Content, contentView =>
            {
                View.Content = contentView.CreateUIView(VxView);
            });
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

        public override CGSize IntrinsicContentSize
        {
            get
            {
                if (Content != null)
                {
                    var contentSize = Content.IntrinsicContentSize;
                    return new CGSize(contentSize.Width + Padding.Width, contentSize.Height + Padding.Height);
                }

                return new CGSize(Padding.Width, Padding.Height);
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            if (Content == null)
            {
                return new CGSize(Padding.Width, Padding.Height);
            }

            size = new CGSize(size.Width - Padding.Width, nfloat.MaxValue);
            var measured = Content.Measure(size);
            return new CGSize(measured.Width + Padding.Width, measured.Height + Padding.Height);
        }

        private UIViewWrapper _prevContent;
        private CGSize _prevSize;
        private Thickness _prevPadding;
        public override void LayoutSubviews()
        {
            if (Content == _prevContent && Frame.Size == _prevSize && Padding == _prevPadding)
            {
                return;
            }

            _prevContent = Content;
            _prevPadding = Padding;
            _prevSize = Frame.Size;

            if (Content != null)
            {
                nfloat width = Frame.Width - Padding.Width;
                nfloat height = nfloat.MaxValue;

                var measured = Content.Measure(new CGSize(width, height));
                Content.Arrange(new CGPoint(Padding.Left, Padding.Top), new CGSize(width, measured.Height));
                ContentSize = new CGSize(width + Padding.Width, measured.Height + Padding.Height);
            }
            else
            {
                ContentSize = new CGSize(Padding.Width, Padding.Height);
            }
        }
    }
}