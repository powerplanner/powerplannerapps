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
                var modifiedMargin = newView.Content.Margin.AsModified();
                subview.ConfigureForVerticalScrolling(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);
            });
        }
    }
}