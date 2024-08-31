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
using PowerPlannerAndroid.ViewModel.Settings;
using BareMvvm.Core.ViewModels;
using Vx.Extensions;
using Vx.Droid;
using PowerPlannerAppDataLibrary.Views;

namespace PowerPlannerAndroid.App
{
    public class PowerPlannerDroidApp : PowerPlannerApp
    {
        static PowerPlannerDroidApp()
        {
            UseUnifiedCalendarDayTabItem = true;
            DoNotShowYearsInTabItems = true;
            ShowClassesAsPopups = true;
            ShowSettingsPagesAsPopups = true;
            UsesIanaTimeZoneIds = true;

            SettingsListViewModel.CustomOpenGoogleCalendarIntegration = settingsListViewModel =>
            {
                var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(GoogleCalendarIntegrationViewModel.Url));

                // Pass in headers
                // https://stackoverflow.com/questions/28868734/open-browser-with-a-url-with-extra-headers-for-android
                var headersBundle = new Bundle();
                headersBundle.PutString("AccountId", settingsListViewModel.Account.AccountId.ToString());
                headersBundle.PutString("Username", settingsListViewModel.Account.Username);
                headersBundle.PutString("Session", settingsListViewModel.Account.Token);

                browserIntent.PutExtra(Android.Provider.Browser.ExtraHeaders, headersBundle);

                Vx.Droid.VxDroidExtensions.ApplicationContext.StartActivity(browserIntent);
            };
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
            ClassRemindersExtension.Current = new DroidClassRemindersExtension();
            RemindersExtension.Current = new AndroidRemindersExtension();
            TilesExtension.Current = new DroidTilesExtension();
            PushExtension.Current = new DroidPushExtension();
            ImagePickerExtension.Current = new DroidImagePickerExtension();
            NetworkInfoExtension.Current = new DroidNetworkInfoExtension();
            AppShortcutsExtension.Current = new DroidAppShortcutsExtension();
            BrowserExtension.Current = new DroidBrowserExtension();
            EmailExtension.Current = new DroidEmailExtension();
            ThemeExtension.Current = new DroidThemeExtension();

            // Initialize custom views
            VxDroidExtensions.RegisterCustomView(v => v is CompletionSlider, v => new DroidCompletionSlider());
            VxDroidExtensions.RegisterCustomView(v => v is FloatingAddItemButton, v => new DroidFloatingAddItemButton());

            return base.InitializeAsyncOverride();
        }
    }
}