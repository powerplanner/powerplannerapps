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
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.IO;
using Android.Webkit;
using AndroidX.Core.Content;
using Android.Content.PM;

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
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.KeyboardHidden)]
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

#if DEBUG
            int disposedCount = 0;
            WeakEventHandler.ObjectDisposedAction = delegate
            {
                Toast.MakeText(Application.Context, disposedCount++ + " ObjectDisposed", ToastLength.Short).Show();
            };
#endif

            AppCenter.Start(Secrets.AppCenterAppSecret, typeof(Analytics), typeof(Crashes));
            TelemetryExtension.Current = new DroidTelemetryExtension();

#if DEBUG
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = new MyUncaughtExceptionHandler();
#endif

            // Color the status bar since the theme color provided in themes.xml seemed to stop taking effect in Android 8.0
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                this.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                this.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                this.Window.SetStatusBarColor(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.primaryDark)));
            }

            // Register the window
            _mainAppWindow = new MainAppWindow();
            await PowerPlannerDroidApp.Current.RegisterWindowAsync(_mainAppWindow, new NativeDroidAppWindow(this));

            HandleIntent();

            HandleVersionChange();
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

            HandleVersionChange();
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

        private static bool _alreadyHandledVersionChange;
        private void HandleVersionChange()
        {
            if (_alreadyHandledVersionChange)
            {
                // We'll make sure not to do this multiple times in single app lifecycle
                return;
            }

            _alreadyHandledVersionChange = true;

            const string VERSION = "Version";

            try
            {
                var versionString = CrossSettings.Current.GetValueOrDefault(VERSION, null);

                Version v;
                if (versionString != null && Version.TryParse(versionString, out v))
                {
                }
                else
                {
                    // If they have an account, that implies they were on version 5.3.16.0 which was the
                    // last version before I started storing the version number
                    if (AccountsManager.GetLastLoginLocalId() != Guid.Empty)
                    {
                        v = new Version(5, 3, 16, 0);
                    }
                    else
                    {
                        v = new Version(0, 0, 0, 0);
                    }
                }

                // Assign new version number
                if (v < Variables.VERSION)
                {
                    CrossSettings.Current.AddOrUpdateValue(VERSION, Variables.VERSION.ToString());
                }

                // If was an existing user and things have changed
                if (v.Major > 0 && v < Variables.VERSION)
                {
                    string changedText = "";


                    if (v <= new Version(2009, 25, 1, 0) && Android.Icu.Text.DecimalFormatSymbols.Instance.DecimalSeparator == ',')
                    {
                        changedText += "\n - Fixed decimal entry support for , as decimal!";
                    }

                    if (v <= new Version(2009, 23, 1, 0))
                    {
                        changedText += "\n - Fix for displaying calendar+day on shorter devices";
                    }

                    if (v <= new Version(2009, 17, 5, 0))
                    {
                        changedText += "\n - New combined calendar/day view, and Years/Settings moved to \"More\" tab on bottom right";
                        changedText += "\n - Bug fix for list items sometimes not displaying correctly";
                    }

                    if (v <= new Version(2008, 12, 2, 0))
                    {
                        changedText += "\n - URLs in class schedule room fields are now clickable for supporting Zoom/online links!";
                    }

                    if (v <= new Version(2007, 30, 1, 99))
                    {
                        changedText += "\n - Fixed order of tasks so incomplete tasks are displayed first";
                    }

                    if (v <= new Version(2005, 30, 3, 99))
                    {
                        changedText += "\n - Fixes for certain time zone scenarios";
                    }

                    if (v <= new Version(2005, 22, 1, 99))
                    {
                        changedText += "\n - Add a grade immediately after completing a task!";
                    }

                    if (v <= new Version(2005, 19, 1, 0))
                    {
                        changedText += "\n - You can now convert tasks to events and vice versa!";
                    }

                    if (v <= new Version(2004, 26, 2, 0))
                    {
                        changedText += "\n - Dark theme support added! Power Planner will now use a dark theme based on your theme selected in Android. Even the widgets will be dark!";
                    }

                    if (v <= new Version(2003, 14, 3, 0))
                    {
                        changedText += "\n - Classes that don't have any grades yet will no longer count towards overall GPA";
                    }

                    if (v <= new Version(2002, 9, 1, 0))
                    {
                        changedText += "\n - Time zone support! If you're traveling, go to the settings page to set your school's time zone.";
                    }

                    if (v <= new Version(2001, 9, 1, 0))
                    {
                        changedText += "\n - Email addresses and phone numbers are now clickable in details text!";
                    }

                    if (v < new Version(1912, 23, 0))
                    {
                        changedText += "\n - App shortcuts added to launcher icon!";
                    }

                    if (v <= new Version(1911, 5, 2, 0))
                    {
                        changedText += "\n - Updated UI! Bottom navigation bar for quicker access to the most used pages.";
                    }

                    if (v < new Version(1911, 2, 0, 0))
                    {
                        changedText += "\n - Support for Monday as first day of week on Calendar for countries like Spain!";
                    }

                    if (v < new Version(5, 4, 86, 0))
                    {
                        changedText += "\n - 2x improvement in app launch speed!";
                    }

                    if (v < new Version(5, 4, 78, 0))
                    {
                        changedText += "\n - Image attachments on tasks/events!";
                        changedText += "\n - Strikethrough on completed items";
                    }

                    if (v < new Version(5, 4, 72, 0))
                    {
                        changedText += "\n - Fixed GPA calculation handling of failed classes";
                    }

                    if (v < new Version(5, 4, 68, 0))
                    {
                        changedText += "\n - Pass/fail classes are now supported in the GPA grade system!\n - Custom selected due times are remembered for faster task entry";
                    }

                    if (v < new Version(5, 4, 60, 0))
                    {
                        changedText += "\n - Custom color picker for class colors!";
                        changedText += "\n - 24-hour time formatting fixed on schedule view";
                    }

                    if (v < new Version(5, 4, 36, 0))
                    {
                        changedText += "\n - Repeating bulk entry of tasks/events!";
                    }

                    if (v < new Version(5, 4, 32, 0))
                    {
                        changedText += "\n - Google Calendar integration! Go to the Settings page to try it out (requires an online account)";
                    }

                    if (v < new Version(5, 4, 26, 0) && v >= new Version(5, 4, 22, 0))
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                        {
                            // I broke the app icon in 5.4.22.0 for people not running Oreo
                            changedText += "\n - Fixed missing app icon";
                        }
                    }

                    if (v < new Version(5, 4, 24, 0))
                    {
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            changedText += "\n - The Android app now syncs in the background when you make a change on another device!";
                        }
                    }

                    if (v < new Version(5, 4, 22, 0))
                    {
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            changedText += "\n - Adaptive icon added!";
                        }
                    }

                    if (v < new Version(5, 4, 20, 0) && v == new Version(5, 4, 18, 0))
                    {
                        // These only get shown to users who were running 5.4.18.0 since that's where I introduced those bugs
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                        {
                            changedText += "\n - Fixed crashing when trying to add items";
                        }
                        else
                        {
                            changedText += "\n - Fixed crashes when opening add grade page";
                        }
                    }

                    if (v < new Version(5, 4, 18, 0))
                    {
                        changedText += "\n - New Schedule widget!";
                        changedText += "\n - Keyboard now auto-appears";
                    }

                    if (v < new Version(5, 4, 16, 0) && Build.VERSION.SdkInt >= BuildVersionCodes.O && v >= new Version(5, 4, 12, 0))
                    {
                        // Only show this to Oreo users who were running the app targeted to Oreo (5.4.12.0)
                        changedText += "\n - Fixed reminders not working on Android 8.0 Oreo devices";
                    }

                    if (v < new Version(5, 4, 14, 0))
                    {
                        changedText += "\n - Pinch-to-zoom on Schedule page!";
                    }

                    if (v < new Version(5, 4, 12, 0))
                    {
                        changedText += "\n - Disable Autofill on everything except username/password related fields";
                    }

                    if (v < new Version(5, 4, 4, 0))
                    {
                        changedText += "\n - Fixed issues with automatic reminders (day-before and day-of reminders)";
                    }

                    if (v < new Version(5, 4, 0, 0))
                    {
                        changedText += "\n - Widget added for Agenda!";
                    }

                    if (changedText.Length > 0)
                    {
                        var dontWait = new PortableMessageDialog("Power Planner just installed an update!\n\nHere's what's new...\n" + changedText, "Just updated").ShowAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static Guid GetLocalAccountId(QueryString qs)
        {
            return Guid.Parse(qs["localAccountId"]);
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

        /// <summary>
        /// Will never throw.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OwnsInAppPurchase()
        {
            MyInAppBillingAssistant billingAssistant = null;
            try
            {
                billingAssistant = new MyInAppBillingAssistant(this);
                return await new MyInAppBillingAssistant(this).OwnsInAppPurchase();
            }

            catch
            {
                return false;
            }

            finally
            {
                if (billingAssistant != null)
                    billingAssistant.Disconnect();
            }
        }

        /// <summary>
        /// Throws if failed to prompt.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PromptPurchase()
        {
            MyInAppBillingAssistant billingAssistant = null;
            try
            {
                billingAssistant = new MyInAppBillingAssistant(this);
                return await new MyInAppBillingAssistant(this).PromptPurchase();
            }

            finally
            {
                if (billingAssistant != null)
                    billingAssistant.Disconnect();
            }
        }
    }
}