using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Views;
using UIKit;

namespace PowerPlanneriOS.Helpers
{
    public static class ColorResources
    {
        // Originally UIColor.FromRGB(63 / 255f, 80 / 255f, 145 / 255f);
        // But now we're lightining it up since we're disabling the translucent option
        public static readonly UIColor PowerPlannerBlueChromeColor = UIColor.FromRGB(93 / 255f, 107 / 255f, 162 / 255f);
        public static readonly UIColor PowerPlannerAccentBlue = UIColor.FromRGB(84 / 255f, 107 / 255f, 199 / 255f);

        public static readonly UIColor InputSectionDividers = BareUIHelper.InputSectionDividerColor;
        public static readonly UIColor InputDividers = BareUIHelper.InputDividerColor;

        public static void ConfigureNavBar(UINavigationBar navBar)
        {
            navBar.BarTintColor = PowerPlannerBlueChromeColor;
            navBar.TintColor = UIColor.White;
            navBar.Translucent = false;

            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                navBar.TitleTextAttributes = new UIStringAttributes()
                {
                    ForegroundColor = UIColor.White
                };
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var appearance = new UINavigationBarAppearance();
                appearance.ConfigureWithOpaqueBackground();
                appearance.BackgroundColor = PowerPlannerBlueChromeColor;
                appearance.TitleTextAttributes = new UIStringAttributes()
                {
                    ForegroundColor = navBar.TintColor
                };

                navBar.StandardAppearance = appearance;
                navBar.ScrollEdgeAppearance = appearance;
            }
        }
    }
}