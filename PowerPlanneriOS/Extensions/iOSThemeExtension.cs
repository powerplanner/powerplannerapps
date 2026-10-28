using System;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlanneriOS.Helpers;
using UIKit;
using Vx.iOS;

namespace PowerPlanneriOS.Extensions
{
    internal class iOSThemeExtension : ThemeExtension
    {
        public override void ApplyThemeAndRelaunchIfNeeded()
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            ApplyTheme(window);
        }

        internal static void ApplyTheme(UIWindow window)
        {
            switch (PowerPlannerAppDataLibrary.Helpers.Settings.ThemeOverride)
            {
                case Themes.Dark:
                    window.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Dark;
                    break;

                case Themes.Light:
                    window.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
                    break;

                case Themes.Custom:
                    if (PowerPlannerAppDataLibrary.Helpers.Settings.CustomBackgroundColor != null)
                    {
                        if (PowerPlannerAppDataLibrary.Helpers.Settings.CustomBackgroundColor.Value.GetBrightness() < 0.5f)
                        {
                            window.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Dark;
                        }
                        else
                        {
                            window.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
                        }
                    }
                    break;

                case Themes.Automatic:
                    window.OverrideUserInterfaceStyle = UIUserInterfaceStyle.Unspecified;
                    break;
            }

            window.BackgroundColor = Vx.Views.Theme.Current.BackgroundColor.ToUI();
            window.TintColor = Vx.Views.Theme.Current.AccentColor.ToUI();
        }
    }
}