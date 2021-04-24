using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTransparentContentButton : iOSView<TransparentContentButton, UIView>
    {
        public iOSTransparentContentButton()
        {
            UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer();

            tapRecognizer.AddTarget(() => VxView.Click?.Invoke());

            View.AddGestureRecognizer(tapRecognizer);
        }

        protected override void ApplyProperties(TransparentContentButton oldView, TransparentContentButton newView)
        {
            base.ApplyProperties(oldView, newView);

            ReconcileContent(oldView?.Content, newView.Content);
        }
    }
}
