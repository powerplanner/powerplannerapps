using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.App;
using BareMvvm.Core.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerAndroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAndroid.Extensions;
using ToolsPortable;

namespace PowerPlannerAndroid.App
{
    public class PowerPlannerDroidApp : PowerPlannerApp
    {
        static PowerPlannerDroidApp()
        {
            DoNotShowYearsInTabItems = true;
            DoNotShowSettingsInTabItems = true;
            ShowClassesAsPopups = true;
        }

        public static new PowerPlannerDroidApp Current
        {
            get { return PortableApp.Current as PowerPlannerDroidApp; }
        }

        protected override Task InitializeAsyncOverride()
        {
            PortableLocalizedResources.CultureExtension = delegate { return System.Globalization.CultureInfo.CurrentCulture; };
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetAppName = delegate { return "Power Planner for Android"; };

            // Beware that ViewModel-related things are dependent on this value being exactly Android, do not change.
            PowerPlannerAppDataLibrary.SyncLayer.SyncExtensions.GetPlatform = delegate { return "Android"; };

            DateTimeFormatterExtension.Current = new DroidDateTimeFormatterExtension();
            InAppPurchaseExtension.Current = new AndroidInAppPurchaseExtension();
            RemindersExtension.Current = new AndroidRemindersExtension();
            TilesExtension.Current = new DroidTilesExtension();
            PushExtension.Current = new DroidPushExtension();
            ImagePickerExtension.Current = new DroidImagePickerExtension();
            NetworkInfoExtension.Current = new DroidNetworkInfoExtension();
            AppShortcutsExtension.Current = new DroidAppShortcutsExtension();

            return base.InitializeAsyncOverride();
        }
    }
}