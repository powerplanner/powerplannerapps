using BackgroundTasksProject;
using InterfacesUWP;
using PowerPlannerAppDataLibrary;
using PowerPlannerSending;
using PowerPlannerUWP.ViewModel;
using PowerPlannerUWP.Views;
using PowerPlannerUWP.Views.ClassViews;
using PowerPlannerUWP.Views.GradeViews;
using PowerPlannerUWP.Views.HomeworkViews;
using PowerPlannerUWP.Views.SettingsViews;
using PowerPlannerUWP.Views.YearViews;
using PowerPlannerUWP.WindowHosts;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using UpgradeFromSilverlight;
using UpgradeFromWin8;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using PowerPlannerUWPLibrary.Extensions;
using StorageEverywhere;
using InterfacesUWP.ViewModelPresenters;
using Windows.UI.Xaml.Data;
using InterfacesUWP.App;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using InterfacesUWP.AppWindows;
using PowerPlannerAppDataLibrary.Windows;
using PowerPlannerUWP.ViewModel.Settings;
using PowerPlannerUWP.ViewModel.Promos;
using PowerPlannerUWP.Views.Promos;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWPLibrary.Helpers;
using PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Schedule;
using Windows.ApplicationModel.DataTransfer;
using PowerPlannerAppDataLibrary.Helpers;
using Windows.System.Profile;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlannerUWP.Views.SettingsViews.Grades;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos;
using PowerPlannerUWP.Views.WelcomeViews;

namespace PowerPlannerUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : NativeUwpApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
#if DEBUG
            //ApplicationLanguages.PrimaryLanguageOverride = "es";
#endif

            this.UnhandledException += App_UnhandledException;

            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;

            var dontWait = ConfigureJumpList();
        }

        public override Type GetPortableAppType()
        {
            return typeof(PowerPlannerUwpApp);
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                // Welcome views
                { typeof(ExistingUserViewModel), typeof(ExistingUserView) },
                { typeof(ConnectAccountViewModel), typeof(ConnectAccountView) },

                // Main views
                { typeof(InitialSyncViewModel), typeof(InitialSyncView) },
                { typeof(AddClassTimeViewModel), typeof(AddClassTimeView) },
                { typeof(AddClassViewModel), typeof(AddClassView) },
                { typeof(AddGradeViewModel), typeof(AddGradeView) },
                { typeof(AddHolidayViewModel), typeof(AddHolidayView) },
                { typeof(AddHomeworkViewModel), typeof(AddHomeworkView) },
                { typeof(AddSemesterViewModel), typeof(AddSemesterView) },
                { typeof(AddYearViewModel), typeof(AddYearView) },
                { typeof(AgendaViewModel), typeof(AgendaView) },
                { typeof(CalendarViewModel), typeof(CalendarMainView) },
                { typeof(ClassesViewModel), typeof(ClassesView) },
                { typeof(ClassViewModel), typeof(ClassView) },
                { typeof(ClassWhatIfViewModel), typeof(ClassWhatIfView) },
                { typeof(CreateAccountViewModel), typeof(CreateAccountView) },
                { typeof(EditClassDetailsViewModel), typeof(EditClassDetailsView) },
                { typeof(ForgotUsernameViewModel), typeof(ForgotUsernameView) },
                { typeof(LoginViewModel), typeof(LoginView) },
                { typeof(DayViewModel), typeof(MainContentDayView) },
                { typeof(MainScreenViewModel), typeof(MainScreenView) },
                { typeof(PremiumVersionViewModel), typeof(PremiumVersionView) },
                { typeof(PromoOtherPlatformsViewModel), typeof(PromoOtherPlatformsView) },
                { typeof(QuickAddViewModel), typeof(QuickAddView) },
                { typeof(RecoveredUsernamesViewModel), typeof(RecoveredUsernamesView) },
                { typeof(ResetPasswordViewModel), typeof(ResetPasswordView) },
                { typeof(SaveGradeScaleViewModel), typeof(SaveGradeScaleView) },
                { typeof(ScheduleViewModel), typeof(ScheduleView) },
                { typeof(ExportSchedulePopupViewModel), typeof(ExportSchedulePopupView) },
                { typeof(SettingsListViewModel), typeof(SettingsListView) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsView) },
                { typeof(UpdateCredentialsViewModel), typeof(UpdateCredentialsView) },
                { typeof(ViewGradeViewModel), typeof(ViewGradeView) },
                { typeof(ViewHomeworkViewModel), typeof(ViewHomeworkView) },
                { typeof(ShowImagesViewModel), typeof(ShowImagesView) },
                { typeof(WelcomeViewModel), typeof(WelcomeView) },
                { typeof(YearsViewModel), typeof(YearsView) },

                // Settings views
                { typeof(AboutViewModel), typeof(AboutView) },
                { typeof(TileSettingsViewModel), typeof(BaseSettingsSplitView) },
                { typeof(SyncOptionsViewModel), typeof(BaseSettingsSplitView) },
                { typeof(CalendarIntegrationViewModel), typeof(BaseSettingsSplitView) },
                { typeof(CalendarIntegrationClassesViewModel), typeof(CalendarIntegrationClassesView) },
                { typeof(CalendarIntegrationTasksViewModel), typeof(CalendarIntegrationTasksView) },
                { typeof(ChangeEmailViewModel), typeof(ChangeEmailView) },
                { typeof(ChangePasswordViewModel), typeof(ChangePasswordView) },
                { typeof(ChangeUsernameViewModel), typeof(ChangeUsernameView) },
                { typeof(ClassTilesViewModel), typeof(ClassTilesView) },
                { typeof(ClassTileViewModel), typeof(ClassTileView) },
                { typeof(ConfirmIdentityViewModel), typeof(ConfirmIdentityView) },
                { typeof(ConvertToOnlineViewModel), typeof(ConvertToOnlineView) },
                { typeof(DeleteAccountViewModel), typeof(DeleteAccountView) },
                { typeof(ImageUploadOptionsViewModel), typeof(ImageUploadOptionsView) },
                { typeof(MainTileViewModel), typeof(MainTileView) },
                { typeof(MyAccountViewModel), typeof(MyAccountView) },
                { typeof(PushSettingsViewModel), typeof(PushSettingsView) },
                { typeof(QuickAddTileViewModel), typeof(QuickAddTileView) },
                { typeof(ReminderSettingsViewModel), typeof(ReminderSettingsView) },
                { typeof(ScheduleTileViewModel), typeof(ScheduleTileView) },
                { typeof(TwoWeekScheduleSettingsViewModel), typeof(TwoWeekScheduleSettingsView) },
                { typeof(GoogleCalendarIntegrationViewModel), typeof(GoogleCalendarIntegrationView) },
                { typeof(ConfigureClassGradesListViewModel), typeof(ConfigureClassGradesListView) },
                { typeof(ConfigureClassCreditsViewModel), typeof(ConfigureClassCreditsView) },
                { typeof(ConfigureClassWeightCategoriesViewModel), typeof(ConfigureClassWeightCategoriesView) },
                { typeof(ConfigureClassGradeScaleViewModel), typeof(ConfigureClassGradeScaleView) },
                { typeof(ConfigureClassAverageGradesViewModel), typeof(ConfigureClassAverageGradesView) },
                { typeof(ConfigureClassRoundGradesUpViewModel), typeof(ConfigureClassRoundGradesUpView) },
                { typeof(ConfigureClassGpaTypeViewModel), typeof(ConfigureClassGpaTypeView) },
                { typeof(ConfigureClassPassingGradeViewModel), typeof(ConfigureClassPassingGradeView) },
                { typeof(PromoContributeViewModel), typeof(PromoContributeView) },
                { typeof(SuccessfullyCreatedAccountViewModel), typeof(SuccessfullyCreatedAccountView) }
            };
        }

        private void OnEntering()
        {
            try
            {
                // Remove/cancel all background tasks when the app is open
                UnregisterAllBackgroundTasks();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void OnResuming(object sender, object e)
        {
            OnEntering();
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TelemetryExtension.Current?.TrackException(e.Exception);
        }
        
        protected override async System.Threading.Tasks.Task OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            try
            {
                // Always call this just to reset value back to false, since Resuming isn't called all the time
                AccountDataStore.RetrieveAndResetWasUpdatedByBackgroundTask();

#if DEBUG
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    this.DebugSettings.EnableFrameRateCounter = true;
                //}
#endif
                
                OnEntering();

                // Wait for initialization to complete, to ensure we don't accidently add multiple windows
                // Although right now we don't even do any async tasks, so this will be useless
                await PowerPlannerApp.InitializeTask;

                MainAppWindow mainAppWindow;

                // If no windows, need to register window
                mainAppWindow = PowerPlannerApp.Current.Windows.OfType<MainAppWindow>().FirstOrDefault();
                if (mainAppWindow == null)
                {
                    // This configures the view models, does NOT call Activate yet
                    var nativeWindow = new NativeUwpAppWindow();
                    mainAppWindow = new MainAppWindow();
                    await PowerPlannerApp.Current.RegisterWindowAsync(mainAppWindow, nativeWindow);

                    if (PowerPlannerApp.Current.Windows.Count > 1)
                    {
                        throw new Exception("There are more than 1 windows registered");
                    }
                }

                if (e is LaunchActivatedEventArgs)
                {
                    var launchEventArgs = e as LaunchActivatedEventArgs;
                    var launchContext = !object.Equals(launchEventArgs.TileId, "App") ? LaunchSurface.SecondaryTile : LaunchSurface.Normal;
                    if (launchContext == LaunchSurface.Normal)
                    {
                        // Track whether was launched from primary tile
                        if (ApiInformation.IsPropertyPresent(typeof(LaunchActivatedEventArgs).FullName, nameof(LaunchActivatedEventArgs.TileActivatedInfo)))
                        {
                            if (launchEventArgs.TileActivatedInfo != null)
                            {
                                launchContext = LaunchSurface.PrimaryTile;
                            }
                        }
                    }

                    await HandleArguments(mainAppWindow, launchEventArgs.Arguments, launchContext);
                }

                else if (e is ToastNotificationActivatedEventArgs)
                {
                    var args = e as ToastNotificationActivatedEventArgs;

                    await HandleArguments(mainAppWindow, args.Argument, LaunchSurface.Toast);
                }

                else if (e is ProtocolActivatedEventArgs)
                {
                    var protocolEventArgs = e as ProtocolActivatedEventArgs;

                    if (!string.IsNullOrWhiteSpace(protocolEventArgs.Uri.PathAndQuery) && protocolEventArgs.Uri.PathAndQuery.StartsWith("?"))
                        await HandleArguments(mainAppWindow, protocolEventArgs.Uri.PathAndQuery.Substring(1), LaunchSurface.Uri);
                }

                else if (e is AppointmentsProviderShowAppointmentDetailsActivatedEventArgs)
                {
                    // Note that this code is essentially deprecated and doesn't get hit... Uri launch happens instead
                    var showDetailsArgs = e as AppointmentsProviderShowAppointmentDetailsActivatedEventArgs;

                    try
                    {
                        AppointmentsHelper.RoamingIdData data = AppointmentsHelper.RoamingIdData.FromString(showDetailsArgs.RoamingId);

                        string finalArgs = null;

                        switch (data.ItemType)
                        {
                            case ItemType.Homework:
                                finalArgs = new ViewHomeworkArguments()
                                {
                                    LocalAccountId = data.LocalAccountId,
                                    ItemId = data.Identifier
                                }.SerializeToString();
                                break;

                            case ItemType.Exam:
                                finalArgs = new ViewExamArguments()
                                {
                                    LocalAccountId = data.LocalAccountId,
                                    ItemId = data.Identifier
                                }.SerializeToString();
                                break;

                            case ItemType.Task:
                                // TODO: Not supported yet
                                break;

                            case ItemType.Schedule:
                                finalArgs = new ViewScheduleArguments()
                                {
                                    LocalAccountId = data.LocalAccountId
                                }.SerializeToString();
                                break;

                            case ItemType.MegaItem:
                                break;
                        }

                        if (finalArgs != null)
                            await HandleArguments(mainAppWindow, finalArgs, LaunchSurface.Calendar);
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }

                if (mainAppWindow.GetViewModel().Content == null)
                {
                    await mainAppWindow.GetViewModel().HandleNormalLaunchActivation();
                }

                Window.Current.Activate();

                // Listen to window activation changes
                Window.Current.Activated += Current_Activated;

                // Set up the default window properties
                ConfigureWindowProperties();

                // Set up the sharing support
                ConfigureDataTransferManager();

                // Display updates
                HandleVersionChange();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            switch (e.WindowActivationState)
            {
                case CoreWindowActivationState.CodeActivated:
                case CoreWindowActivationState.PointerActivated:

                    try
                    {
                        foreach (var window in PowerPlannerApp.Current.Windows.OfType<MainAppWindow>())
                        {
                            var dontWait = window.GetViewModel().HandleBeingReturnedTo();
                        }
                    }

                    catch { }

                    break;

                case CoreWindowActivationState.Deactivated:

                    try
                    {
                        foreach (var window in PowerPlannerApp.Current.Windows.OfType<MainAppWindow>())
                        {
                            (window.ViewModel as MainWindowViewModel)?.HandleBeingLeft();
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }

                    break;
            }
        }

        private static async System.Threading.Tasks.Task HandleArguments(MainAppWindow mainAppWindow, string arguments, LaunchSurface launchContext)
        {
            try
            {
                MainWindowViewModel viewModel = mainAppWindow.GetViewModel();

                var args = ArgumentsHelper.Parse(arguments);

                Guid desiredLocalAccountId = Guid.Empty;

                if (args is BaseArgumentsWithAccount)
                    desiredLocalAccountId = (args as BaseArgumentsWithAccount).LocalAccountId;
                else
                    desiredLocalAccountId = AccountsManager.GetLastLoginLocalId();

                Guid currentLocalAccountId = Guid.Empty;
                if (viewModel.CurrentAccount != null)
                    currentLocalAccountId = viewModel.CurrentAccount.LocalAccountId;
                
                // View schedule
                if (args is ViewScheduleArguments)
                {
                    TrackLaunch(args, launchContext, "Schedule");
                    await viewModel.HandleViewScheduleActivation(desiredLocalAccountId);
                }

                // View class
                else if (args is ViewClassArguments)
                {
                    TrackLaunch(args, launchContext, "Class");
                    var viewClassArgs = args as ViewClassArguments;

                    await viewModel.HandleViewClassActivation(viewClassArgs.LocalAccountId, viewClassArgs.ItemId);
                }

                else if (args is ViewHomeworkArguments)
                {
                    TrackLaunch(args, launchContext, "HomeworkExam");
                    var viewHomeworkArgs = args as ViewHomeworkArguments;
                    await viewModel.HandleViewHomeworkActivation(viewHomeworkArgs.LocalAccountId, viewHomeworkArgs.ItemId);
                }

                else if (args is ViewExamArguments)
                {
                    TrackLaunch(args, launchContext, "HomeworkExam");
                    var viewExamArgs = args as ViewExamArguments;
                    await viewModel.HandleViewExamActivation(viewExamArgs.LocalAccountId, viewExamArgs.ItemId);
                }

                else if (args is ViewHolidayArguments)
                {
                    TrackLaunch(args, launchContext, "Holiday");
                    var viewHolidayArgs = args as ViewHolidayArguments;
                    await viewModel.HandleViewHolidayActivation(viewHolidayArgs.LocalAccountId, viewHolidayArgs.ItemId);
                }


                else if (args is QuickAddArguments)
                {
                    TrackLaunch(args, launchContext, "QuickAdd");
                    var quickAddArgs = args as QuickAddArguments;
                    await viewModel.HandleQuickAddActivation(quickAddArgs.LocalAccountId);
                }

                else if (args is QuickAddHomeworkToCurrentAccountArguments)
                {
                    // Jump list was created before we included the launch surface, so we'll manually port it
                    if (launchContext == LaunchSurface.Normal)
                    {
                        launchContext = LaunchSurface.JumpList;
                    }
                    TrackLaunch(args, launchContext, "QuickAddHomework");
                    await viewModel.HandleQuickAddHomework();
                }

                else if (args is QuickAddExamToCurrentAccountArguments)
                {
                    // Jump list was created before we included the launch surface, so we'll manually port it
                    if (launchContext == LaunchSurface.Normal)
                    {
                        launchContext = LaunchSurface.JumpList;
                    }
                    TrackLaunch(args, launchContext, "QuickAddExam");
                    await viewModel.HandleQuickAddExam();
                }

                else
                {
                    TrackLaunch(args, launchContext, "Launch");
                    if (viewModel.Content == null)
                    {
                        await viewModel.HandleNormalLaunchActivation();
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void TrackLaunch(BaseArguments args, LaunchSurface launchSurface, string action)
        {
            if (launchSurface == LaunchSurface.Uri || launchSurface == LaunchSurface.Normal)
            {
                if (args != null)
                {
                    launchSurface = args.LaunchSurface;
                }
            }

            if (launchSurface != LaunchSurface.Normal)
            {
                TelemetryExtension.Current?.TrackEvent($"Launch_From{launchSurface}_{action}");
            }
        }

        private static async System.Threading.Tasks.Task ConfigureJumpList()
        {
            try
            {
                if (ApiInformation.IsTypePresent("Windows.UI.StartScreen.JumpList") && Windows.UI.StartScreen.JumpList.IsSupported())
                {
                    var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();


                    if (jumpList.Items.Count == 0)
                    {
                        jumpList.SystemGroupKind = Windows.UI.StartScreen.JumpListSystemGroupKind.Frequent;

                        var addHomeworkItem = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(new QuickAddHomeworkToCurrentAccountArguments().SerializeToString(), "New task");
                        addHomeworkItem.Logo = new Uri("ms-appx:///Assets/JumpList/Add.png");
                        jumpList.Items.Add(addHomeworkItem);

                        var addExamItem = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(new QuickAddExamToCurrentAccountArguments().SerializeToString(), "New event");
                        addExamItem.Logo = new Uri("ms-appx:///Assets/JumpList/Add.png");
                        jumpList.Items.Add(addExamItem);

                        await jumpList.SaveAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void ConfigureWindowProperties()
        {
            try
            {
                var view = ApplicationView.GetForCurrentView();

                // Set up the title bar
                var titleBar = view.TitleBar;

                titleBar.BackgroundColor = Color.FromArgb(255, 26, 32, 74);
                titleBar.ForegroundColor = Colors.White;

                titleBar.InactiveBackgroundColor = Color.FromArgb(255, 73, 79, 117);
                titleBar.InactiveForegroundColor = Colors.LightGray;


                titleBar.ButtonBackgroundColor = titleBar.BackgroundColor;
                titleBar.ButtonForegroundColor = titleBar.ForegroundColor;

                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 75, 96, 179);
                titleBar.ButtonHoverForegroundColor = Colors.White;

                titleBar.ButtonInactiveBackgroundColor = titleBar.InactiveBackgroundColor;
                titleBar.ButtonInactiveForegroundColor = titleBar.InactiveForegroundColor;

                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 84, 107, 199);
                titleBar.ButtonPressedForegroundColor = Colors.White;


                // Set up the min window size
                view.SetPreferredMinSize(new Size(300, 300));



                // Set up status bar
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

                    statusBar.BackgroundColor = (Color)Application.Current.Resources["PowerPlannerBlueColor"];
                    statusBar.BackgroundOpacity = 1;
                    statusBar.ForegroundColor = Colors.White;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static bool IsSharingSupported { get; private set; }
        private static Action<DataRequestedEventArgs> _shareDataRequestedHandler { get; set; }
        private static bool _configuredDataTransferManager;

        private static void ConfigureDataTransferManager()
        {
            if (_configuredDataTransferManager)
            {
                return;
            }

            _configuredDataTransferManager = true;

            try
            {
                // Hook up share handler
                // IsSupported method was added in API contract 3
                bool isSupported = ApiInformation.IsMethodPresent(typeof(DataTransferManager).FullName, nameof(DataTransferManager.IsSupported));
                // If the IsSupported method exists
                if (isSupported)
                {
                    // Use that to determine whether it's supported
                    isSupported = DataTransferManager.IsSupported();
                }
                else
                {
                    // Otherwise, only desktop/mobile before API contract 3 supports it
                    isSupported = !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3) &&
                        (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop" || AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile");
                }

                if (isSupported)
                {
                    var dataTransferManager = DataTransferManager.GetForCurrentView();
                    dataTransferManager.DataRequested += DataTransferManager_DataRequested;

                    IsSharingSupported = true;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static void ShowShareUI(Action<DataRequestedEventArgs> dataRequestedHandler)
        {
            try
            {
                _shareDataRequestedHandler = dataRequestedHandler;
                DataTransferManager.ShowShareUI();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                _shareDataRequestedHandler = null;
            }
        }

        private static void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            try
            {
                var handler = _shareDataRequestedHandler;

                if (handler != null)
                {
                    _shareDataRequestedHandler = null;

                    handler(args);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Awaiting this will await on the background request access, not on the message dialog
        /// </summary>
        /// <returns></returns>
        private static void HandleVersionChange()
        {
            const string VERSION = "Version";

            try
            {
                object o;
                Version v;
                AppSettings.Current.TryGetValue(VERSION, out o);

                // If new user
                if (o == null)
                {
                    // Set to current version
                    AppSettings.Current[VERSION] = Variables.VERSION.ToString();

                    return;
                }

                v = Version.Parse(o as string);

                if (v < Variables.VERSION)
                {
                    // Set to current version
                    AppSettings.Current[VERSION] = Variables.VERSION.ToString();


                    string changedText = "";

                    // Message about the display scaling issue
                    if (v < new Version(3, 0, 6, 0))
                        changedText = "\nIf the app is appearing too large, PLEASE EMAIL ME! My email is support@powerplanner.net (you can find it in Settings -> About).";


                    if (v <= new Version(2001, 26, 2, 0))
                    {
                        if (Controls.TimePickers.TextBasedTimePicker.IsSupported && AbTestHelper.Tests.NewTimePicker)
                        {
                            changedText += "\n - New text-based time pickers! Let me know what you think!";
                        }
                    }
                    
                    if (v <= new Version(2001, 9, 1, 0))
                    {
                        changedText += "\n - Hyperlink detection in details text!";
                    }

                    if (v < new Version(1911, 2))
                    {
                        changedText += "\n - Support for Monday as first day of week on Calendar for countries like Spain!";
                    }

                    if (v < new Version(1909, 12, 1, 0))
                    {
                        changedText += "\n - Fixed duplicate calendar entries in Windows Calendar integration";
                    }

                    if (v < new Version(5, 4, 76, 0))
                    {
                        changedText += "\n - Strikethrough for completed items";
                    }

                    if (v < new Version(5, 4, 72, 0) && v >= new Version(5, 4, 64, 0))
                    {
                        changedText += "\n - Fixed GPA calculation for failed classes";
                    }

                    if (v < new Version(5, 4, 64, 0))
                    {
                        changedText += "\n - Pass/fail classes are now supported in the GPA grade system!\n - Custom selected due times are remembered for faster task entry";
                    }

                    if (v < new Version(5, 4, 60, 0) && InterfacesUWP.ColorPicker.IsCustomPickerSupported.Value)
                    {
                        changedText += "\n - Custom color picker for class colors!";
                    }

                    if (v < new Version(5, 4, 36, 0))
                    {
                        changedText += "\n - Repeating bulk entry of tasks/events!";
                    }

                    if (v < new Version(5, 4, 32, 0))
                    {
                        changedText += "\n - Google Calendar integration! Go to the Settings page to try it out (requires an online account)";
                    }

                    if (v < new Version(5, 4, 6, 0) && v >= new Version(5, 3, 14, 0) && !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
                    {
                        // Only affects 10240 and 10586 (TH1 and TH2)
                        changedText += "\n - Sorry for that issue with the app not launching! It was an issue that only affected older versions of Windows, and only revealed itself after being compiled for the store. Welcome back to Power Planner! :)";
                    }

                    if (v < new Version(5, 3, 14, 0))
                    {
                        changedText += "\n - Export schedule to image feature added to Schedule page";
                    }

                    if (v < new Version(5, 3, 12, 0))
                    {
                        changedText += "\n - Headers on Schedule view properly zoom out now\n - Wide schedule tile now displays more info";
                    }

                    if (v < new Version(5, 3, 0, 0))
                    {
                        changedText += "\n - You can now assign times to tasks/events!\n - You can add generic tasks/events that aren't under a specific class!\n - Incomplete items now appear on schedule views\n - Setting the first day of the week in two week schedule settings now updates the weekly schedule so that it starts on the same day";
                    }

                    if (v < new Version(5, 2, 16, 0))
                    {
                        changedText += "\n - Added start/end dates to classes!";
                    }

                    if (v < new Version(5, 2, 0, 0))
                    {
                        changedText += "\n - You can now add holidays!";
                        changedText += "\n - Bug fixes for overall GPA calculation and Live Tile displaying completed exams";
                    }

                    if (v < new Version(5, 1, 28, 0))
                        changedText += "\n - Drag/drop items between dates in the large Calendar view!";

                    if (v < new Version(5, 1, 14, 0))
                        changedText += "\n - App respects your system 24hr time settings now!";

                    if (v < new Version(5, 1, 8, 0))
                        changedText += "\n - Convert your homework/exams to grades! Open a class, go to the grades page, and you'll see your \"unassigned\" homework and exams!";

                    if (v < new Version(5, 1, 0, 0))
                        changedText += "\n - Universal Dismiss (requires Anniversary Update)\n - Intelligently picks current class and next class date when adding homework/exam";

                    if (v < new Version(5, 0, 15, 0))
                        changedText += "\n - Ability to view old completed homework and exams on class page\n - When adding homework, default date automatically set to your next class";

                    if (v < new Version(5, 0, 13, 0))
                        changedText += "\n - Improved schedule editor! Open the Schedule page and click the edit button.";

                    if (v < new Version(5, 0, 12, 0))
                        changedText += "\n - Visual refresh of welcome screen/login/free version popup";

                    if (v < new Version(5, 0, 11, 0))
                        changedText += "\n - Start/end dates on semesters!";

                    if (v < new Version(5, 0, 10, 0))
                        changedText += "\n - Hola! The app has been translated to Spanish, contact me if you'd like to help translate to your language!";

                    if (v < new Version(5, 0, 5, 0))
                        changedText += "\n - Dark theme support added!";

                    if (v < new Version(5, 0, 4, 0))
                        changedText += "\n - Outlook Calendar integration added!";

                    if (v < new Version(5, 0, 3, 0))
                        changedText += "\n - Jump list actions, right click Power Planner on the taskbar and instantly add homework/exam!\n - Quick Add tile, pin it and quickly add homework/exams";

                    if (v < new Version(5, 0, 1, 0))
                        changedText += "\n - Image attachment symbol on items in list view";

                    if (v < new Version(3, 2, 0, 0))
                        changedText += "\n - App instantly syncs when you make a change on another device, even when not open!\n - Detailed lock screen status added. See your homework on your lock screen, or select the Schedule tile to see your upcoming class.\n - Settings for live tiles added";

                    if (v < new Version(3, 1, 4, 0))
                        changedText += "\n - Settings for sync options added";

                    if (v < new Version(3, 1, 2, 0))
                        changedText += "\n - Secondary tiles for classes added! Open a class and click the pin button!\n - Primary tile now displays multiple days of content on larger sizes";

                    if (v < new Version(3, 1, 1, 0))
                        changedText += "\n - Schedule tile added! Open your schedule and click the pin button!";

                    if (v < new Version(3, 1, 0, 0))
                        changedText += "\n - What If? feature for grades!";

                    if (v < new Version(3, 0, 9, 0))
                        changedText += "\n - Settings for automatic reminders added";

                    if (v < new Version(3, 0, 0, 0))
                        changedText += "\n - REWRITTEN FOR WINDOWS 10!!!\n - The entire underlying structure of Power Planner has been re-written for Windows 10";

                    if (v < new Version(2, 2, 0, 0))
                        changedText += "\n - Added percent complete slider to homework items\n - Added class color rectangles in side menu bar";

                    if (v < new Version(2, 1, 1, 0))
                        changedText += "\n - Image attachments added!";

                    if (v < new Version(2, 1, 0, 1))
                    {
                        changedText += "\n - Ability to drop grades!";
                    }

                    if (v < new Version(2, 1, 0, 0))
                    {
                        changedText += "\n - Grades added! Open a class and scroll to the right.";
                    }


                    if (changedText.Length > 0)
                        MessageBox.Show("Power Planner just installed an update. Here's what's new!\n" + changedText, "Just Updated", MessageBoxButton.OK);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        internal static async Task<bool> ConfirmDelete(string message, string title)
        {
            MessageDialog dialog = new MessageDialog(message, title);

            var commandDelete = new UICommand(LocalizedResources.GetString("MenuItemDelete"));
            var commandCancel = new UICommand(LocalizedResources.GetString("MenuItemCancel"));

            dialog.Commands.Add(commandDelete);
            dialog.Commands.Add(commandCancel);

            var response = await dialog.ShowAsync();

            if (response == commandDelete)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            //NotificationsExtensions.Toasts.ToastContent c = new NotificationsExtensions.Toasts.ToastContent()
            //{
            //    Visual = new NotificationsExtensions.Toasts.ToastVisual()
            //    {
            //        TitleText = new NotificationsExtensions.Toasts.ToastText()
            //        {
            //            Text = "Suspended"
            //        }
            //    }
            //};

            //Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(new Windows.UI.Notifications.ToastNotification(c.GetXml()));

            //TODO: Save application state and stop any background activity

            // TODO: Cancel any current syncs

            // Register the tasks
            try
            {
                // Make sure none are registered (they should have already been unregistered)
                UnregisterAllBackgroundTasks();

                RegisterInfrequentBackgroundTask();
                RegisterRawPushBackgroundTask();
            }

            catch (Exception ex)
            {
                if (UWPExceptionHelper.TrackIfRpcServerUnavailable(ex, "RegisterBgTasks"))
                {
                }
                else if (UWPExceptionHelper.TrackIfPathInvalid(ex, "RegisterBgTasks"))
                {
                }
                else
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            deferral.Complete();
        }

        private static void UnregisterAllBackgroundTasks()
        {
            foreach (var t in BackgroundTaskRegistration.AllTasks)
                t.Value.Unregister(true);
        }

        private static void RegisterRawPushBackgroundTask()
        {
            try
            {
                var builder = CreateBackgroundTaskBuilder("RawPushBackgroundTask", typeof(RawPushBackgroundTask));

                // Trigger on raw push received
                builder.SetTrigger(new PushNotificationTrigger());

                // Make sure internet available when triggered
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                builder.Register();
            }
            catch (Exception ex)
            {
                if (!UWPExceptionHelper.TrackIfNotificationsIssue(ex, nameof(RegisterRawPushBackgroundTask)))
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private static void RegisterInfrequentBackgroundTask()
        {
            var builder = CreateBackgroundTaskBuilder("InfrequentBackgroundTask", typeof(InfrequentBackgroundTask));

            // Run every 4 days
            builder.SetTrigger(new MaintenanceTrigger(5760, false));

            builder.Register();
        }

        private static BackgroundTaskBuilder CreateBackgroundTaskBuilder(string name, Type backgroundTaskClassType)
        {
            return new BackgroundTaskBuilder()
            {
                Name = name,
                TaskEntryPoint = backgroundTaskClassType.FullName
            };
        }

        /// <summary>
        /// Simply shows a popup, doesn't do any view model logic
        /// </summary>
        /// <param name="elToCenterFrom"></param>
        /// <param name="addHomeworkAction"></param>
        /// <param name="addExamAction"></param>
        public static void ShowFlyoutAddHomeworkOrExam(FrameworkElement elToCenterFrom, Action addHomeworkAction, Action addExamAction, Action addHolidayAction = null)
        {
            MenuFlyoutItem menuItemHomework = new MenuFlyoutItem()
            {
                Text = LocalizedResources.Common.GetStringTask()
            };
            menuItemHomework.Click += delegate
            {
                addHomeworkAction();
            };

            MenuFlyoutItem menuItemExam = new MenuFlyoutItem()
            {
                Text = LocalizedResources.Common.GetStringEvent()
            };
            menuItemExam.Click += delegate
            {
                addExamAction();
            };

            MenuFlyoutItem menuItemHoliday = null;
            if (addHolidayAction != null)
            {
                menuItemHoliday = new MenuFlyoutItem()
                {
                    Text = LocalizedResources.Common.GetStringHoliday()
                };
                menuItemHoliday.Click += delegate { addHolidayAction(); };
            }

            MenuFlyout f = new MenuFlyout()
            {
                Items =
                {
                    menuItemHomework,
                    menuItemExam
                }
            };

            if (menuItemHoliday != null)
            {
                f.Items.Add(menuItemHoliday);
            }

            f.ShowAt(elToCenterFrom);
        }

        public static BitmapImage GetImageThumbnail(string image)
        {
            BitmapImage bmp = ImageDownloader.NewLoadingBitmap();

            getImageThumbnail(image, bmp);

            return bmp;
        }

        private static async void getImageThumbnail(string image, BitmapImage bmp)
        {
            try
            {
                var currAccount = PowerPlannerApp.Current.GetCurrentAccount();
                if (currAccount == null)
                    return;

                IFolder imagesFolder = await FileHelper.GetOrCreateImagesFolder(currAccount.LocalAccountId);
                
                //if already exists
                try
                {
                    IFile file = await imagesFolder.GetFileAsync(image);

                    if (file != null)
                    {
                        StorageFile nativeFile = await StorageFile.GetFileFromPathAsync(file.Path);
                        if (nativeFile != null)
                        {
                            await getImageThumbnail(nativeFile, bmp);
                            return;
                        }
                    }
                }

                catch { }

                //otherwise we need to download the image
                StorageFolder nativeImagesFolder = await StorageFolder.GetFolderFromPathAsync(imagesFolder.Path);
                await ImageDownloader.GetImageAsync(nativeImagesFolder, image, Website.GetImageUrl(currAccount.AccountId, image), bmp);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static async System.Threading.Tasks.Task getImageThumbnail(StorageFile file, BitmapImage bmp)
        {
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.PicturesView);

            bmp.UriSource = null;
            await bmp.SetSourceAsync(thumbnail);
        }

        public static BitmapImage GetImage(string image)
        {
            BitmapImage bmp = ImageDownloader.NewLoadingBitmap();

            getImage(image, bmp);

            return bmp;
        }

        private static async void getImage(string image, BitmapImage bmp)
        {
            try
            {
                var currAccount = PowerPlannerApp.Current.GetCurrentAccount();
                if (currAccount == null)
                    return;

                IFolder imagesFolder = await FileHelper.GetOrCreateImagesFolder(currAccount.LocalAccountId);
                StorageFolder nativeImagesFolder = await StorageFolder.GetFolderFromPathAsync(imagesFolder.Path);

                await ImageDownloader.GetImageAsync(nativeImagesFolder, image, Website.GetImageUrl(currAccount.AccountId, image), bmp);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static bool IsMobile()
        {
            return InterfacesUWP.DeviceInfo.GetCurrentDeviceFormFactor() == DeviceFormFactor.Mobile;
        }
    }
}
