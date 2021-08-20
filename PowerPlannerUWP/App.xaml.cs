using InterfacesUWP;
using PowerPlannerAppDataLibrary;
using PowerPlannerSending;
using PowerPlannerUWP.Views;
using PowerPlannerUWP.Views.SettingsViews;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToolsUniversal;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using PowerPlannerUWP.Extensions;
using StorageEverywhere;
using InterfacesUWP.App;
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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
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
using PowerPlannerUWP.Helpers;
using PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Schedule;
using Windows.ApplicationModel.DataTransfer;
using PowerPlannerAppDataLibrary.Helpers;
using Windows.System.Profile;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos;
using PowerPlannerUWP.BackgroundTasks;
using PowerPlannerAppDataLibrary.ViewModels;

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

        public override Dictionary<Type, Type> GetGenericViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>
            {
                { typeof(PopupComponentViewModel), typeof(PopupComponentView) },
                { typeof(ComponentViewModel), typeof(ComponentView) } // Popup must be first since Popup is a subclass
            };
        }

        public override Dictionary<Type, Type> GetViewModelToViewMappings()
        {
            return new Dictionary<Type, Type>()
            {
                // Main views
                { typeof(InitialSyncViewModel), typeof(InitialSyncView) },
                { typeof(AddClassViewModel), typeof(AddClassView) },
                { typeof(AddGradeViewModel), typeof(AddGradeView) },
                { typeof(AddHolidayViewModel), typeof(AddHolidayView) },
                { typeof(AddTaskOrEventViewModel), typeof(AddTaskOrEventView) },
                { typeof(AddSemesterViewModel), typeof(AddSemesterView) },
                { typeof(AddYearViewModel), typeof(AddYearView) },
                { typeof(AgendaViewModel), typeof(AgendaView) },
                { typeof(CalendarViewModel), typeof(CalendarMainView) },
                { typeof(ClassesViewModel), typeof(ClassesView) },
                { typeof(ClassViewModel), typeof(ClassView) },
                { typeof(ClassWhatIfViewModel), typeof(ClassWhatIfView) },
                { typeof(EditClassDetailsViewModel), typeof(EditClassDetailsView) },
                { typeof(LoginViewModel), typeof(LoginView) },
                { typeof(DayViewModel), typeof(MainContentDayView) },
                { typeof(MainScreenViewModel), typeof(MainScreenView) },
                { typeof(PremiumVersionViewModel), typeof(PremiumVersionView) },
                { typeof(PromoOtherPlatformsViewModel), typeof(PromoOtherPlatformsView) },
                { typeof(QuickAddViewModel), typeof(QuickAddView) },
                { typeof(ScheduleViewModel), typeof(ScheduleView) },
                { typeof(ExportSchedulePopupViewModel), typeof(ExportSchedulePopupView) },
                { typeof(SyncErrorsViewModel), typeof(SyncErrorsView) },
                { typeof(ViewGradeViewModel), typeof(ViewGradeView) },
                { typeof(ViewTaskOrEventViewModel), typeof(ViewTaskOrEventView) },
                { typeof(ShowImagesViewModel), typeof(ShowImagesView) },
                { typeof(WelcomeViewModel), typeof(WelcomeView) },

                // Settings views
                { typeof(TileSettingsViewModel), typeof(BaseSettingsSplitView) },
                { typeof(SyncOptionsViewModel), typeof(BaseSettingsSplitView) },
                { typeof(CalendarIntegrationViewModel), typeof(BaseSettingsSplitView) },
                { typeof(CalendarIntegrationClassesViewModel), typeof(CalendarIntegrationClassesView) },
                { typeof(CalendarIntegrationTasksViewModel), typeof(CalendarIntegrationTasksView) },
                { typeof(ClassTilesViewModel), typeof(ClassTilesView) },
                { typeof(ClassTileViewModel), typeof(ClassTileView) },
                { typeof(ImageUploadOptionsViewModel), typeof(ImageUploadOptionsView) },
                { typeof(MainTileViewModel), typeof(MainTileView) },
                { typeof(PushSettingsViewModel), typeof(PushSettingsView) },
                { typeof(QuickAddTileViewModel), typeof(QuickAddTileView) },
                { typeof(ScheduleTileViewModel), typeof(ScheduleTileView) },
                { typeof(GoogleCalendarIntegrationViewModel), typeof(GoogleCalendarIntegrationView) },
                { typeof(PromoContributeViewModel), typeof(PromoContributeView) },
                { typeof(LanguageSettingsViewModel), typeof(LanguageSettingsView) }
            };
        }

        private void OnResuming(object sender, object e)
        {
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            TelemetryExtension.Current?.TrackException(e.Exception);
        }

        private bool _registeredBackgroundTasks = false;
        protected override async System.Threading.Tasks.Task OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            try
            {
#if DEBUG
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    this.DebugSettings.EnableFrameRateCounter = true;
                //}
#endif

                // Register background tasks
                if (!_registeredBackgroundTasks)
                {
                    try
                    {
                        // Make sure none are registered (they should have already been unregistered)
                        UnregisterAllBackgroundTasks();

                        RegisterInfrequentBackgroundTask();
                        RegisterRawPushBackgroundTask();
                        RegisterToastBackgroundTask();
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

                    _registeredBackgroundTasks = true;
                }

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
                            case ItemType.Schedule:
                                finalArgs = new ViewScheduleArguments()
                                {
                                    LocalAccountId = data.LocalAccountId
                                }.SerializeToString();
                                break;

                            case ItemType.MegaItem:
                                finalArgs = new ViewTaskArguments()
                                {
                                    LocalAccountId = data.LocalAccountId,
                                    ItemId = data.Identifier
                                }.SerializeToString();
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

                else if (args is ViewTaskArguments)
                {
                    TrackLaunch(args, launchContext, "Task");
                    var viewTaskArgs = args as ViewTaskArguments;
                    await viewModel.HandleViewTaskActivation(viewTaskArgs.LocalAccountId, viewTaskArgs.ItemId);
                }

                else if (args is ViewEventArguments)
                {
                    TrackLaunch(args, launchContext, "Event");
                    var viewEventArgs = args as ViewEventArguments;
                    await viewModel.HandleViewEventActivation(viewEventArgs.LocalAccountId, viewEventArgs.ItemId);
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

                else if (args is QuickAddTaskToCurrentAccountArguments)
                {
                    // Jump list was created before we included the launch surface, so we'll manually port it
                    if (launchContext == LaunchSurface.Normal)
                    {
                        launchContext = LaunchSurface.JumpList;
                    }
                    TrackLaunch(args, launchContext, "QuickAddTask");
                    await viewModel.HandleQuickAddTask();
                }

                else if (args is QuickAddEventToCurrentAccountArguments)
                {
                    // Jump list was created before we included the launch surface, so we'll manually port it
                    if (launchContext == LaunchSurface.Normal)
                    {
                        launchContext = LaunchSurface.JumpList;
                    }
                    TrackLaunch(args, launchContext, "QuickAddEvent");
                    await viewModel.HandleQuickAddEvent();
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

                        var addTaskItem = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(new QuickAddTaskToCurrentAccountArguments().SerializeToString(), "New task");
                        addTaskItem.Logo = new Uri("ms-appx:///Assets/JumpList/Add.png");
                        jumpList.Items.Add(addTaskItem);

                        var addEventItem = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(new QuickAddEventToCurrentAccountArguments().SerializeToString(), "New event");
                        addEventItem.Logo = new Uri("ms-appx:///Assets/JumpList/Add.png");
                        jumpList.Items.Add(addEventItem);

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

                    if (v <= new Version(2108, 16, 2, 99))
                    {
                        changedText += "\n - Ability to override final grade/GPA on classes";
                    }

                    if (v < new Version(2105, 26, 1, 99))
                    {
                        changedText += "\n - Class reminders will respect semester end dates (so you won't get reminders after your semester is over)";
                    }

                    if (v >= new Version(2105, 19, 3, 0) && v <= new Version(2105, 19, 3, 99))
                    {
                        changedText += "\n - Fixed viewing tasks/events not working";
                    }

                    if (v <= new Version(2105, 17, 1, 0))
                    {
                        changedText += "\n - Default grade scales! Go to the settings page to configure default grade options for all classes!";
                    }

                    if (v <= new Version(2102, 16, 3, 99))
                    {
                        changedText += "\n - Right click on tasks/events to quickly mark complete, delete, or edit!";
                    }

                    if (v <= new Version(2011, 25, 4, 99) && v >= new Version(2011, 25, 4, 0))
                    {
                        // I temporarily broke these views for one build when introducing collapsable groups
                        changedText += "\n - Fixed \"+\" buttons on Day views";
                    }

                    if (v <= new Version(2011, 16, 1, 99))
                    {
                        changedText += "\n - Collapsable groups in Agenda (so you can collapse overdue items, etc)!";
                    }

                    if (v <= new Version(2008, 12, 2, 99))
                    {
                        changedText += "\n - Reminders for class schedule!";
                        changedText += "\n - URLs in class schedule room fields are now clickable for supporting Zoom/online links!";
                    }

                    if (v <= new Version(2007, 30, 1, 99))
                    {
                        changedText += "\n - Fixed order of tasks so incomplete tasks are displayed first";
                        changedText += "\n - Performance improvements for accounts with lots of items!";
                    }

                    if (v <= new Version(2005, 30, 3, 99))
                    {
                        changedText += "\n - Fixes for certain time zone scenarios";
                    }

                    if (v <= new Version(2005, 27, 1, 99))
                    {
                        changedText += "\n - Add a grade immediately after completing a task!";
                    }

                    if (v <= new Version(2005, 3, 1, 99))
                    {
                        changedText += "\n - You can now convert tasks to events and vice versa!";
                    }

                    if (v <= new Version(2005, 1, 2, 99))
                    {
                        changedText += "\n - Language setting added to allow overriding system selected language!";
                        changedText += "\n - Fixed hyperlink detection bug with upper case characters";
                    }

                    if (v <= new Version(2004, 30, 1, 99))
                    {
                        if (v > new Version(2004, 26, 1, 99))
                        {
                            changedText += "\n - Back by popular demand, you can now toggle showing past complete items on the calendar view! Use the filter button in the top right.";
                        }
                        else
                        {
                            changedText += "\n - Past complete items hidden from calendar by default. You can now toggle showing past complete items by using the filter button in the top right.";
                        }
                    }

                    if (v <= new Version(2004, 24, 2, 99))
                    {
                        changedText += "\n - Classes that don't have any grades yet will no longer count towards overall GPA";
                    }

                    if (v <= new Version(2003, 6, 1, 0))
                    {
                        changedText += "\n - Time zone support! If you're traveling, go to the settings page to set your school's time zone.";
                    }

                    if (v <= new Version(2002, 9, 1, 0))
                    {
                        changedText += "\n - UI fixes for tablet devices";
                    }

                    if (v <= new Version(2002, 3, 1, 0))
                    {
                        // Switched 100% to new time picker, need to show message to the other 50% that are just getting it now
                        if (Vx.Uwp.Controls.TimePickers.TextBasedTimePicker.IsSupported && !AbTestHelper.Tests.NewTimePicker)
                        {
                            changedText += "\n - New text-based time pickers!";
                        }
                    }

                    if (v <= new Version(2001, 26, 2, 0))
                    {
                        if (Vx.Uwp.Controls.TimePickers.TextBasedTimePicker.IsSupported && AbTestHelper.Tests.NewTimePicker)
                        {
                            changedText += "\n - New text-based time pickers!";
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
                var builder = CreateBackgroundTaskBuilder("RawPushBackgroundTask");

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
            var builder = CreateBackgroundTaskBuilder("InfrequentBackgroundTask");

            // Run every 4 days
            builder.SetTrigger(new MaintenanceTrigger(5760, false));

            builder.Register();
        }

        private static void RegisterToastBackgroundTask()
        {
            var builder = CreateBackgroundTaskBuilder("ToastBackgroundTask");

            builder.SetTrigger(new ToastNotificationActionTrigger());

            builder.Register();
        }

        private static BackgroundTaskBuilder CreateBackgroundTaskBuilder(string name)
        {
            return new BackgroundTaskBuilder()
            {
                Name = name
            };
        }

        /// <summary>
        /// Simply shows a popup, doesn't do any view model logic
        /// </summary>
        /// <param name="elToCenterFrom"></param>
        /// <param name="addTaskAction"></param>
        /// <param name="addEventAction"></param>
        public static void ShowFlyoutAddTaskOrEvent(FrameworkElement elToCenterFrom, Action addTaskAction, Action addEventAction, Action addHolidayAction = null)
        {
            MenuFlyoutItem menuItemTask = new MenuFlyoutItem()
            {
                Text = LocalizedResources.Common.GetStringTask()
            };
            menuItemTask.Click += delegate
            {
                addTaskAction();
            };

            MenuFlyoutItem menuItemEvent = new MenuFlyoutItem()
            {
                Text = LocalizedResources.Common.GetStringEvent()
            };
            menuItemEvent.Click += delegate
            {
                addEventAction();
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
                    menuItemTask,
                    menuItemEvent
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

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            if (args.TaskInstance.Task.Name == "InfrequentBackgroundTask")
            {
                new InfrequentBackgroundTask().Handle(args);
            }

            else if (args.TaskInstance.Task.Name == "RawPushBackgroundTask")
            {
                new RawPushBackgroundTask().Handle(args);
            }

            else if (args.TaskInstance.Task.Name == "ToastBackgroundTask")
            {
                new ToastBackgroundTask().Handle(args);
            }
        }
    }
}
