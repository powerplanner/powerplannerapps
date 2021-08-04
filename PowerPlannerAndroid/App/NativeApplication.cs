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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
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
using PowerPlannerAndroid.Views.SettingsViews;
using PowerPlannerAppDataLibrary.ViewModels;

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
            var versionName = Context.PackageManager.GetPackageInfo(Context.PackageName, 0).VersionName;
            Variables.VERSION = Version.Parse(versionName);

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
                { typeof(YearsViewModel), typeof(YearsView) },
                { typeof(AddYearViewModel), typeof(AddYearView) },
                { typeof(AddSemesterViewModel), typeof(AddSemesterView) },
                { typeof(AddClassViewModel), typeof(AddClassView) },
                { typeof(ScheduleViewModel), typeof(ScheduleView) },
                { typeof(CalendarViewModel), typeof(CalendarMainView) },
                { typeof(AgendaViewModel), typeof(AgendaView) },
                { typeof(AddTaskOrEventViewModel), typeof(AddTaskOrEventView) },
                { typeof(ViewTaskOrEventViewModel), typeof(ViewTaskOrEventView) },
                { typeof(DayViewModel), typeof(DayView) },
                { typeof(ClassViewModel), typeof(ClassView) },
                { typeof(ClassesViewModel), typeof(ClassesView) },
                { typeof(EditClassDetailsViewModel), typeof(EditClassDetailsView) },
                { typeof(ChangeEmailViewModel), typeof(ChangeEmailView) },
                { typeof(UpdateCredentialsViewModel), typeof(UpdateCredentialsView) },
                { typeof(PremiumVersionViewModel), typeof(PremiumVersionView) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsView) },
                { typeof(ViewGradeViewModel), typeof(ViewGradeView) },
                { typeof(AddGradeViewModel), typeof(AddGradeView) },
                { typeof(ClassWhatIfViewModel), typeof(ClassWhatIfView) },
                { typeof(AddHolidayViewModel), typeof(AddHolidayView) },
                { typeof(QuickAddViewModel), typeof(QuickAddView) },
                { typeof(WidgetsViewModel), typeof(SettingsWidgetsView) },
                { typeof(WidgetAgendaViewModel), typeof(SettingsWidgetAgendaView) },
                { typeof(WidgetScheduleViewModel), typeof(SettingsWidgetScheduleView) },
                { typeof(SyncOptionsSimpleViewModel), typeof(SettingsSyncOptionsView) },
                { typeof(ImageUploadOptionsViewModel), typeof(SettingsImageUploadOptionsView) },

                { typeof(ShowImagesViewModel), typeof(ShowImagesView) },

                // Settings
                { typeof(SuccessfullyCreatedAccountViewModel), typeof(SuccessfullyCreatedAccountView) }
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
    }
}