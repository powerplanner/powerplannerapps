using System;
using CoreGraphics;
using UIKit;

namespace Vx.iOS.Views
{
    public class UIViewWrapper
    {
        public UIView View { get; private set; }
        public Vx.Views.View VxView { get; private set; }

        public UIViewWrapper(UIView view, Vx.Views.View vxView)
        {
            View = view;
            VxView = vxView;
        }

        public CGSize IntrinsicContentSize
        {
            get
            {
                var size = View.IntrinsicContentSize;
                var margin = VxView.Margin.AsModified();
                if (size.Width != UIView.NoIntrinsicMetric)
                {
                    size.Width += margin.Width;
                }
                if (size.Height != UIView.NoIntrinsicMetric)
                {
                    size.Height += margin.Height;
                }

                return size;
            }
        }

        public void Arrange(CGRect pos, CGSize finalSize)
        {
            nfloat width, height;
            var margin = VxView.Margin.AsModified();

            if (!float.IsNaN(VxView.Width))
            {
                width = VxView.Width;
            }
            else
            {
                switch (VxView.HorizontalAlignment)
                {
                    case Vx.Views.HorizontalAlignment.Stretch:
                        width = finalSize.Width - margin.Width;
                        break;

                    default:
                        if (View.IntrinsicContentSize.Width == UIView.NoIntrinsicMetric)
                        {
                            width = finalSize.Width - margin.Width;
                        }
                        else
                        {
                            width = View.IntrinsicContentSize.Width;
                            if (width > finalSize.Width - margin.Width)
                            {
                                width = finalSize.Width - margin.Width;
                            }
                        }
                        break;
                }
            }

            if (!float.IsNaN(VxView.Height))
            {
                height = VxView.Height;
            }
            else
            {
                switch (VxView.VerticalAlignment)
                {
                    case Vx.Views.VerticalAlignment.Stretch:
                        height = finalSize.Height - margin.Height;
                        break;

                    default:
                        if (View.IntrinsicContentSize.Height == UIView.NoIntrinsicMetric)
                        {
                            height = finalSize.Height - margin.Height;
                        }
                        else
                        {
                            height = View.IntrinsicContentSize.Height;
                            if (height > finalSize.Height - margin.Height)
                            {
                                height = finalSize.Height - margin.Height;
                            }
                        }
                        break;
                }
            }

            nfloat childX, childY;

            switch (VxView.HorizontalAlignment)
            {
                case Vx.Views.HorizontalAlignment.Stretch:
                case Vx.Views.HorizontalAlignment.Left:
                    childX = pos.X + margin.Left;
                    break;

                case Vx.Views.HorizontalAlignment.Center:
                    nfloat center = (width + margin.Width) / 2.0f;
                    nfloat offset = width / 2.0f;
                    childX = pos.X + center - offset;
                    break;

                case Vx.Views.HorizontalAlignment.Right:
                    childX = pos.X + (finalSize.Width - width - margin.Right);
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (VxView.VerticalAlignment)
            {
                case Vx.Views.VerticalAlignment.Stretch:
                case Vx.Views.VerticalAlignment.Top:
                    childY = pos.Y + margin.Top;
                    break;

                case Vx.Views.VerticalAlignment.Center:
                    nfloat center = (height + margin.Height) / 2.0f;
                    nfloat offset = height / 2.0f;
                    childY = pos.Y + center - offset;
                    break;

                case Vx.Views.VerticalAlignment.Bottom:
                    childY = pos.Y + (finalSize.Height - height - margin.Bottom);
                    break;

                default:
                    throw new NotImplementedException();
            }

            View.Frame = new CGRect(childX, childY, width, height);
        }
    }
}

