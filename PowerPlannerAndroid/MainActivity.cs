using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Linq;
using PowerPlannerAndroid.App;
using PowerPlannerAppDataLibrary.Windows;
using InterfacesDroid.Activities;
using InterfacesDroid.Windows;
using System.Threading.Tasks;
using PowerPlannerAndroid.Extensions;
using Microsoft.QueryStringDotNET;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using Plugin.Settings;
using PowerPlannerAppDataLibrary;
using ToolsPortable;
using System.IO;
using Android.Webkit;
using Android.Content.PM;
using AndroidX.Core.View;
using Vx.Droid.Helpers;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlannerAndroid
{
    [Activity(Label =
#if DEBUG
        "PP-Dev"
#else
        "Power Planner"
#endif
        , Name =
        #if DEBUG
        "com.barebonesdev.powerplanner.dev.MainActivity"
#else
        "com.barebonesdev.powerplanner.MainActivity"
#endif
        ,
        MainLauncher = true,
        Icon = "@mipmap/icon",
        LaunchMode = LaunchMode.SingleInstance,
        WindowSoftInputMode = SoftInput.AdjustResize,
        // ConfigurationChanges enables my app to handle orientation changes without re-rendering the views
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.KeyboardHidden | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : BareActivity
    {
        public static MainActivity GetCurrent()
        {
            return (PowerPlannerAndroid.App.PowerPlannerDroidApp.Current.GetCurrentWindow()?.NativeAppWindow as NativeDroidAppWindow)?.Activity as MainActivity;
        }

        private MainAppWindow _mainAppWindow;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Enable bleeding full screen into status and navigation bar
            WindowCompat.SetDecorFitsSystemWindows(Window, false);

            // Initialize Vx (have to initialize here rather than in App so it picks up the right context with the themed context)
            Vx.Droid.VxDroidExtensions.ApplicationContext = this;

#if DEBUG
            //int disposedCount = 0;
            //WeakEventHandler.ObjectDisposedAction = delegate
            //{
            //    Toast.MakeText(Application.Context, disposedCount++ + " ObjectDisposed", ToastLength.Short).Show();
            //};
#endif

            TelemetryExtension.Current = new DroidTelemetryExtension();

#if DEBUG
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = new MyUncaughtExceptionHandler();
#endif

            // Color the status bar since the theme color provided in themes.xml seemed to stop taking effect in Android 8.0
            // Min API is 23 (Marshmallow), so Lollipop (API 21) APIs are always available
            // Status bar color is now handled by drawing proper background behind the status bar insets
            // (via StatusBarSpacer views in layouts) rather than using the deprecated SetStatusBarColor API.
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                this.Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
            }

            // Register the window
            _mainAppWindow = new MainAppWindow();
            await PowerPlannerDroidApp.Current.RegisterWindowAsync(_mainAppWindow, new NativeDroidAppWindow(this));

            HandleIntent();

            AppUpdatedHandler.HandleAppVersionUpdated();
        }

#if DEBUG
        private class MyUncaughtExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
        {
            public void UncaughtException(Java.Lang.Thread t, Java.Lang.Throwable e)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
#endif

        public const int PickImageId = 1000;
        public TaskCompletionSource<PickImageResult> PickImageTaskCompletionSource { get; set; }

        public class PickImageResult : IDisposable
        {
            public Stream Stream { get; set; }

            /// <summary>
            /// The file extension. Does NOT include the period.
            /// </summary>
            public string Extension { get; set; }

            public void Dispose()
            {
                Stream.Dispose();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (requestCode == PickImageId)
            {
                if (PickImageTaskCompletionSource == null)
                {
                    TelemetryExtension.Current?.TrackException(new InvalidOperationException("PickImageTaskCompletionSource was null, app must have dehydrated while picking images"));
                    return;
                }

                PickImageResult result;

                try
                {
                    if ((resultCode == Result.Ok) && (intent != null))
                    {
                        Android.Net.Uri uri = intent.Data;
                        Stream stream = ContentResolver.OpenInputStream(uri);
                        string extension = MimeTypeMap.Singleton.GetExtensionFromMimeType(ContentResolver.GetType(uri));

                        result = new PickImageResult()
                        {
                            Stream = stream,
                            Extension = extension
                        };
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        PickImageTaskCompletionSource.SetException(ex);
                    }
                    catch
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                    return;
                }

                // Set the result as the completion of the Task
                try
                {
                    PickImageTaskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            // http://stackoverflow.com/questions/23446120/onnewintent-is-not-called
            base.OnNewIntent(intent);
            Intent = intent;
            HandleIntent();

            AppUpdatedHandler.HandleAppVersionUpdated();
        }

        private async void HandleIntent()
        {
            try
            {
                if (Intent.Action == Intent.ActionView && Intent.DataString != null && Intent.DataString.StartsWith("powerplanner:?"))
                {
                    var args = ArgumentsHelper.Parse(Intent.DataString.Substring("powerplanner:?".Length));

                    LaunchSurface launchContext = LaunchSurface.Normal;
                    if (args != null)
                    {
                        launchContext = args.LaunchSurface;
                    }

                    var viewModel = _mainAppWindow.GetViewModel();

                    if (args is ViewPageArguments)
                    {
                        var viewPageArgs = args as ViewPageArguments;
                        TrackLaunch(args, launchContext, "View" + viewPageArgs.Page);
                        switch (viewPageArgs.Page)
                        {
                            // View agenda
                            case ViewPageArguments.Pages.Agenda:
                                await viewModel.HandleViewAgendaActivation(viewPageArgs.LocalAccountId);
                                return;
                        }
                    }

                    else if (args is ViewScheduleArguments)
                    {
                        var viewScheduleArgs = args as ViewScheduleArguments;
                        TrackLaunch(args, launchContext, "ViewSchedule");
                        await viewModel.HandleViewScheduleActivation(viewScheduleArgs.LocalAccountId);
                        return;
                    }

                    else if (args is ViewClassArguments)
                    {
                        var viewClassArgs = args as ViewClassArguments;
                        TrackLaunch(args, launchContext, "ViewClass");
                        await viewModel.HandleViewClassActivation(viewClassArgs.LocalAccountId, viewClassArgs.ItemId);
                        return;
                    }

                    else if (args is ViewHolidayArguments)
                    {
                        var viewHolidayArgs = args as ViewHolidayArguments;
                        TrackLaunch(args, launchContext, "ViewHoliday");
                        await viewModel.HandleViewHolidayActivation(viewHolidayArgs.LocalAccountId, viewHolidayArgs.ItemId);
                        return;
                    }

                    else if (args is ViewTaskArguments)
                    {
                        TrackLaunch(args, launchContext, "ViewTask");
                        var viewTaskArgs = args as ViewTaskArguments;
                        await viewModel.HandleViewTaskActivation(viewTaskArgs.LocalAccountId, viewTaskArgs.ItemId);
                        return;
                    }

                    else if (args is ViewEventArguments)
                    {
                        TrackLaunch(args, launchContext, "ViewEvent");
                        var viewEventArgs = args as ViewEventArguments;
                        await viewModel.HandleViewEventActivation(viewEventArgs.LocalAccountId, viewEventArgs.ItemId);
                        return;
                    }

                    else if (args is QuickAddTaskToCurrentAccountArguments)
                    {
                        if (launchContext == LaunchSurface.Normal)
                        {
                            launchContext = LaunchSurface.JumpList;
                        }

                        TrackLaunch(args, launchContext, "QuickAddTask");
                        await viewModel.HandleQuickAddTask();
                        return;
                    }

                    else if (args is QuickAddEventToCurrentAccountArguments)
                    {
                        if (launchContext == LaunchSurface.Normal)
                        {
                            launchContext = LaunchSurface.JumpList;
                        }

                        TrackLaunch(args, launchContext, "QuickAddEvent");
                        await viewModel.HandleQuickAddEvent();
                        return;
                    }

                    else if (args is QuickAddToCurrentAccountArguments)
                    {
                        TrackLaunch(args, launchContext, "QuickAdd");
                        await viewModel.HandleQuickAddActivation(Guid.Empty);
                        return;
                    }

                    else if (args is QuickAddArguments)
                    {
                        TrackLaunch(args, launchContext, "QuickAdd");
                        var quickAddArgs = args as QuickAddArguments;
                        await viewModel.HandleQuickAddActivation(quickAddArgs.LocalAccountId);
                        return;
                    }

                    // Otherwise still track launch if from special surface
                    TrackLaunch(args, launchContext, "Launch");
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            // For all other cases (like ActionMain), fallback action is launch as main
            await _mainAppWindow.GetViewModel().HandleNormalLaunchActivation();
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

        protected override void OnStop()
        {
            _mainAppWindow?.GetViewModel()?.HandleBeingLeft();
            base.OnStop();
        }

        protected override void OnRestart()
        {
            _mainAppWindow?.GetViewModel()?.HandleBeingReturnedTo();

            base.OnRestart();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var dontWait = PowerPlannerDroidApp.Current.UnregisterWindowAsync(_mainAppWindow);
        }

        public override void OnLowMemory()
        {
            GC.Collect();
            base.OnLowMemory();
        }
    }
}
