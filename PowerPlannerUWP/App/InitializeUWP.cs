using InterfacesUWP;
using InterfacesUWP.App;
using Microsoft.BingAds.UETSdk;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Views;
using PowerPlannerUWP.Extensions;
using PowerPlannerUWP.Views;
using System;
using ToolsPortable;
using Vx.Extensions;
using Vx.Uwp;
using Windows.ApplicationModel;

namespace PowerPlannerUWP
{
    public static class InitializeUWP
    {
        private static bool _initialized;
        private static UETSdk _uetSdk;

        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            Variables.VERSION = new Version(Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Power Planner for Windows 10"; };
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetPlatform = delegate
            {
                if (DeviceInfo.DeviceFamily == DeviceFamily.Mobile)
                {
                    return "Windows 10 Mobile";
                }
                else
                {
                    return "Windows 10";
                }
            };

            // Have to initialize dispatcher so that some sort of dispatcher is registered for background task
            PortableDispatcher.ObtainDispatcherFunction = () => { return new UwpDispatcher(); };
            LocalizationExtension.Current = new UWPLocalizationExtension();
            TelemetryExtension.Current = new UWPTelemetryExtension();
            InAppPurchaseExtension.Current = new UWPInAppPurchaseExtension();
            AppointmentsExtension.Current = new UWPAppointmentsExtension();
            NetworkInfoExtension.Current = new UWPNetworkInfoExtension();
            PushExtension.Current = new UWPPushExtension();
            ClassRemindersExtension.Current = new UWPClassRemindersExtension();
            RemindersExtension.Current = new UWPRemindersExtension();
            NetworkInfoExtension.Current = new UWPNetworkInfoExtension();
            //ScheduleTileExtension.Current = new UWPSch
            //TelemetryExtension.Current = new 
            TilesExtension.Current = new UWPTilesExtension();
            DateTimeFormatterExtension.Current = new UWPDateTimeFormatterExtension();
            ImagePickerExtension.Current = new UWPImagePickerExtension();
            LanguageExtension.Current = new UWPLanguageExtension();
            SoundsExtension.Current = new UWPSoundsExtension();
            BrowserExtension.Current = new UWPBrowserExtension();
            EmailExtension.Current = new UWPEmailExtension();
            ThemeExtension.Current = new UWPThemeExtension();

            // Register custom Vx views
            VxUwpExtensions.RegisterCustomView(v => v is PowerPlannerAppDataLibrary.Views.CompletionSlider, v => new UwpCompletionSlider());

            // Initialize Microsoft Ads SDK
            try
            {
                _uetSdk = new UETSdk(97151577);
            }
            catch { }
        }
    }
}
