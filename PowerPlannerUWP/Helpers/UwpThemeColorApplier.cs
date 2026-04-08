using PowerPlannerAppDataLibrary.Helpers;
using System;
using Vx.Views;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PowerPlannerUWP.Helpers
{
    public static class UwpThemeColorApplier
    {
        public static void Apply(ThemeColors colors)
        {
            try
            {
                UpdateBrushResources(colors);
                UpdateColorResources(colors);
                UpdateTitleBar(colors);
            }
            catch (Exception ex)
            {
                PowerPlannerAppDataLibrary.Extensions.TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void UpdateBrushResources(ThemeColors colors)
        {
            var resources = Application.Current.Resources;

            SetBrushColor(resources, "PowerPlannerBlue", colors.Primary);
            SetBrushColor(resources, "PowerPlannerSecondaryBlue", colors.PrimaryLight);
            SetBrushColor(resources, "PowerPlannerBlueSelected", colors.PrimaryLight);
            SetBrushColor(resources, "PowerPlannerBlueHover", colors.PrimaryHover);
            SetBrushColor(resources, "PowerPlannerDarkerBlue", colors.PrimaryDark);
            SetBrushColor(resources, "PowerPlannerCommandBarBackground", colors.PrimaryDark);
            SetBrushColor(resources, "TaskOrEventBrush", colors.PrimaryLight);
        }

        private static void UpdateColorResources(ThemeColors colors)
        {
            var resources = Application.Current.Resources;

            // Generate a further-darkened variant for SystemAccentColorDark3
            var darkest = ThemeColorGenerator.Generate(colors.PrimaryDark).PrimaryDark;

            resources["PowerPlannerBlueColor"] = ToUwpColor(colors.Primary);

            resources["SystemAccentColor"] = ToUwpColor(colors.Primary);
            resources["SystemAccentColorDark1"] = ToUwpColor(colors.Primary);
            resources["SystemAccentColorDark2"] = ToUwpColor(colors.PrimaryDark);
            resources["SystemAccentColorDark3"] = ToUwpColor(darkest);

            resources["SystemAccentColorLight1"] = ToUwpColor(colors.PrimaryLight);
            resources["SystemAccentColorLight2"] = ToUwpColor(colors.PrimaryLight);
            resources["SystemAccentColorLight3"] = ToUwpColor(colors.DarkAccent);
        }

        /// <summary>
        /// Updates the title bar using the current Theme.Current.ChromeColor.
        /// Called during initial window setup before account is loaded, so it uses
        /// whatever color SharedInitialization set (cached or default).
        /// </summary>
        public static void UpdateTitleBarFromTheme()
        {
            try
            {
                var colors = ThemeColorGenerator.Generate(Theme.Current.ChromeColor);
                UpdateTitleBar(colors);
            }
            catch (Exception ex)
            {
                PowerPlannerAppDataLibrary.Extensions.TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void UpdateTitleBar(ThemeColors colors)
        {
            var view = ApplicationView.GetForCurrentView();
            var titleBar = view.TitleBar;

            var darkColor = ToUwpColor(colors.PrimaryDarkest);
            var hoverColor = ToUwpColor(colors.PrimaryHover);
            var lightColor = ToUwpColor(colors.PrimaryLight);

            // Generate an inactive color (lighter variant of primary)
            var inactiveColor = ToUwpColor(colors.PrimaryInactive);

            titleBar.BackgroundColor = darkColor;
            titleBar.ForegroundColor = Colors.White;

            titleBar.InactiveBackgroundColor = inactiveColor;
            titleBar.InactiveForegroundColor = Colors.LightGray;

            titleBar.ButtonBackgroundColor = darkColor;
            titleBar.ButtonForegroundColor = Colors.White;

            titleBar.ButtonHoverBackgroundColor = hoverColor;
            titleBar.ButtonHoverForegroundColor = Colors.White;

            titleBar.ButtonInactiveBackgroundColor = inactiveColor;
            titleBar.ButtonInactiveForegroundColor = Colors.LightGray;

            titleBar.ButtonPressedBackgroundColor = lightColor;
            titleBar.ButtonPressedForegroundColor = Colors.White;
        }

        private static void SetBrushColor(ResourceDictionary resources, string key, System.Drawing.Color color)
        {
            if (resources.ContainsKey(key) && resources[key] is SolidColorBrush brush)
            {
                brush.Color = ToUwpColor(color);
            }
        }

        private static Color ToUwpColor(System.Drawing.Color color)
        {
            return Color.FromArgb(255, color.R, color.G, color.B);
        }
    }
}
