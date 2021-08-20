﻿using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Vx.iOS.Views;
using Vx.Views;

namespace Vx.iOS
{
    public class iOSNativeComponent : UIView, INativeComponent
    {
        public Action<UIView> AfterViewChanged { get; set; }

        public SizeF ComponentSize => new SizeF((float)this.Frame.Width, (float)this.Frame.Height);

        public event EventHandler<SizeF> ComponentSizeChanged;

        public iOSNativeComponent()
        {
            
        }

        private SizeF _currSize;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var newSize = ComponentSize;
            if (_currSize != newSize)
            {
                _currSize = newSize;
                ComponentSizeChanged?.Invoke(this, newSize);
            }
        }

        public void ChangeView(View view)
        {
            foreach (var subview in base.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            base.RemoveConstraints(base.Constraints);

            if (view == null)
            {
                return;
            }

            var uiView = view.CreateUIView(null);
            uiView.TranslatesAutoresizingMaskIntoConstraints = false;

            base.Add(uiView);

            var modifiedMargin = view.Margin.AsModified();
            uiView.StretchWidthAndHeight(this, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);

            AfterViewChanged?.Invoke(uiView);
        }
    }
}