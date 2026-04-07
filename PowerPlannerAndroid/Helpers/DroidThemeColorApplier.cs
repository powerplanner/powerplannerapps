using System;
using System.Drawing;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAndroid.Helpers
{
    public static class DroidThemeColorApplier
    {
        /// <summary>
        /// Fires when theme colors have been updated. Views subscribe to update their appearance.
        /// </summary>
        public static event Action<ThemeColors> ThemeChanged;

        /// <summary>
        /// The currently applied theme colors, initialized to defaults.
        /// </summary>
        public static ThemeColors Current { get; private set; } =
            ThemeColorGenerator.Generate(ThemeColorGenerator.DefaultPrimary);

        public static void Apply(ThemeColors colors)
        {
            try
            {
                Current = colors;
                ThemeChanged?.Invoke(colors);

                // Update widgets so their headers reflect the new theme color
                WidgetsHelper.UpdateAllWidgets();
            }
            catch (Exception ex)
            {
                PowerPlannerAppDataLibrary.Extensions.TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Returns the current theme's PrimaryDark color as an Android ARGB int,
        /// suitable for use in widget RemoteViews. Reads from the cached settings
        /// so it works even when the app hasn't fully initialized.
        /// </summary>
        public static int GetWidgetHeaderColor()
        {
            try
            {
                var cached = Settings.CachedPrimaryThemeColor;
                if (cached != null)
                {
                    var primary = ColorTranslator.FromHtml(cached);
                    var colors = ThemeColorGenerator.Generate(primary);
                    return new Android.Graphics.Color(colors.PrimaryDark.R, colors.PrimaryDark.G, colors.PrimaryDark.B).ToArgb();
                }
            }
            catch
            {
                // Invalid cached value, fall through to default
            }

            var defaultColors = ThemeColorGenerator.Generate(ThemeColorGenerator.DefaultPrimary);
            return new Android.Graphics.Color(defaultColors.PrimaryDark.R, defaultColors.PrimaryDark.G, defaultColors.PrimaryDark.B).ToArgb();
        }

        /// <summary>
        /// Converts a System.Drawing.Color to Android.Graphics.Color.
        /// </summary>
        public static Android.Graphics.Color ToDroid(Color color)
        {
            return new Android.Graphics.Color(color.R, color.G, color.B);
        }
    }
}
