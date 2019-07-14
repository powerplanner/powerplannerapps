using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlanneriOS.Helpers;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Views
{
    /// <summary>
    /// Automatically sets its height to the status bar height. No need to set this frame's height, but you should set its width to stretch. Note that UIVisualEffectView was added in 8.0, but that's the min version Power Planner supports so we're good.
    /// </summary>
    public class UIStatusBarView : UIView
    {
        public UIStatusBarView()
        {
            base.BackgroundColor = ColorResources.PowerPlannerBlueChromeColor;
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
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                    statusBar.BottomAnchor.ConstraintEqualTo(viewToAddTo.SafeAreaLayoutGuide.TopAnchor)
                });
            }
            else
            {
                statusBar.ConfigureHeight();
            }

            return statusBar;
        }

        public override void LayoutSubviews()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                ConfigureHeight();
            }

            base.LayoutSubviews();
        }

        private void ConfigureHeight()
        {
            switch (UIDevice.CurrentDevice.Orientation)
            {
                case UIDeviceOrientation.LandscapeLeft:
                case UIDeviceOrientation.LandscapeRight:
                    SetHeightIfNeeded(0);
                    break;

                default:
                    SetHeightIfNeeded(20);
                    break;
            }
        }

        private float _currHeight = -1;
        private void SetHeightIfNeeded(float height)
        {
            if (_currHeight == height)
            {
                return;
            }

            _currHeight = height;
            this.SetHeight(height);
        }
    }
}