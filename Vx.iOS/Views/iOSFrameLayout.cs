using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSFrameLayout : iOSView<FrameLayout, UIView>
    {
        protected override void ApplyProperties(FrameLayout oldView, FrameLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.BackgroundColor = newView.BackgroundColor.ToUI();

            ReconcileList(
                oldView?.Children,
                newView.Children,
                insert: Insert,
                remove: (i) =>
                {
                    View.Subviews[i].RemoveFromSuperview();
                },
                replace: (i, v) =>
                {
                    View.Subviews[i].RemoveFromSuperview();
                    Insert(i, v);
                },
                clear: () =>
                {
                    foreach (var subview in View.Subviews)
                    {
                        subview.RemoveFromSuperview();
                    }
                });
        }

        private void Insert(int i, View v)
        {
            var childView = v.CreateUIView(VxParentView).View;
            childView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.InsertSubview(childView, i);
            var modifiedMargin = v.Margin.AsModified();

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
            {
                // Left
                v.HorizontalAlignment == HorizontalAlignment.Left || v.HorizontalAlignment == HorizontalAlignment.Stretch
                    ? childView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, modifiedMargin.Left)
                    : childView.LeftAnchor.ConstraintGreaterThanOrEqualTo(View.LeftAnchor, modifiedMargin.Left),

                // Top
                v.VerticalAlignment == VerticalAlignment.Top || v.VerticalAlignment == VerticalAlignment.Stretch
                    ? childView.TopAnchor.ConstraintEqualTo(View.TopAnchor, modifiedMargin.Top)
                    : childView.TopAnchor.ConstraintGreaterThanOrEqualTo(View.TopAnchor, modifiedMargin.Top),

                // Right
                v.HorizontalAlignment == HorizontalAlignment.Right || v.HorizontalAlignment == HorizontalAlignment.Stretch
                    ? childView.RightAnchor.ConstraintEqualTo(View.RightAnchor, modifiedMargin.Right)
                    : childView.RightAnchor.ConstraintLessThanOrEqualTo(View.RightAnchor, modifiedMargin.Right),

                // Bottom
                v.VerticalAlignment == VerticalAlignment.Bottom || v.VerticalAlignment == VerticalAlignment.Stretch
                    ? childView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, modifiedMargin.Bottom)
                    : childView.BottomAnchor.ConstraintLessThanOrEqualTo(View.BottomAnchor, modifiedMargin.Bottom),
            });

            if (v.HorizontalAlignment == HorizontalAlignment.Center)
            {
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
                {
                    childView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor)
                });
            }

            if (v.VerticalAlignment == VerticalAlignment.Center)
            {
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
                {
                    childView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
                });
            }

            // Prevent this from stretching and filling horizontal width
            childView.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Horizontal);
            childView.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Vertical);
        }
    }
}
