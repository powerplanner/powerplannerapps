using BareMvvm.Core.App;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using PowerPlannerAppDataLibrary.Windows;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.App
{
    public class PowerPlannerApp : PortableApp
    {
        /// <summary>
        /// iOS sets this to true, it only has a Calendar tab entry that also has days.
        /// </summary>
        public static bool UseUnifiedCalendarDayTabItem { get; set; }

        /// <summary>
        /// iOS and Android sets this to true. iOS shows years within settings page, opened as a popup, Android shows it in a separate menu, also opened as a popup.
        /// </summary>
        public static bool DoNotShowYearsInTabItems { get; set; }

        /// <summary>
        /// Android sets this to true, it shows settings in separate menu item and opens as popup.
        /// </summary>
        public static bool DoNotShowSettingsInTabItems { get; set; }

        /// <summary>
        /// Android sets this to true
        /// </summary>
        public static bool ShowClassesAsPopups { get; set; }

        /// <summary>
        /// Android should set this to true as TimeZoneInfo operates on IANA ids. Windows should keep this false;
        /// </summary>
        public static bool UsesIanaTimeZoneIds { get; set; }

        public new static PowerPlannerApp Current
        {
            get { return PortableApp.Current as PowerPlannerApp; }
        }

        protected override System.Threading.Tasks.Task InitializeAsyncOverride()
        {
            SharedInitialization.Initialize();

            Sync.SyncQueued += Sync_SyncQueued;

            return System.Threading.Tasks.Task.FromResult(true);
        }

        public void ShowImage(ImageAttachmentViewModel image, ImageAttachmentViewModel[] allImages)
        {
            try
            {
                var mainWindow = Windows.OfType<MainAppWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    MainWindowViewModel viewModel = mainWindow.GetViewModel();
                    if (viewModel != null)
                    {
                        viewModel.ShowImage(image, allImages);
                    }
                    else
                    {
                        throw new NullReferenceException("Couldn't find MainWindowViewModel");
                    }
                }
                else
                {
                    throw new NullReferenceException("Couldn't find main window");
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool _ownsInAppPurchase;

        /// <summary>
        /// Use IsFullVersionAsync to check either whether the account is premium or the in app purchase is owned.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OwnsInAppPurchaseAsync()
        {
            if (_ownsInAppPurchase)
            {
                return true;
            }

            try
            {
                if (InAppPurchaseExtension.Current != null)
                {
                    _ownsInAppPurchase = await InAppPurchaseExtension.Current.OwnsInAppPurchaseAsync();
                    return _ownsInAppPurchase;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return false;
        }

        public async Task<bool> IsFullVersionAsync()
        {
#if DEBUG
            await System.Threading.Tasks.Task.Delay(800);
            return true;
#endif

// Ignore unreachable code warning
#pragma warning disable 0162
            try
            {
                var currAccount = GetCurrentAccount();
                if (currAccount != null)
                {
                    if (currAccount.PremiumAccountExpiresOn > DateTime.UtcNow)
                    {
                        return true;
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return await OwnsInAppPurchaseAsync();

#pragma warning restore 0162
        }

        /// <summary>
        /// Note that this WILL throw exceptions, caller should catch
        /// </summary>
        /// <param name="createViewModel"></param>
        public void ShowPopup(Func<BaseViewModel, BaseViewModel> createViewModel)
        {
            var mainWindow = Windows.OfType<MainAppWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                // For iOS, we get the currently visible view model and then find its closest popup host, so that this works
                // when there's already a popup visible (like adding semester -> repeating requires premium).
                // I think it has to do with only being able to show one modal popup controller at a time.
                // For Windows, we do NOT want that, since we want the popup shown on a separate layer, rather than in the same page stack as the current popup
                if (SyncExtensions.GetPlatform() == "iOS")
                {
                    var popupViewModelHost = mainWindow.GetViewModel()?.GetFinalContent()?.GetPopupViewModelHost();

                    if (popupViewModelHost != null)
                    {
                        popupViewModelHost.ShowPopup(createViewModel(popupViewModelHost));
                    }
                    else
                    {
                        throw new NullReferenceException("Couldn't find popup view model host");
                    }
                }

                else
                {
                    mainWindow.ViewModel.ShowPopup(createViewModel(mainWindow.ViewModel));
                }
            }
            else
            {
                throw new NullReferenceException("Couldn't find main window");
            }
        }

        public async void PromptPurchase(string contextualMessage)
        {
            try
            {
                ShowPopup((parent) => new PremiumVersionViewModel(parent, contextualMessage));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                try
                {
                    await new PortableMessageDialog("Failed to open upgrade to premium page. Your error has been reported to the developer.", "Error").ShowAsync();
                }
                catch { }
            }
        }

        private async void Sync_SyncQueued(object sender, SyncQueuedEventArgs e)
        {
            try
            {
                SyncResult result = await e.ResultTask;

                foreach (var mainWindow in Windows.OfType<MainAppWindow>())
                {
                    mainWindow.Dispatcher.Run(async delegate
                    {
                        try
                        {
                            var currAccount = mainWindow.GetCurrentAccount();

                            if (e.Account != currAccount || currAccount == null)
                                return;

                            if (result.Error != null)
                            {
                                if (result.Error.Equals(SyncResponse.INCORRECT_PASSWORD) || result.Error.Equals(SyncResponse.USERNAME_CHANGED) || result.Error.Equals(SyncResponse.DEVICE_NOT_FOUND))
                                {
                                    mainWindow.ShowPopupUpdateCredentials(currAccount, UpdateCredentialsType.Normal);
                                    return;
                                }

                                else if (result.Error.Equals(SyncResponse.DEVICE_NOT_FOUND) || result.Error.Equals(SyncResponse.NO_DEVICE))
                                {
                                    mainWindow.ShowPopupUpdateCredentials(currAccount, UpdateCredentialsType.NoDevice);
                                    return;
                                }
                            }
                            
                            //if the user bought the in-app purchase, but their account isn't a lifetime account, we'll
                            //make their account a lifetime account
                            if (!currAccount.IsLifetimePremiumAccount && await OwnsInAppPurchaseAsync())
                            {
                                var dontWait = Sync.SetAsPremiumAccount(currAccount);
                            }
                        }

                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    });
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public MainWindowViewModel GetMainWindowViewModel()
        {
            return (GetCurrentWindow() as MainAppWindow)?.GetViewModel();
        }

        public MainScreenViewModel GetMainScreenViewModel()
        {
            return (GetCurrentWindow() as MainAppWindow)?.GetMainScreenViewModel();
        }

        public AccountDataItem GetCurrentAccount()
        {
            return (GetCurrentWindow() as MainAppWindow)?.GetCurrentAccount();
        }

        public System.Threading.Tasks.Task SaveChanges(DataChanges changes)
        {
            var account = GetCurrentAccount();

            return SaveChanges(account, changes);
        }

        public async System.Threading.Tasks.Task SaveChanges(AccountDataItem account, DataChanges changes, bool waitForSaveAndSyncTasks = false)
        {
            if (account == null)
            {
                throw new NullReferenceException("account was null. Windows: " + Windows.Count);
            }

            var dataStore = await AccountDataStore.Get(account.LocalAccountId);
            var saveChangeTasks = await dataStore.ProcessLocalChanges(changes);
            System.Threading.Tasks.Task<SyncResult> syncTask = null;

            // Don't await this, we don't want it blocking
            if (account.IsOnlineAccount)
            {
                if (waitForSaveAndSyncTasks)
                {
                    syncTask = Sync.SyncAccountAsync(account);
                }
                else
                {
                    SyncWithoutBlocking(account);
                }
            }

            if (waitForSaveAndSyncTasks)
            {
                // Need to wait for the tile/toast tasks to finish
                await saveChangeTasks.WaitForAllTasksAsync();
                if (syncTask != null)
                {
                    var syncResult = await syncTask;
                    if (syncResult != null && syncResult.SaveChangesTask != null)
                    {
                        await syncResult.SaveChangesTask.WaitForAllTasksAsync();
                    }
                }
            }
        }

        private async void SyncWithoutBlocking(AccountDataItem account)
        {
            try
            {
                await Sync.SyncAccountAsync(account);
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Returns true if the class takes place on the specified date.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool DoesClassOccurOnDate(AccountDataItem account, DateTime date, ViewItemClass c)
        {
            if (c.Schedules == null || c.Schedules.Count == 0 || account == null)
            {
                return false;
            }

            var currWeek = account.GetWeekOnDifferentDate(date);

            return c.Schedules.Any(s =>
                    s.DayOfWeek == date.DayOfWeek
                    && (s.ScheduleWeek == Schedule.Week.BothWeeks || s.ScheduleWeek == currWeek));
        }

        /// <summary>
        /// Finds the next date that the class occurs on. If the class already started today, returns the
        /// NEXT instance of that class.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static DateTime? GetNextClassDate(AccountDataItem account, ViewItemClass c)
        {
            if (c.Schedules == null || c.Schedules.Count == 0)
            {
                return null;
            }

            var now = DateTime.Now;
            DateTime date = now.Date;

            // Look up to 2 weeks (and one day) in advance
            // We include the extra one day since if the class is currently going on, we want the NEXT instance of that class,
            // which could possibly be 2 weeks out
            for (int i = 0; i < 15; i++, date = date.AddDays(1))
            {
                var currWeek = account.GetWeekOnDifferentDate(date);

                // If there's a schedule on that day
                // (If it's not today, then we're good... otherwise if it's today,
                // make sure that the class hasn't started yet - if it started, we want the NEXT instance of the class)
                if (c.Schedules.Any(s =>
                    s.DayOfWeek == date.DayOfWeek
                    && (s.ScheduleWeek == Schedule.Week.BothWeeks || s.ScheduleWeek == currWeek)
                    && (date.Date != now.Date || s.StartTime.TimeOfDay > now.TimeOfDay)))
                {
                    return date;
                }
            }

            return null;
        }

        public static ViewItemClass GetFirstClassOnDay(DateTime date, AccountDataItem account, IEnumerable<ViewItemClass> classes)
        {
            if (classes == null)
                return null;

            var currWeek = account.GetWeekOnDifferentDate(date);

            var schedules = SchedulesOnDay.Get(classes, date, currWeek, trackChanges: false);

            return schedules.FirstOrDefault()?.Class;
        }

        public static ViewItemClass GetClosestClassBasedOnSchedule(DateTime now, AccountDataItem account, IEnumerable<ViewItemClass> classes)
        {
            if (classes == null)
                return null;

            var currWeek = account.GetWeekOnDifferentDate(now);

            var schedules = SchedulesOnDay.Get(classes, now, currWeek, trackChanges: false);

            ViewItemSchedule closestBefore = null;
            ViewItemSchedule closestAfter = null;

            //look through all schedules
            foreach (var s in schedules)
            {
                //if the class is currently going on
                if (now.TimeOfDay >= s.StartTime.TimeOfDay && now.TimeOfDay <= s.EndTime.TimeOfDay)
                {
                    return s.Class;
                }

                //else if the class is in the future, we instantly select it for the after class since it's sorted from earliest to latest
                else if (s.StartTime.TimeOfDay >= now.TimeOfDay)
                {
                    // Make sure it's only 10 mins after
                    if ((s.StartTime.TimeOfDay - now.TimeOfDay) < TimeSpan.FromMinutes(10))
                    {
                        closestAfter = s;
                    }

                    // Regardless we break
                    break;
                }

                else
                {
                    // Make sure it's only 10 mins before
                    if ((now.TimeOfDay - s.EndTime.TimeOfDay) < TimeSpan.FromMinutes(10))
                    {
                        closestBefore = s;
                    }
                }
            }

            if (closestAfter == null && closestBefore == null)
            {
                return null;
            }

            else if (closestAfter == null)
                return closestBefore.Class;

            else if (closestBefore == null)
                return closestAfter.Class;

            else if ((now.TimeOfDay - closestBefore.EndTime.TimeOfDay) < (closestAfter.StartTime.TimeOfDay - now.TimeOfDay))
                return closestBefore.Class;

            else
                return closestAfter.Class;
        }

        /// <summary>
        /// Used for syncing from background task (raw push notification).
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task SyncAccountInBackgroundAsync(long accountId)
        {
            try
            {
                if (accountId == 0)
                {
                    return;
                }

                // First try to grab cached
                var account = AccountsManager.GetCurrentlyLoadedAccounts().FirstOrDefault(i => i.AccountId == accountId);
                if (account == null)
                {
                    account = (await AccountsManager.GetAllAccounts()).FirstOrDefault(i => i.AccountId == accountId);
                }
                if (account == null)
                {
                    return;
                }

                var syncResult = await Sync.SyncAccountAsync(account);

                if (syncResult != null && syncResult.SaveChangesTask != null)
                {
                    await syncResult.SaveChangesTask.WaitForAllTasksAsync();
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
