using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSLinearLayout : iOSView<LinearLayout, UIView>
    {
        protected override void ApplyProperties(LinearLayout oldView, LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            //View.Orientation = newView.Orientation;

            ReconcileList(
                oldView?.Children,
                newView.Children,
                insert: (i, v) =>
                {
                    var childView = v.CreateUIView(VxParentView);
                    childView.TranslatesAutoresizingMaskIntoConstraints = false;
                    View.InsertSubview(childView, (nint)i);
                },
                remove: (i) => View.Subviews.ElementAt(i).RemoveFromSuperview(),
                replace: (i, v) =>
                {
                    View.Subviews.ElementAt(i).RemoveFromSuperview();

                    var childView = v.CreateUIView(VxParentView);
                    childView.TranslatesAutoresizingMaskIntoConstraints = false;
                    View.InsertSubview(childView, (nint)i);
                },
                clear: () =>
                {
                    foreach (var subview in View.Subviews.ToArray())
                    {
                        subview.RemoveFromSuperview();
                    }
                }
                );

            View.RemoveConstraints(View.Constraints);

            UIView prevView = null;

            for (int i = 0; i < newView.Children.Count; i++)
            {
                var uiView = View.Subviews[i];
                var vxView = newView.Children[i];

                if (prevView == null)
                {
                    uiView.PinToTop(View);
                }
                else
                {
                    uiView.SetBelow(prevView, View);
                }

                if (i == newView.Children.Count - 1)
                {
                    View.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:[view]-(>=0)-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", uiView)));
                    //uiView.PinToBottom(View);
                }

                uiView.StretchWidth(View);

                prevView = uiView;
            }
        }
    }


    public class UILinearLayout : UIView
    {
        private Orientation _orientation;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (value != _orientation)
                {
                    _orientation = value;
                    SetNeedsLayout();
                }
            }
        }

        public override CoreGraphics.CGSize SystemLayoutSizeFittingSize(CoreGraphics.CGSize size)
        {
            double height = 0;
            double width = 0;
            foreach (var subview in Subviews)
            {
                var subviewSize = subview.SystemLayoutSizeFittingSize(size);

                if (Orientation == Orientation.Vertical)
                {
                    height += subviewSize.Height;
                    width = Math.Max(subviewSize.Width, width);
                }
                else
                {
                    height = Math.Max(subviewSize.Height, height);
                    width += subviewSize.Width;
                }
            }

            return new CoreGraphics.CGSize(width, height);
        }

        public override void LayoutSubviews()
        {
            if (Orientation == Orientation.Vertical)
            {
                double y = 0;

                foreach (var subview in Subviews)
                {
                    var size = subview.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
                    subview.Frame = new CoreGraphics.CGRect(0, y, Frame.Width, size.Height);
                    y += size.Height;
                }
            }
            else
            {
                double x = 0;

                foreach (var subview in Subviews)
                {
                    var size = subview.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
                    subview.Frame = new CoreGraphics.CGRect(x, 0, Frame.Width, size.Height);
                    x += size.Width;
                }
            }
        }

        public override void SubviewAdded(UIView uiview)
        {
            base.SubviewAdded(uiview);

            base.SetNeedsLayout();
        }

        public override void WillRemoveSubview(UIView uiview)
        {
            base.WillRemoveSubview(uiview);

            base.InvalidateIntrinsicContentSize();
        }
    }
}