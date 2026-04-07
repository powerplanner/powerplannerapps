using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.App
{
    public static class SharedInitialization
    {
        private static bool _hasBeenCalled;
        /// <summary>
        /// Initialization needed for the shared data model to work. Automatically called if running as PowerPlannerApp, but background task projects need to call this themselves when not using the full app.
        /// </summary>
        public static void Initialize()
        {
            if (_hasBeenCalled)
            {
                return;
            }
            _hasBeenCalled = true;

            // Set up API key
            SetUpApiKey();
            PowerPlannerAuth.WebsiteApiUrl = new Uri(Website.ClientApiUrl);

            // Pipe localizer so it works on Android from portable binding library
            PortableLocalizedResources.LocalizerExtension = PowerPlannerResources.GetString;

            Color primary = ThemeColorGenerator.DefaultPrimary;
            var cached = Settings.CachedPrimaryThemeColor;
            if (cached != null)
            {
                try
                {
                    primary = ColorTranslator.FromHtml(cached);
                }
                catch
                {
                    // Invalid cached value, fall back to default
                }
            }
            var colors = ThemeColorGenerator.Generate(primary);
            Theme.DefaultAccentColor = colors.Accent;
            Theme.DefaultDarkAccentColor = colors.DarkAccent;
            Theme.Current.ChromeColor = colors.Primary;
        }

        private static void SetUpApiKey()
        {
            Website.ApiKey = new ApiKeyCombo(Secrets.PowerPlannerApiKey, Secrets.PowerPlannerApiHashedKey);
            PowerPlannerAuth.SetApiKey(Website.ApiKey.ApiKey, Website.ApiKey.HashedKey);
        }
    }
}
