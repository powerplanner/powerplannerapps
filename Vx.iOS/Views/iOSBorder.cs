using System;
using InterfacesiOS.Helpers;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSBorder : iOSView<Vx.Views.Border, UIContentView>
    {
        public iOSBorder()
        {
        }

        protected override void ApplyProperties(Border oldView, Border newView)
        {
            base.ApplyProperties(oldView, newView);

            View.BackgroundColor = newView.BackgroundColor.ToUI();
            View.Layer.BorderWidth = newView.BorderThickness.Top;
            View.Layer.BorderColor = newView.BorderColor.ToUI().CGColor;
            View.Layer.CornerRadius = newView.CornerRadius;
            View.Padding = newView.Padding;

            ReconcileContentNew(oldView?.Content, newView.Content);
        }
    }
}
