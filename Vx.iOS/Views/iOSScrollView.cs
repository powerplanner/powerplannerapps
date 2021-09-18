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

            ReconcileContent(oldView?.Content, newView.Content, subview =>
            {
                ApplyMargins(subview, newView.Content);
            }, afterTransfer: subview =>
            {
                if (oldView.Content.Margin != newView.Content.Margin)
                {
                    ApplyMargins(subview, newView.Content, removeExisting: true);
                }
            });
        }

        private void ApplyMargins(UIView subview, View subVxView, bool removeExisting = false)
        {
            var modifiedMargin = subVxView.Margin.AsModified();
            subview.ConfigureForVerticalScrolling(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom, removeExisting: removeExisting);
        }
    }
}