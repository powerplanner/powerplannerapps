using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public abstract class iOSView<V, N> : NativeView<V, N> where V : View where N : UIView
    {
        public iOSView()
        {
            View = Activator.CreateInstance<N>();
            //View.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        public iOSView(N view)
        {
            View = view;
            //view.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            View.Alpha = newView.Opacity;
        }

        protected void ReconcileContent(View oldContent, View newContent, Action<UIView> afterSubviewAddedAction)
        {
            VxReconciler.Reconcile(oldContent, newContent, view =>
            {
                View.RemoveAllConstraints();
                View.ClearAllSubviews();

                var child = view.CreateUIView(VxView);
                child.TranslatesAutoresizingMaskIntoConstraints = false;
                View.AddSubview(child);
                afterSubviewAddedAction(child);
            });
        }

        protected void ReconcileContent(View oldContent, View newContent)
        {
            ReconcileContent(oldContent, newContent, subview =>
            {
                var modifiedMargin = newContent.Margin.AsModified();
                subview.StretchWidthAndHeight(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);

                // Prevent this from stretching and filling horizontal width
                subview.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Horizontal);
            });
        }
    }
}