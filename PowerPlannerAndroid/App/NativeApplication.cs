using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.App;
using InterfacesDroid.App;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAndroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAndroid.ViewModel.Settings;
using PowerPlannerAppDataLibrary.ViewModels;
using AndroidX.Core.Content.PM;
using Android.Content.PM;
using AndroidX.AppCompat.App;
using Microsoft.ApplicationInsights.Channel;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.App
{
    [Application(
#if DEBUG
        Debuggable = true
#else
        Debuggable = false
#endif
        )]
    public class NativeApplication : NativeDroidApplication
    {
        protected NativeApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            PackageInfo packageInfo;
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                packageInfo = Context.PackageManager.GetPackageInfo(Context.PackageName, PackageManager.PackageInfoFlags.Of(PackageInfoFlagsLong.None));
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                packageInfo = Context.PackageManager.GetPackageInfo(Context.PackageName, 0);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            var versionName = packageInfo.VersionName;
            Variables.VERSION = Version.Parse(versionName);

            Extensions.DroidThemeExtension.ApplyTheme(ignoreAutomatic: true);

#if DEBUG
            //var culture = new System.Globalization.CultureInfo("es-MX");
            //System.Globalization.CultureInfo.CurrentCulture = culture;
            //System.Globalization.CultureInfo.CurrentUICulture = culture;
            //System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            //System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
#endif
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override Dictionary<Type, Type> GetGenericViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(PopupComponentViewModel), typeof(PopupComponentView) },
                { typeof(ComponentViewModel), typeof(ComponentView) } // This must be after Popup since Popup is a subclass
            };
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                // Welcome views
                { typeof(WelcomeViewModel), typeof(WelcomeView) },

                // Main views
                { typeof(InitialSyncViewModel), typeof(InitialSyncView) },
                { typeof(MainScreenViewModel), typeof(MainScreenView) },
                { typeof(ScheduleViewModel), typeof(ScheduleView) },
                { typeof(CalendarViewModel), typeof(ComponentView) },
                { typeof(AgendaViewModel), typeof(ComponentView) },
                { typeof(ClassViewModel), typeof(ClassView) },
                { typeof(ClassesViewModel), typeof(ClassesView) },
                { typeof(EditClassDetailsViewModel), typeof(EditClassDetailsView) },
                { typeof(PremiumVersionViewModel), typeof(PremiumVersionView) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsView) },
                { typeof(QuickAddViewModel), typeof(QuickAddView) },
                { typeof(SyncOptionsSimpleViewModel), typeof(SettingsSyncOptionsView) },
                { typeof(ImageUploadOptionsViewModel), typeof(SettingsImageUploadOptionsView) }
            };
        }

        public override Dictionary<Type, Type> GetViewModelToSplashMappings()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(MainWindowViewModel), typeof(SplashScreenView) }
            };
        }

        public override Type GetPortableAppType()
        {
            return typeof(PowerPlannerDroidApp);
        }

        /// <summary>
        /// Only called in debugging instances when app is terminated with debugger attached
        /// </summary>
        public override void OnTerminate()
        {
            base.OnTerminate();

            TelemetryExtension.Current?.SuspendingApp();
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            base.OnTrimMemory(level);

            // Flush or send telemetry data
            if (level == TrimMemory.UiHidden || level == TrimMemory.Complete)
            {
                TelemetryExtension.Current?.SuspendingApp();
            }
        }
    }
}