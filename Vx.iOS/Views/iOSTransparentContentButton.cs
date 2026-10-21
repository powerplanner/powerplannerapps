using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTransparentContentButton : iOSView<TransparentContentButton, UIContentView>
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

            View.BackgroundColor = newView.BackgroundColor.ToUI();
            View.Layer.CornerRadius = newView.CornerRadius;
            View.Layer.MasksToBounds = newView.CornerRadius > 0; // Clip the corners of subviews

            ReconcileContentNew(oldView?.Content, newView.Content);
        }
    }
}