using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlanneriOS.Helpers;
using InterfacesiOS.Views;
using Vx.Views;

namespace PowerPlanneriOS.Views
{
    /// <summary>
    /// Automatically sets its height to the status bar height using SafeAreaLayoutGuide.
    /// </summary>
    public class UIStatusBarView : UIView
    {
        public UIStatusBarView()
        {
            // Extend the chrome color to the status bar area.
            base.BackgroundColor = iOSThemeColorApplier.ToUIColor(Theme.Current.ChromeColor);
        }

        public static UIStatusBarView CreateAndAddTo(UIView viewToAddTo)
        {
            var statusBar = new UIStatusBarView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            viewToAddTo.Add(statusBar);
            statusBar.StretchWidth(viewToAddTo);
            statusBar.PinToTop(viewToAddTo);
            // https://stackoverflow.com/questions/46344381/ios-11-layout-guidance-about-safe-area-for-iphone-x
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                statusBar.BottomAnchor.ConstraintEqualTo(viewToAddTo.SafeAreaLayoutGuide.TopAnchor)
            });

            return statusBar;
        }
    }
}