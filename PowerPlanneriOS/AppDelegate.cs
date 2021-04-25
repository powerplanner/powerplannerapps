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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
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
using UserNotifications;
using InterfacesiOS.Helpers;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlanneriOS.Controllers.Settings.Grades;
using PowerPlannerAppDataLibrary.ViewModels;

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

        public override Dictionary<Type, Type> GetGenericViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>
            {
                { typeof(PopupComponentViewModel), typeof(PopupComponentViewController) },
                { typeof(ComponentViewModel), typeof(ComponentViewController) } // This needs to be after Popup since Popup is a subclass
            };
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                // Welcome views
                { typeof(WelcomeViewModel), typeof(WelcomeViewController) },
                { typeof(LoginViewModel), typeof(LoginViewController) },
                { typeof(CreateAccountViewModel), typeof(CreateAccountViewController) },
                { typeof(ExistingUserViewModel), typeof(ExistingUserViewController) },
                { typeof(ConnectAccountViewModel), typeof(ConnectAccountViewController) },

                { typeof(InitialSyncViewModel), typeof(InitialSyncViewController) },
                { typeof(MainScreenViewModel), typeof(MainScreenViewController) },
                { typeof(CalendarViewModel), typeof(CalendarViewController) },
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
                { typeof(AddTaskOrEventViewModel), typeof(AddTaskOrEventViewController) },
                { typeof(ViewTaskOrEventViewModel), typeof(ViewTaskOrEventViewController) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsViewController) },
                { typeof(AddHolidayViewModel), typeof(AddHolidayViewController) },
                { typeof(ViewGradeViewModel), typeof(ViewGradeViewController) },
                { typeof(AddGradeViewModel), typeof(AddGradeViewController) },

                // Settings views
                { typeof(SettingsViewModel), typeof(SettingsViewController) },
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
                { typeof(SuccessfullyCreatedAccountViewModel), typeof(SuccessfullyCreatedAccountViewController) },
                { typeof(SchoolTimeZoneSettingsViewModel), typeof(SchoolTimeZoneSettingsViewController) },

                { typeof(ConfigureClassGradesViewModel), typeof(ConfigureClassGradesViewController) },
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

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            ShortcutAction? action = ConvertShortcutItem(shortcutItem);
            if (action != null)
            {
                HandleShortcutAction(action.Value);
            }
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            try
            {
                // https://docs.microsoft.com/en-us/xamarin/ios/platform/user-notifications/deprecated/remote-notifications-in-ios#registering-with-apns
                string pushToken = ParsePushToken(deviceToken);
                iOSPushExtension.RegisteredForRemoteNotifications(pushToken);
            }
            catch (Exception ex)
            {
                try
                {
                    iOSPushExtension.FailedToRegisterForRemoteNotifications(ex.ToString());
                }
                catch (Exception ex2)
                {
                    TelemetryExtension.Current?.TrackException(ex2);
                }
            }
        }

        private static string ParsePushToken(NSData deviceToken)
        {
            // https://onesignal.com/blog/ios-13-introduces-4-breaking-changes-to-notifications/
            // https://medium.com/@kevinle/correctly-capture-ios-13-device-token-in-xamarin-3d0fa390b71b

            int length = (int)deviceToken.Length;
            if (deviceToken.Length == 0)
            {
                return null;
            }

            string[] hexArray = deviceToken.Select(b => b.ToString("x2")).ToArray();
            return string.Join(string.Empty, hexArray);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            // Might fail if not connected to network, APNs servers unreachable, or doesn't have proper code-signing entitlement
            try
            {
                iOSPushExtension.FailedToRegisterForRemoteNotifications(error.ToString());
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            _hasActivatedWindow = false;

            // Start telemetry and crash handling
            AppCenter.Start(Secrets.AppCenterAppSecret,
                   typeof(Analytics), typeof(Crashes));

            TelemetryExtension.Current = new iOSTelemetryExtension();
            InAppPurchaseExtension.Current = new iOSInAppPurchaseExtension();
            PushExtension.Current = new iOSPushExtension();

            if (SdkSupportHelper.IsNotificationsSupported)
            {
                UNUserNotificationCenter.Current.Delegate = new MyUserNotificationCenterDelegate(this);

                RemindersExtension.Current = new IOSRemindersExtension();
            }

            // Get whether launched from shortcut
            ShortcutAction? shortcutAction = null;
            if (launchOptions != null && UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                var shortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
                shortcutAction = ConvertShortcutItem(shortcutItem);
            }

            bool result = base.FinishedLaunching(application, launchOptions);

            RegisterWindow(shortcutAction);

            return result;
        }

        public override async void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // Payload reference: https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server/generating_a_remote_notification

            /*
             * {
             *   "content-available": 1
             *   "action": "syncAccount",
             *   "accountId": 49831
             * }
             * */

            try
            {
                if (userInfo.TryGetValue(new NSString("action"), out NSObject actionValue)
                    && actionValue is NSString actionValueStr
                    && actionValueStr == "syncAccount"
                    && userInfo.TryGetValue(new NSString("accountId"), out NSObject accountIdValue)
                    && accountIdValue is NSNumber accountIdNum)
                {
                    long accountId = accountIdNum.Int64Value;

                    await PowerPlannerApp.SyncAccountInBackgroundAsync(accountId);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            try
            {
                completionHandler(UIBackgroundFetchResult.NewData);
            }
            catch { }
        }

        private static ShortcutAction? ConvertShortcutItem(UIApplicationShortcutItem item)
        {
            if (item == null)
            {
                return null;
            }

            switch (item.Type)
            {
                case "com.barebonesdev.powerplanner.addtask":
                    return ShortcutAction.AddTask;

                case "com.barebonesdev.powerplanner.addevent":
                    return ShortcutAction.AddEvent;
            }

            return null;
        }

        private enum ShortcutAction
        {
            AddTask,
            AddEvent
        }

        private class MyUserNotificationCenterDelegate : UNUserNotificationCenterDelegate
        {
            private AppDelegate _appDelegate;
            public MyUserNotificationCenterDelegate(AppDelegate appDelegate)
            {
                _appDelegate = appDelegate;
            }

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
                                if (AppDelegate._hasActivatedWindow)
                                {
                                    _appDelegate.HandleLaunch(async (viewModel) =>
                                    {
                                        await viewModel.HandleViewDayActivation(localAccountId, dateToShow);
                                    });
                                }
                                else
                                {
                                    // We just set the properties, weird stuff seemed to happen if we tried to use the unified activation methods
                                    CalendarViewModel.SetInitialDisplayState(CalendarViewModel.DisplayStates.Day, dateToShow);
                                    NavigationManager.MainMenuSelection = NavigationManager.MainMenuSelections.Calendar;
                                }
                            }

                            else if (IOSRemindersExtension.TryParsingDayOfTaskIdentifier(identifier, out Guid taskIdentifier))
                            {
                                TelemetryExtension.Current?.TrackEvent($"Launch_FromToast_Task");

                                // Show task
                                _appDelegate.HandleLaunch(async (viewModel) =>
                                {
                                    await viewModel.HandleViewTaskActivation(localAccountId, taskIdentifier);
                                });
                            }

                            else if (IOSRemindersExtension.TryParsingDayOfEventIdentifier(identifier, out Guid eventIdentifier))
                            {
                                TelemetryExtension.Current?.TrackEvent($"Launch_FromToast_Event");

                                // Show task
                                _appDelegate.HandleLaunch(async (viewModel) =>
                                {
                                    await viewModel.HandleViewEventActivation(localAccountId, eventIdentifier);
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

        public static bool _hasActivatedWindow;
        public static Func<MainWindowViewModel, Task> _handleLaunchAction;
        private async void RegisterWindow(ShortcutAction? shortcutAction)
        {
            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);

            Window.BackgroundColor = UIColorCompat.SystemBackgroundColor;
            Window.TintColor = ColorResources.PowerPlannerAccentBlue;
            this.Window.RootViewController = UIStoryboard.FromName("LaunchScreen", null).InstantiateInitialViewController();

            this.Window.MakeKeyAndVisible();

            _mainAppWindow = new MainAppWindow();
            await PortableApp.Current.RegisterWindowAsync(_mainAppWindow, new NativeiOSAppWindow(Window));

            // Launch the app
            var mainWindowViewModel = _mainAppWindow.GetViewModel();
            if (shortcutAction != null)
            {
                HandleShortcutAction(shortcutAction.Value);

                // We make sure to activate the normal launch, and then later the HandleLaunch kicks in
                if (!_hasActivatedWindow)
                {
                    await mainWindowViewModel.HandleNormalLaunchActivation();
                }
            }
            else
            {
                await mainWindowViewModel.HandleNormalLaunchActivation();
            }

            ViewManager.RootViewModel = _mainAppWindow.ViewModel;
        }

        private void HandleShortcutAction(ShortcutAction action)
        {
            TelemetryExtension.Current?.TrackEvent($"Launch_FromJumpList_QuickAdd" + (action == ShortcutAction.AddTask ? "Task" : "Event"));

            // This works unless there's currently a popup open (like view task is open)
            // So the fact that the shared code clears all popups and then adds a popup messes things up...
            // My iOS code doesn't like all popups being cleared and then a new popup being added immediately...
            HandleLaunch(async (viewModel) =>
            {
                switch (action)
                {
                    case ShortcutAction.AddTask:
                        await viewModel.HandleQuickAddTask();
                        break;

                    case ShortcutAction.AddEvent:
                        await viewModel.HandleQuickAddEvent();
                        break;
                }
            });
        }

        public override Type GetPortableAppType()
        {
            return typeof(PowerPlanneriOSApp);
        }
    }
}