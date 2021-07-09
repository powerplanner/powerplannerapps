using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSBorder : iOSView<Vx.Views.Border, UIView>
    {
        public iOSBorder()
        {
        }

        protected override void ApplyProperties(Border oldView, Border newView)
        {
            base.ApplyProperties(oldView, newView);

            View.BackgroundColor = newView.BackgroundColor.ToUI();
            // Padding is a TODO

            ReconcileContent(oldView?.Content, newView.Content);
        }
    }
}
