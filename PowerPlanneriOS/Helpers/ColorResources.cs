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
        /// <summary>
        /// Dynamic chrome color that reads from the current theme colors.
        /// </summary>
        public static UIColor PowerPlannerBlueChromeColor =>
            iOSThemeColorApplier.ToUIColor(iOSThemeColorApplier.Current.Primary);

        /// <summary>
        /// Dynamic accent color that reads from the current theme colors.
        /// </summary>
        public static UIColor PowerPlannerAccentBlue =>
            iOSThemeColorApplier.ToUIColor(iOSThemeColorApplier.Current.Accent);

        public static readonly UIColor InputSectionDividers = BareUIHelper.InputSectionDividerColor;
        public static readonly UIColor InputDividers = BareUIHelper.InputDividerColor;

        public static void ConfigureNavBar(UINavigationBar navBar)
        {
            navBar.BarTintColor = PowerPlannerBlueChromeColor;
            navBar.TintColor = UIColor.White;
            navBar.Translucent = false;

            navBar.TitleTextAttributes = new UIStringAttributes()
            {
                ForegroundColor = UIColor.White
            };

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

        public static void ConfigureTabBar(UITabBar tabBar)
        {
            var primaryColor = iOSThemeColorApplier.ToUIColor(iOSThemeColorApplier.Current.Primary);

            tabBar.TintColor = UIColor.White;
            tabBar.UnselectedItemTintColor = UIColor.White.ColorWithAlpha(0.7f);
            tabBar.BarTintColor = primaryColor;
            tabBar.Translucent = false;

            var itemAppearance = new UITabBarItemAppearance();
            itemAppearance.Normal.IconColor = UIColor.White.ColorWithAlpha(0.7f);
            itemAppearance.Normal.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = UIColor.White.ColorWithAlpha(0.7f) };
            itemAppearance.Selected.IconColor = UIColor.White;
            itemAppearance.Selected.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = UIColor.White };

            var appearance = new UITabBarAppearance();
            appearance.ConfigureWithOpaqueBackground();
            appearance.BackgroundColor = primaryColor;
            appearance.StackedLayoutAppearance = itemAppearance;
            appearance.InlineLayoutAppearance = itemAppearance;
            appearance.CompactInlineLayoutAppearance = itemAppearance;

            tabBar.StandardAppearance = appearance;
            
            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                tabBar.ScrollEdgeAppearance = appearance;
            }
        }
    }
}