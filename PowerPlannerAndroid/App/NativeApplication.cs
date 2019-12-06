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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAndroid.ViewModel.Settings;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlannerAndroid.Views.SettingsViews.Grades;
using PowerPlannerAndroid.Views.WelcomeViews;
using PowerPlannerAndroid.Views.SettingsViews;

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

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                // Welcome views
                { typeof(WelcomeViewModel), typeof(WelcomeView) },
                { typeof(LoginViewModel), typeof(LoginView) },
                { typeof(ConnectAccountViewModel), typeof(ConnectAccountView) },
                { typeof(ExistingUserViewModel), typeof(ExistingUserView) },

                // Main views
                { typeof(InitialSyncViewModel), typeof(InitialSyncView) },
                { typeof(MainScreenViewModel), typeof(MainScreenView) },
                { typeof(YearsViewModel), typeof(YearsView) },
                { typeof(CreateAccountViewModel), typeof(CreateAccountView) },
                { typeof(AddYearViewModel), typeof(AddYearView) },
                { typeof(AddSemesterViewModel), typeof(AddSemesterView) },
                { typeof(AddClassViewModel), typeof(AddClassView) },
                { typeof(ScheduleViewModel), typeof(ScheduleView) },
                { typeof(CalendarViewModel), typeof(CalendarMainView) },
                { typeof(AddClassTimeViewModel), typeof(AddClassTimeView) },
                { typeof(AgendaViewModel), typeof(AgendaView) },
                { typeof(AddHomeworkViewModel), typeof(AddHomeworkView) },
                { typeof(ViewHomeworkViewModel), typeof(ViewHomeworkView) },
                { typeof(DayViewModel), typeof(DayView) },
                { typeof(ClassViewModel), typeof(ClassView) },
                { typeof(ClassesViewModel), typeof(ClassesView) },
                { typeof(EditClassDetailsViewModel), typeof(EditClassDetailsView) },
                { typeof(MyAccountViewModel), typeof(MyAccountView) },
                { typeof(SettingsListViewModel), typeof(SettingsListView) },
                { typeof(AboutViewModel), typeof(AboutView) },
                { typeof(ConfirmIdentityViewModel), typeof(ConfirmIdentityView) },
                { typeof(ChangeEmailViewModel), typeof(ChangeEmailView) },
                { typeof(ChangeUsernameViewModel), typeof(ChangeUsernameView) },
                { typeof(ChangePasswordViewModel), typeof(ChangePasswordView) },
                { typeof(UpdateCredentialsViewModel), typeof(UpdateCredentialsView) },
                { typeof(ForgotUsernameViewModel), typeof(ForgotUsernameView) },
                { typeof(RecoveredUsernamesViewModel), typeof(RecoveredUsernamesView) },
                { typeof(ResetPasswordViewModel), typeof(ResetPasswordView) },
                { typeof(PremiumVersionViewModel), typeof(PremiumVersionView) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsView) },
                { typeof(ViewGradeViewModel), typeof(ViewGradeView) },
                { typeof(AddGradeViewModel), typeof(AddGradeView) },
                { typeof(ClassWhatIfViewModel), typeof(ClassWhatIfView) },
                { typeof(AddHolidayViewModel), typeof(AddHolidayView) },
                { typeof(TwoWeekScheduleSettingsViewModel), typeof(SettingsTwoWeekScheduleView) },
                { typeof(QuickAddViewModel), typeof(QuickAddView) },
                { typeof(WidgetsViewModel), typeof(SettingsWidgetsView) },
                { typeof(WidgetAgendaViewModel), typeof(SettingsWidgetAgendaView) },
                { typeof(WidgetScheduleViewModel), typeof(SettingsWidgetScheduleView) },
                { typeof(SyncOptionsSimpleViewModel), typeof(SettingsSyncOptionsView) },
                { typeof(ImageUploadOptionsViewModel), typeof(SettingsImageUploadOptionsView) },

                { typeof(ConfigureClassGradesListViewModel), typeof(ConfigureClassGradesListView) },
                { typeof(ConfigureClassAverageGradesViewModel), typeof(ConfigureClassAverageGradesView) },
                { typeof(ConfigureClassCreditsViewModel), typeof(ConfigureClassCreditsView) },
                { typeof(ConfigureClassGpaTypeViewModel), typeof(ConfigureClassGpaTypeView) },
                { typeof(ConfigureClassGradeScaleViewModel), typeof(ConfigureClassGradeScaleView) },
                { typeof(ConfigureClassPassingGradeViewModel), typeof(ConfigureClassPassingGradeView) },
                { typeof(ConfigureClassRoundGradesUpViewModel), typeof(ConfigureClassRoundGradesUpView) },
                { typeof(ConfigureClassWeightCategoriesViewModel), typeof(ConfigureClassWeightCategoriesView) },

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