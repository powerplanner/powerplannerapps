using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSListItemButton : iOSView<ListItemButton, UIContentView>
    {
        public iOSListItemButton()
        {
            UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer();

            tapRecognizer.AddTarget(() => VxView.Click?.Invoke());

            View.AddGestureRecognizer(tapRecognizer);
        }

        protected override void ApplyProperties(ListItemButton oldView, ListItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            ReconcileContentNew(oldView?.Content, newView.Content);
        }
    }
}
