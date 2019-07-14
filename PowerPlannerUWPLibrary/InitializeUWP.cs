using InterfacesUWP;
using InterfacesUWP.App;
using Microsoft.HockeyApp;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWPLibrary.App;
using PowerPlannerUWPLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace PowerPlannerUWPLibrary
{
    public static class InitializeUWP
    {
        private const string HOCKEY_APP_ID = Secrets.HockeyAppId;

        private static bool _initialized;
        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            Variables.VERSION = new Version(Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

            try
            {
                HockeyClient.Current.Configure(HOCKEY_APP_ID);
            }

            catch { }

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
            PowerPlannerAppDataLibrary.Extensions.AppointmentsExtension.Current = new UWPAppointmentsExtension();
            PowerPlannerAppDataLibrary.Extensions.NetworkInfoExtension.Current = new UWPNetworkInfoExtension();
            PowerPlannerAppDataLibrary.Extensions.PushExtension.Current = new UWPPushExtension();
            PowerPlannerAppDataLibrary.Extensions.RemindersExtension.Current = new UWPRemindersExtension();
            NetworkInfoExtension.Current = new UWPNetworkInfoExtension();
            //PowerPlannerAppDataLibrary.Extensions.ScheduleTileExtension.Current = new UWPSch
            //PowerPlannerAppDataLibrary.Extensions.TelemetryExtension.Current = new 
            PowerPlannerAppDataLibrary.Extensions.TilesExtension.Current = new UWPTilesExtension();
            DateTimeFormatterExtension.Current = new UWPDateTimeFormatterExtension();
            ImagePickerExtension.Current = new UWPImagePickerExtension();
        }
    }
}
