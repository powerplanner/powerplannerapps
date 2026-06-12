using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTransparentContentMenuButton : iOSView<TransparentContentMenuButton, UIButtonContentView>
    {
        public iOSTransparentContentMenuButton()
        {
            View.IsAccessibilityElement = true;
            View.AccessibilityTraits = UIAccessibilityTrait.Button;
            View.ShowsMenuAsPrimaryAction = true;
        }

        protected override void ApplyProperties(TransparentContentMenuButton oldView, TransparentContentMenuButton newView)
        {
            base.ApplyProperties(oldView, newView);

            View.AccessibilityLabel = newView.AltText;

            ReconcileContentNew(oldView?.Content, newView.Content, view =>
            {
                View.Content = view?.CreateUIView(VxView);
            });

            if (newView.Menu != null && newView.Menu.Count > 0)
            {
                View.Menu = VxiOSContextMenu.CreateMenu(newView.Menu);
            }
            else
            {
                View.Menu = null;
            }
        }
    }
}
