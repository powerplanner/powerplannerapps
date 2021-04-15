using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSLinearLayout : iOSView<LinearLayout, UILinearLayout>
    {
        protected override void ApplyProperties(LinearLayout oldView, LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            //View.Axis = UILayoutConstraintAxis.Vertical;

            ReconcileList(
                oldView?.Children,
                newView.Children,
                insert: (i, v) => View.InsertSubview(v.CreateUIView(VxParentView), (nint)i),
                remove: (i) => View.Subviews.ElementAt(i).RemoveFromSuperview(),
                replace: (i, v) =>
                {
                    View.Subviews.ElementAt(i).RemoveFromSuperview();
                    View.InsertSubview(v.CreateUIView(VxParentView), (nint)i);
                },
                clear: () =>
                {
                    foreach (var subview in View.Subviews.ToArray())
                    {
                        subview.RemoveFromSuperview();
                    }
                }
                );

            //View.RemoveConstraints(View.Constraints);

            //UIView prevView = null;

            //for (int i = 0; i < newView.Children.Count; i++)
            //{
            //    var uiView = View.Subviews[i];
            //    var vxView = newView.Children[i];

            //    if (prevView == null)
            //    {
            //        uiView.PinToTop(View);
            //    }
            //    else
            //    {
            //        uiView.SetBelow(prevView, View);
            //    }

            //    if (i == newView.Children.Count - 1)
            //    {
            //        uiView.PinToBottom(View);
            //    }

            //    uiView.StretchWidth(View);

            //    prevView = uiView;
            //}
        }
    }


    public class UILinearLayout : UIView
    {
        public override void LayoutSubviews()
        {
            double y = 0;

            foreach (var subview in Subviews)
            {
                var size = subview.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
                subview.Frame = new CoreGraphics.CGRect(0, y, Frame.Width, size.Height);
                y += size.Height;
            }
        }

        public override CoreGraphics.CGSize SystemLayoutSizeFittingSize(CoreGraphics.CGSize size)
        {
            double height = 0;
            double width = 0;
            foreach (var subview in Subviews)
            {
                var subviewSize = subview.SystemLayoutSizeFittingSize(size);

                height += subviewSize.Height;
                width = Math.Max(subviewSize.Width, width);
            }

            return new CoreGraphics.CGSize(width, height);
            return base.SystemLayoutSizeFittingSize(size);
        }
    }
}