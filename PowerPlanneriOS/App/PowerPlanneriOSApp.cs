using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Helpers;
using BareMvvm.Core.App;
using System.Threading.Tasks;
using InterfacesiOS.Views;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.App
{
    public class PowerPlanneriOSApp : PowerPlannerApp
    {
        static PowerPlanneriOSApp()
        {
            UseUnifiedCalendarDayTabItem = true;
            UsesIanaTimeZoneIds = true;
            ShowSettingsPagesAsPopups = true;
        }

        public static new PowerPlanneriOSApp Current
        {
            get { return PortableApp.Current as PowerPlanneriOSApp; }
        }

        protected override Task InitializeAsyncOverride()
        {
            ThemeColorApplier.PlatformThemeApplier = iOSThemeColorApplier.Apply;

#if __MACCATALYST__
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Power Planner for Mac"; };
#else
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Power Planner for iOS"; };
#endif

            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetPlatform = delegate
            {
#if __MACCATALYST__
                return "Mac";
#else
                return "iOS";
#endif
            };

            return base.InitializeAsyncOverride();
        }
    }
}