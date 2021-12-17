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
            var childView = v.CreateUIView(VxParentView);
            childView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.InsertSubview(childView, i);
            var modifiedMargin = v.Margin.AsModified();
            childView.StretchWidthAndHeight(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);

            // Prevent this from stretching and filling horizontal width
            childView.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Horizontal);
        }
    }
}
