using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreAnimation;
using CoreGraphics;

namespace InterfacesiOS.Views
{
    public class BareUIEllipseView : BareUIView
    {
        private CAShapeLayer _ellipseLayer;

        public BareUIEllipseView()
        {
            _ellipseLayer = new CAShapeLayer();
            this.Layer.AddSublayer(_ellipseLayer);
        }

        public CGColor FillColor
        {
            get { return _ellipseLayer.FillColor; }
            set { _ellipseLayer.FillColor = value; }
        }

        private AspectRatios _aspectRatio;
        public AspectRatios AspectRatio
        {
            get { return _aspectRatio; }
            set { _aspectRatio = value; }
        }

        public override void LayoutSubviews()
        {
            UpdatePath();

            base.LayoutSubviews();
        }

        private void UpdatePath()
        {
            nfloat width = this.Bounds.Width;
            nfloat height = this.Bounds.Height;
            nfloat x = 0;
            nfloat y = 0;

            switch (AspectRatio)
            {
                case AspectRatios.Circle:
                    if (width < height)
                    {
                        y = (height - width) / 2f;
                        height = width;
                    }
                    else if (height < width)
                    {
                        x = (width - height) / 2f;
                        width = height;
                    }
                    break;
            }

            _ellipseLayer.Path = CGPath.EllipseFromRect(new CGRect(x, y, width, height));
        }

        public enum AspectRatios
        {
            Circle,
            Ellipse
        }
    }
}