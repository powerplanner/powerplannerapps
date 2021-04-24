using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSScrollView : iOSView<Vx.Views.ScrollView, UIScrollView>
    {
        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                View.RemoveAllConstraints();
                View.ClearAllSubviews();

                var child = view.CreateUIView(VxView);
                child.TranslatesAutoresizingMaskIntoConstraints = false;
                View.AddSubview(child);
                child.ConfigureForVerticalScrolling(View, view.Margin.Left, view.Margin.Top, view.Margin.Right, view.Margin.Bottom);
            });
        }
    }
}