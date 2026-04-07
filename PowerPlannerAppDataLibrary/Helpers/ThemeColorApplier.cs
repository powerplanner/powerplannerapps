using System;
using System.Drawing;
using PowerPlannerAppDataLibrary.DataLayer;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class ThemeColorApplier
    {
        /// <summary>
        /// Platform-specific callback to apply theme colors (e.g., UWP brushes, Android status bar).
        /// Set by each platform's initialization code.
        /// </summary>
        public static Action<ThemeColors> PlatformThemeApplier { get; set; }

        /// <summary>
        /// Applies the theme color from the given account (or defaults if null/no color set).
        /// Updates Theme.Current properties and caches the color for instant next startup.
        /// </summary>
        public static void Apply(AccountDataItem account)
        {
            var primary = account?.PrimaryThemeColor?.ToColor() ?? ThemeColorGenerator.DefaultPrimary;

            // If ToColor returned default (empty), use the default primary
            if (primary.A == 0 && primary.R == 0 && primary.G == 0 && primary.B == 0)
            {
                primary = ThemeColorGenerator.DefaultPrimary;
            }

            var colors = ThemeColorGenerator.Generate(primary);

            Theme.Current.ChromeColor = colors.Primary;
            Theme.DefaultAccentColor = colors.Accent;
            Theme.DefaultDarkAccentColor = colors.DarkAccent;

            // Cache for next startup
            if (account?.PrimaryThemeColor != null && account.PrimaryThemeColor.Length >= 3)
            {
                Settings.CachedPrimaryThemeColor = $"#{primary.R:X2}{primary.G:X2}{primary.B:X2}";
            }
            else
            {
                Settings.CachedPrimaryThemeColor = null;
            }

            // Notify platform-specific code
            PlatformThemeApplier?.Invoke(colors);
        }
    }
}
