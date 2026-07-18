using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public override Dictionary<Type, Func<ViewGroup, View>> GetGenericViewModelToViewMappings()
        {
            return new Dictionary<Type, Func<ViewGroup, View>>()
            {
                { typeof(PopupComponentViewModel), root => new PopupComponentView(root) },
                { typeof(ComponentViewModel), root => new ComponentView(root) } // This must be after Popup since Popup is a subclass
            };
        }

        public override Dictionary<Type, Func<ViewGroup, View>> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Func<ViewGroup, View>>()
            {
                // Welcome views
                { typeof(WelcomeViewModel), root => new WelcomeView(root) },

                // Main views
                { typeof(InitialSyncViewModel), root => new InitialSyncView(root) },
                { typeof(MainScreenViewModel), root => new MainScreenView(root) },
                { typeof(ScheduleViewModel), root => new ScheduleView(root) },
                { typeof(CalendarViewModel), root => new ComponentView(root) },
                { typeof(AgendaViewModel), root => new ComponentView(root) },
                { typeof(ClassViewModel), root => new ClassView(root) },
                { typeof(EditClassDetailsViewModel), root => new EditClassDetailsView(root) },
                { typeof(PremiumVersionViewModel), root => new PremiumVersionView(root) },
                { typeof(SyncErrorsViewModel), root => new SyncErrorsView(root) },
                { typeof(QuickAddViewModel), root => new QuickAddView(root) }
            };
        }

        public override Dictionary<Type, Func<ViewGroup, View>> GetViewModelToSplashMappings()
        {
            return new Dictionary<Type, Func<ViewGroup, View>>()
            {
                { typeof(MainWindowViewModel), root => new SplashScreenView(root.Context) }
            };
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
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