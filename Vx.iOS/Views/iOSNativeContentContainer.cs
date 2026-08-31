using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    internal class iOSNativeContentContainer : iOSView<NativeContentContainer, UIContentView>
    {
        protected override void ApplyProperties(NativeContentContainer oldView, NativeContentContainer newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!ReferenceEquals(oldView?.NativeContent, newView.NativeContent))
            {
                View.Content = newView.NativeContent is UIView nativeContent
                    ? new UIViewWrapper(nativeContent)
                    : null;
            }
        }
    }
}