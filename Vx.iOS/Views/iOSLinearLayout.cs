using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSLinearLayout : iOSView<LinearLayout, StretchingUIView>
    {
        protected override void ApplyProperties(LinearLayout oldView, LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            // Temp background color for debugging purposes
            //View.BackgroundColor = UIColor.FromRGBA(0, 0, 255, 15);
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
            Vx.Views.View prevVxView = null;
            var totalWeight = newView.TotalWeight();
            UIView firstWeighted = null;
            float firstWeightedValue = 0;
            var isVertical = newView.Orientation == Orientation.Vertical;

            for (int i = 0; i < newView.Children.Count; i++)
            {
                var uiView = View.Subviews[i];
                var vxView = newView.Children[i];

                if (prevView == null)
                {
                    if (isVertical)
                    {
                        uiView.PinToTop(View, vxView.Margin.Top);
                    }
                    else
                    {
                        uiView.PinToLeft(View, vxView.Margin.Left);
                    }
                }
                else
                {
                    if (isVertical)
                    {
                        uiView.SetBelow(prevView, View, vxView.Margin.Top + prevVxView.Margin.Bottom);
                    }
                    else
                    {
                        uiView.SetToRightOf(prevView, View, vxView.Margin.Left + prevVxView.Margin.Right);
                    }
                }

                var weight = LinearLayout.GetWeight(vxView);
                if (weight > 0)
                {
                    uiView.SetContentHuggingPriority(0, isVertical ? UILayoutConstraintAxis.Vertical : UILayoutConstraintAxis.Horizontal);

                    if (uiView is StretchingUIView stretchingUIView)
                    {
                        stretchingUIView.StretchingOrientation = newView.Orientation;
                    }

                    if (firstWeighted == null)
                    {
                        firstWeighted = uiView;
                        firstWeightedValue = weight;
                    }
                    else
                    {
                        if (isVertical)
                        {
                            View.AddConstraint(NSLayoutConstraint.Create(
                                uiView,
                                NSLayoutAttribute.Height,
                                NSLayoutRelation.Equal,
                                firstWeighted,
                                NSLayoutAttribute.Height,
                                weight / firstWeightedValue, 0));
                        }
                        else
                        {
                            View.AddConstraint(NSLayoutConstraint.Create(
                                uiView,
                                NSLayoutAttribute.Width,
                                NSLayoutRelation.Equal,
                                firstWeighted,
                                NSLayoutAttribute.Width,
                                weight / firstWeightedValue, 0));
                        }
                    }
                }
                else
                {
                    uiView.SetContentHuggingPriority(250, isVertical ? UILayoutConstraintAxis.Vertical : UILayoutConstraintAxis.Horizontal);

                    if (uiView is StretchingUIView stretchingUIView)
                    {
                        stretchingUIView.StretchingOrientation = null;
                    }
                }

                if (i == newView.Children.Count - 1)
                {
                    if (totalWeight == 0)
                    {
                        if (isVertical)
                        {
                            View.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:[view]-(>={vxView.Margin.Bottom})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", uiView)));
                        }
                        else
                        {
                            View.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:[view]-(>={vxView.Margin.Right})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", uiView)));
                        }
                    }
                    else
                    {
                        if (isVertical)
                        {
                            uiView.PinToBottom(View, vxView.Margin.Bottom);
                        }
                        else
                        {
                            uiView.PinToRight(View, vxView.Margin.Right);
                        }
                    }
                }

                if (isVertical)
                {
                    uiView.StretchWidth(View, vxView.Margin.Left, vxView.Margin.Right);
                }
                else
                {
                    uiView.StretchHeight(View, vxView.Margin.Top, vxView.Margin.Bottom);
                }

                prevView = uiView;
                prevVxView = vxView;
            }
        }
    }

    public class StretchingUIView : UIView
    {
        private Orientation? _stretchingOrientation;
        public Orientation? StretchingOrientation
        {
            get => _stretchingOrientation;
            set
            {
                if (!object.Equals(_stretchingOrientation, value))
                {
                    _stretchingOrientation = value;
                    InvalidateIntrinsicContentSize();
                }
            }
        }

        public override CoreGraphics.CGSize IntrinsicContentSize
        {
            get
            {
                if (StretchingOrientation == null)
                {
                    return new CoreGraphics.CGSize(-1, -1);
                }

                if (StretchingOrientation == Orientation.Vertical)
                {
                    return new CoreGraphics.CGSize(0, 50000);
                }
                else
                {
                    return new CoreGraphics.CGSize(50000, 0);
                }
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