using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.App;
using BareMvvm.Core.App;
using System.Threading.Tasks;

namespace PowerPlanneriOS.App
{
    public class PowerPlanneriOSApp : PowerPlannerApp
    {
        static PowerPlanneriOSApp()
        {
            UseUnifiedCalendarDayTabItem = true;
            DoNotShowYearsInTabItems = true;
        }

        public static new PowerPlanneriOSApp Current
        {
            get { return PortableApp.Current as PowerPlanneriOSApp; }
        }

        protected override Task InitializeAsyncOverride()
        {
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Power Planner for iOS"; };

            // Note that there's several places my code takes a dependency on this to change behavior for iOS version
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetPlatform = delegate { return "iOS"; };

            //InAppPurchaseExtension.Current = new AndroidInAppPurchaseExtension();
            //RemindersExtension.Current = new AndroidRemindersExtension();
            //TilesExtension.Current = new DroidTilesExtension();

            return base.InitializeAsyncOverride();
        }
    }
}