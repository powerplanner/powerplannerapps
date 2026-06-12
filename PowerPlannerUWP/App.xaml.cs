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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesUWP.AppWindows;
using PowerPlannerAppDataLibrary.Windows;
using PowerPlannerUWP.ViewModel.Settings;
using PowerPlannerUWP.ViewModel.Promos;
using PowerPlannerUWP.Views.Promos;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.Helpers;
using PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Schedule;
using Windows.ApplicationModel.DataTransfer;
using PowerPlannerAppDataLibrary.Helpers;
using Windows.System.Profile;
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

            switch (Settings.ThemeOverride)
            {
                case Themes.Light:
                    RequestedTheme = ApplicationTheme.Light;
                    break;

                case Themes.Dark:
                    RequestedTheme = ApplicationTheme.Dark;
                    break;
            }

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
                { typeof(YearsViewModel), typeof(ComponentView) }, // Don't show Years as a popup on Windows
                { typeof(ClassWhatIfViewModel), typeof(ComponentView) }, // Don't show What If as a popup on Windows
                { typeof(ShowImagesViewModel), typeof(ComponentView) }, // Don't show ShowImages as a popup on Windows (we have integrated titlebar back button)
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
                { typeof(AgendaViewModel), typeof(AgendaView) },
                { typeof(CalendarViewModel), typeof(ComponentView) },
                { typeof(ClassViewModel), typeof(ClassView) },
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
                { typeof(WelcomeViewModel), typeof(WelcomeView) },

                // Settings views
                { typeof(TileSettingsViewModel), typeof(BaseSettingsSplitView) },
                { typeof(ClassTilesViewModel), typeof(ClassTilesView) },
                { typeof(ClassTileViewModel), typeof(ClassTileView) },
                { typeof(MainTileViewModel), typeof(MainTileView) },
                { typeof(QuickAddTileViewModel), typeof(QuickAddTileView) },
                { typeof(ScheduleTileViewModel), typeof(ScheduleTileView) },
                { typeof(GoogleCalendarIntegrationViewModel), typeof(GoogleCalendarIntegrationView) }
            };
        }

        private void OnResuming(object sender, object e)
        {
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // If they have a custom theme color, show the extended splash screen
            if (Settings.CachedPrimaryThemeColor != null && args.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                Window.Current.Content = new ExtendedSplash(args.SplashScreen);
                Window.Current.Activate();
            }

            base.OnLaunched(args);
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            string currPage = TelemetryExtension.Current?.LastPageName;

            TelemetryExtension.Current?.TrackException(e.Exception, properties: currPage != null ? new Dictionary<string, string>
            {
                { "CurrPage", currPage }
            } : (Dictionary<string, string>)null);

            TelemetryExtension.Current?.SuspendingApp();
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
                        if (UWPExceptionHelper.TrackIfNotificationsIssue(ex, "RegisterBgTasks"))
                        {
                        }
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
                NativeUwpAppWindow nativeWindow = null;
                if (mainAppWindow == null)
                {
                    // This configures the view models, does NOT call Activate yet
                    nativeWindow = new NativeUwpAppWindow();
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

                if (mainAppWindow.GetViewModel().Content == null)
                {
                    await mainAppWindow.GetViewModel().HandleNormalLaunchActivation();
                }

                // Show the window content and activate the window
                nativeWindow?.DisplayWindowContent();
                Window.Current.Activate();

                // Listen to window activation changes
                Window.Current.Activated += Current_Activated;

                // Set up the default window properties
                ConfigureWindowProperties();

                // Set up the sharing support
                ConfigureDataTransferManager();

                // Display updates
                AppUpdatedHandler.DisplayWhatsNew();
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

                // Apply themed colors to XAML brush resources and title bar
                // using cached/default theme from SharedInitialization
                var colors = ThemeColorGenerator.Generate(Vx.Views.Theme.Current.ChromeColor);
                Helpers.UwpThemeColorApplier.Apply(colors);

                // Set up the min window size
                view.SetPreferredMinSize(new Size(300, 300));
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
        }

        internal static async Task<bool> ConfirmDelete(string message, string title)
        {
            MessageDialog dialog = new MessageDialog(message, title);

            var commandDelete = new UICommand(LocalizedResources.GetString("MenuItemDelete"));
            var commandCancel = new UICommand(LocalizedResources.GetString("MenuItemCancel"));

            dialog.Commands.Add(commandDelete);
            dialog.Commands.Add(commandCancel);

            var response = await dialog.ShowAsync();

            if (object.ReferenceEquals(response, commandDelete))
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
            try
            {
                TelemetryExtension.Current?.SuspendingApp();
            }
            catch { }
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
