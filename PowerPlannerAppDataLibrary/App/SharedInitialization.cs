using PowerPlannerAppAuthLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

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
            PowerPlannerAuth.WebsiteApiUrl = new Uri(Website.URL);

            // Pipe localizer so it works on Android from portable binding library
            PortableLocalizedResources.LocalizerExtension = PowerPlannerResources.GetString;
        }

        private static void SetUpApiKey()
        {
            Website.ApiKey = new ApiKeyCombo(Secrets.PowerPlannerApiKey, Secrets.PowerPlannerApiHashedKey);
            PowerPlannerAuth.SetApiKey(Website.ApiKey.ApiKey, Website.ApiKey.HashedKey);
        }
    }
}
