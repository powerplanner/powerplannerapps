using PowerPlannerApp.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PowerPlannerApp.Uwp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            Variables.VERSION = new Version(1, 0, 0);
            PowerPlannerApp.SyncLayer.SyncExtensions.GetAppName = delegate { return "Power Planner for Windows 10"; };
            PowerPlannerApp.SyncLayer.SyncExtensions.GetPlatform = delegate
            {
                if (DeviceInfo.DeviceFamily == DeviceFamily.Mobile)
                {
                    return "Windows 10 Mobile";
                }
                else
                {
                    return "Windows 10";
                }
            };

            PowerPlannerCoreApp.Initialize();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        private void OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                Xamarin.Forms.Forms.Init(args);

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (!(args is LaunchActivatedEventArgs launchArgs && launchArgs.PrelaunchActivated == true))
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage));
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }

    public static class DeviceInfo
    {
        private static DeviceFamily? _currentDeviceFamily;

        public static DeviceFamily DeviceFamily
        {
            get
            {
                if (_currentDeviceFamily == null)
                {
                    switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                    {
                        case "Windows.Mobile":
                            _currentDeviceFamily = DeviceFamily.Mobile;
                            break;

                        case "Windows.Desktop":
                            _currentDeviceFamily = DeviceFamily.Desktop;
                            break;

                        default:
                            _currentDeviceFamily = DeviceFamily.Unknown;
                            break;
                    }
                }

                return _currentDeviceFamily.Value;
            }
        }

        public static DeviceFormFactor GetCurrentDeviceFormFactor()
        {
            if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch)
            {
                if (DeviceFamily == DeviceFamily.Mobile)
                    return DeviceFormFactor.Mobile;

                return DeviceFormFactor.Tablet;
            }

            else
            {
                return DeviceFormFactor.Desktop;
            }
        }
    }

    public enum DeviceFamily
    {
        Desktop,
        Mobile,
        Unknown
    }

    public enum DeviceFormFactor
    {
        Mobile,
        Tablet,
        Desktop
    }
}
