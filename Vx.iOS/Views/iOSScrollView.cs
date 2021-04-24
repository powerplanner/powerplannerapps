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
                subview.ConfigureForVerticalScrolling(View, newView.Content.Margin.Left, newView.Content.Margin.Top, newView.Content.Margin.Right, newView.Content.Margin.Bottom);
            });
        }
    }
}