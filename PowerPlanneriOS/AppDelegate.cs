using System;
using System.Collections.Generic;
using Foundation;
using InterfacesiOS.App;
using UIKit;
using PowerPlanneriOS.App;
using PowerPlannerAppDataLibrary.Windows;
using BareMvvm.Core.App;
using InterfacesiOS.Windows;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlanneriOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlanneriOS.Controllers.Settings;
using PowerPlanneriOS.Welcome;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlanneriOS.Controllers.ClassViewControllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.SyncLayer;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlanneriOS.Extensions;
using PowerPlannerAppDataLibrary.App;
using System.Linq;
using PowerPlanneriOS.Controllers.Welcome;
using PowerPlanneriOS.ViewModels;
using PowerPlanneriOS.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Tasks;
using UserNotifications;
using InterfacesiOS.Helpers;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlanneriOS.Controllers.Settings.Grades;

namespace PowerPlanneriOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : NativeiOSApplication
    {
        private MainAppWindow _mainAppWindow;

        public AppDelegate()
        {
            string versionName = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString") as NSString;
            Variables.VERSION = Version.Parse(versionName);
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                { typeof(WelcomeViewModel), typeof(WelcomeViewController) },
                { typeof(LoginViewModel), typeof(LoginViewController) },
                { typeof(CreateAccountViewModel), typeof(CreateAccountViewController) },
                { typeof(MainScreenViewModel), typeof(MainScreenViewController) },
                { typeof(CalendarViewModel), typeof(CalendarViewController) },
                { typeof(DayViewModel), typeof(DayViewController) },
                { typeof(AgendaViewModel), typeof(AgendaViewController) },
                { typeof(ScheduleViewModel), typeof(ScheduleViewController) },
                { typeof(ClassesViewModel), typeof(ClassesViewController) },
                { typeof(ClassViewModel), typeof(ClassViewController) },
                { typeof(AddClassViewModel), typeof(AddClassViewController) },
                { typeof(AddClassTimeViewModel), typeof(AddClassTimeViewController) },
                { typeof(EditClassDetailsViewModel), typeof(EditClassDetailsViewController) },
                { typeof(YearsViewModel), typeof(YearsViewController) },
                { typeof(AddYearViewModel), typeof(AddYearViewController) },
                { typeof(AddSemesterViewModel), typeof(AddSemesterViewController) },
                { typeof(PremiumVersionViewModel), typeof(PremiumVersionViewController) },
                { typeof(AddHomeworkViewModel), typeof(AddHomeworkViewController) },
                { typeof(ViewHomeworkViewModel), typeof(ViewHomeworkViewController) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsViewController) },
                { typeof(AddHolidayViewModel), typeof(AddHolidayViewController) },
                { typeof(TasksViewModel), typeof(TasksViewController) },
                { typeof(ViewGradeViewModel), typeof(ViewGradeViewController) },
                { typeof(AddGradeViewModel), typeof(AddGradeViewController) },
                { typeof(ConfigureClassCreditsViewModel), typeof(EditClassCreditsViewController) },
                { typeof(ConfigureClassWeightCategoriesViewModel), typeof(EditClassWeightCategoriesViewController) },

                { typeof(SettingsViewModel), typeof(SettingsViewController) },
                { typeof(SettingsListViewModel), typeof(SettingsListViewController) },
                { typeof(MyAccountViewModel), typeof(MyAccountViewController) },
                { typeof(ConfirmIdentityViewModel), typeof(ConfirmIdentityViewController) },
                { typeof(ChangeUsernameViewModel), typeof(ChangeUsernameViewController) },
                { typeof(ChangePasswordViewModel), typeof(ChangePasswordViewController) },
                { typeof(ChangeEmailViewModel), typeof(ChangeEmailViewController) },
                { typeof(ConvertToOnlineViewModel), typeof(ConvertToOnlineViewController) },
                { typeof(DeleteAccountViewModel), typeof(DeleteAccountViewController) },
                { typeof(UpdateCredentialsViewModel), typeof(UpdateCredentialsViewController) },
                { typeof(ForgotUsernameViewModel), typeof(ForgotUsernameViewController) },
                { typeof(RecoveredUsernamesViewModel), typeof(RecoveredUsernamesViewController) },
                { typeof(ResetPasswordViewModel), typeof(ResetPasswordViewController) },
                { typeof(AboutViewModel), typeof(AboutViewController) },
                { typeof(AboutViewModelAsPopup), typeof(AboutViewControllerAsPopup) },
                { typeof(ReminderSettingsViewModel), typeof(ReminderSettingsViewController) },
                { typeof(TwoWeekScheduleSettingsViewModel), typeof(TwoWeekScheduleSettingsViewController) },

                { typeof(ConfigureClassGradesViewModel), typeof(ConfigureClassGradesViewController) },
                { typeof(ConfigureClassGradesListViewModel), typeof(ConfigureClassGradesListViewController) },
                { typeof(ConfigureClassAverageGradesViewModel), typeof(ConfigureClassAverageGradesViewController) },
                { typeof(ConfigureClassRoundGradesUpViewModel), typeof(ConfigureClassRoundGradesUpViewController) },
                { typeof(ConfigureClassGradeScaleViewModel), typeof(ConfigureClassGradeScaleViewController) },
                { typeof(ConfigureClassGpaTypeViewModel), typeof(ConfigureClassGpaTypeViewController) },
                { typeof(ConfigureClassPassingGradeViewModel), typeof(ConfigureClassPassingGradeViewController) },
                { typeof(SaveGradeScaleViewModel), typeof(SaveGradeScaleViewController) }
            };
        }

        public override void OnActivated(UIApplication application)
        {
            base.OnActivated(application);

            try
            {
                foreach (var window in PowerPlannerApp.Current.Windows.OfType<MainAppWindow>())
                {
                    var dontWait = window.GetViewModel().HandleBeingReturnedTo();
                }
            }

            catch { }
        }

        public override void DidEnterBackground(UIApplication application)
        {
            base.DidEnterBackground(application);

            try
            {
                foreach (var window in PowerPlannerApp.Current.Windows.OfType<MainAppWindow>())
                {
                    window.GetViewModel().HandleBeingLeft();
                }
            }

            catch { }
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            _hasActivatedWindow = false;

            // Start telemetry and crash handling
            AppCenter.Start(Secrets.AppCenterAppSecret,
                   typeof(Analytics), typeof(Crashes));

            // Request notification permissions from the user
            if (SdkSupportHelper.IsNotificationsSupported)
            {
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
                {
                    // Don't need to do anything to handle approval
                });

                UNUserNotificationCenter.Current.Delegate = new MyUserNotificationCenterDelegate();

                RemindersExtension.Current = new IOSRemindersExtension();
            }

            TelemetryExtension.Current = new iOSTelemetryExtension();
            InAppPurchaseExtension.Current = new iOSInAppPurchaseExtension();

            bool result = base.FinishedLaunching(application, launchOptions);

            RegisterWindow();

            return result;
        }

        private class MyUserNotificationCenterDelegate : UNUserNotificationCenterDelegate
        {
            public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
            {
                completionHandler(UNNotificationPresentationOptions.Alert);
            }

            public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
            {
                try
                {
                    if (response.IsDefaultAction)
                    {
                        string identifier = response.Notification.Request.Identifier;

                        if (IOSRemindersExtension.TryParseAccount(identifier, out Guid localAccountId))
                        {
                            if (IOSRemindersExtension.TryParseDayBeforeIdentifier(identifier, out DateTime dateOfArtificalToday))
                            {
                                TelemetryExtension.Current?.TrackEvent($"Launch_FromToast_DayBefore");

                                DateTime dateToShow = dateOfArtificalToday.AddDays(1);

                                // Show day view
                                HandleLaunch((viewModel) =>
                                {
                                    viewModel.HandleViewDayActivation(localAccountId, dateToShow);
                                    return Task.FromResult(true);
                                });
                            }

                            else if (IOSRemindersExtension.TryParsingDayOfTaskIdentifier(identifier, out Guid taskIdentifier))
                            {
                                TelemetryExtension.Current?.TrackEvent($"Launch_FromToast_HomeworkExam");

                                // Show task
                                HandleLaunch(async (viewModel) =>
                                {
                                    await viewModel.HandleViewHomeworkActivation(localAccountId, taskIdentifier);
                                });
                            }

                            else if (IOSRemindersExtension.TryParsingDayOfEventIdentifier(identifier, out Guid eventIdentifier))
                            {
                                TelemetryExtension.Current?.TrackEvent($"Launch_FromToast_HomeworkExam");

                                // Show task
                                HandleLaunch(async (viewModel) =>
                                {
                                    await viewModel.HandleViewExamActivation(localAccountId, eventIdentifier);
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
                finally
                {
                    // Inform caller it has been handled
                    completionHandler();
                }
            }

            private async void HandleLaunch(Func<MainWindowViewModel, Task> action)
            {
                if (_hasActivatedWindow)
                {
                    try
                    {
                        var viewModel = PowerPlannerApp.Current.GetMainWindowViewModel();
                        if (viewModel != null)
                        {
                            await action(viewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }
                else
                {
                    _handleLaunchAction = action;
                }
            }
        }

        public static bool _hasActivatedWindow;
        public static Func<MainWindowViewModel, Task> _handleLaunchAction;
        private async void RegisterWindow()
        {
            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);

            Window.BackgroundColor = new UIColor(1, 1);
            Window.TintColor = ColorResources.PowerPlannerAccentBlue;
            this.Window.RootViewController = UIStoryboard.FromName("LaunchScreen", null).InstantiateInitialViewController();

            this.Window.MakeKeyAndVisible();

            _mainAppWindow = new MainAppWindow();
            await PortableApp.Current.RegisterWindowAsync(_mainAppWindow, new NativeiOSAppWindow(Window));

            // Launch the app
            await _mainAppWindow.GetViewModel().HandleNormalLaunchActivation();

            ViewManager.RootViewModel = _mainAppWindow.ViewModel;
        }

        public override Type GetPortableAppType()
        {
            return typeof(PowerPlanneriOSApp);
        }
    }
}