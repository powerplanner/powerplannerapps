using InterfacesUWP;
using InterfacesUWP.App;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.Extensions;
using System;
using ToolsPortable;
using Windows.ApplicationModel;

namespace PowerPlannerUWP
{
    public static class InitializeUWP
    {
        private const string APP_CENTER_APP_SECRET = Secrets.AppCenterAppSecret;

        private static bool _initialized;
        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            Variables.VERSION = new Version(Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

#if !DEBUG
            try
            {
                AppCenter.Start(APP_CENTER_APP_SECRET, typeof(Crashes), typeof(Analytics));
            }

            catch { }
#endif

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
        }
    }
}
