using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Tasks;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow
{
    public class MainWindowViewModel : PagedViewModelWithPopups
    {
        private static bool _hasInitializedAppShortcuts;
        public static event EventHandler<AccountDataItem> LoggedInFromNormalActivation;

        public AccountDataItem CurrentAccount { get; private set; }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, "IsEnabled"); }
        }

        public MainWindowViewModel(BaseViewModel parent) : base(parent)
        {
            if (!_hasInitializedAppShortcuts)
            {
                _hasInitializedAppShortcuts = true;
                InitializeAppShortcuts();
            }

            AccountsManager.OnAccountDeleted += new WeakEventHandler<Guid>(AccountsManager_OnAccountDeleted).Handler;

            PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private async void InitializeAppShortcuts()
        {
            try
            {
                if (AppShortcutsExtension.Current != null)
                {
                    await Task.Run(async delegate
                    {
                        await AppShortcutsExtension.Current.UpdateAsync(showAddItemShortcuts: true);
                    });
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void MainWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FinalContent):
                    string pageName = FinalContent.GetPageName();
                    PageViewTelemetryHelper.TrackPageVisited(pageName);
                    break;
            }
        }

        public void ShowImage(ImageAttachmentViewModel image, ImageAttachmentViewModel[] allImages)
        {
            Navigate(new ShowImagesViewModel(this, image, allImages));
        }

        private void AccountsManager_OnAccountDeleted(object sender, Guid localAccountId)
        {
            if (CurrentAccount != null && CurrentAccount.LocalAccountId == localAccountId)
            {
                Dispatcher.Run(delegate
                {
                    var dontWait = SetCurrentAccount(null);
                });
            }
        }

        /// <summary>
        /// Assumes already on UI thread.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task SetCurrentAccount(AccountDataItem account, bool syncAccount = true)
        {
            base.Popups.Clear();

            CurrentAccount = account;

            if (account == null)
            {
                base.ClearBackStack();
                base.Replace(new WelcomeViewModel(this));
            }

            else
            {
                try
                {
                    IsEnabled = false;
                    base.ClearBackStack();

                    if (account.NeedsInitialSync)
                    {
                        base.Replace(new InitialSyncViewModel(this, account));
                    }
                    else
                    {
                        base.Replace(await MainScreenViewModel.LoadAsync(this, account, syncAccount: syncAccount));
                    }
                }

                finally
                {
                    IsEnabled = true;
                }

                try
                {
                    // Reset reminders upon login
                    RemindersExtension.Current?.ClearCurrentReminders(account.LocalAccountId);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        public void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsType updateType)
        {
            // TODO
        }

        public async Task HandleNormalLaunchActivation()
        {
            // Restore previous login
            AccountDataItem lastAccount = await AccountsManager.GetLastLogin();

            if (lastAccount == null && (await AccountsManager.GetAllAccounts()).Count == 0)
            {
                // If no accounts, we create the default account
                try
                {
                    var account = await AccountsManager.CreateAndInitializeAccountAsync(AccountsManager.DefaultOfflineAccountUsername, "", null, 0, 0);

                    if (account != null)
                    {
                        lastAccount = account;
                        TelemetryExtension.Current?.TrackEvent("CreatedDefaultOfflineAccount");
                    }
                    else
                    {
                        TelemetryExtension.Current?.TrackException(new Exception("Tried creating default offline account, but it returned null"));
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            if (lastAccount != null && lastAccount.IsAutoLoginPossible && lastAccount.AutoLogin)
            {
                await this.SetCurrentAccount(lastAccount);
                PromoRegistrations.StartPromoLogic(lastAccount);
                LoggedInFromNormalActivation?.Invoke(this, lastAccount);
            }

            else
                await this.SetCurrentAccount(null);
        }

        public Task HandleViewAgendaActivation(Guid localAccountId)
        {
            return HandleSelectMenuItemActivation(localAccountId, NavigationManager.MainMenuSelections.Agenda);
        }

        public Task HandleViewDayActivation(Guid localAccountId, DateTime date)
        {
            if (CurrentAccount != null && CurrentAccount.LocalAccountId == localAccountId)
            {
                var mainScreen = GetMainScreenViewModel();
                if (mainScreen != null)
                {
                    var dayViewModel = (mainScreen.Content as TasksViewModel)?.Content as DayViewModel;
                    if (dayViewModel != null)
                    {
                        Popups.Clear();
                        mainScreen.Popups.Clear();
                        dayViewModel.CurrentDate = date;
                        return Task.FromResult(true);
                    }
                }
            }

            NavigationManager.SetSelectedDate(date);
            NavigationManager.SetDisplayMonth(date);
            return HandleSelectMenuItemActivation(localAccountId, NavigationManager.MainMenuSelections.Day);
        }

        public Task HandleViewScheduleActivation(Guid localAccountId)
        {
            return HandleSelectMenuItemActivation(localAccountId, NavigationManager.MainMenuSelections.Schedule);
        }

        private async Task HandleSelectMenuItemActivation(Guid localAccountId, NavigationManager.MainMenuSelections menuItem)
        {
            if (CurrentAccount != null && (localAccountId == Guid.Empty || CurrentAccount.LocalAccountId == localAccountId))
            {
                var mainScreen = GetMainScreenViewModel();
                if (mainScreen != null)
                {
                    Popups.Clear();
                    mainScreen.Popups.Clear();
                    if (mainScreen.AvailableItems.Contains(menuItem))
                    {
                        mainScreen.SelectedItem = menuItem;
                    }
                }
            }

            else
            {
                if (localAccountId != Guid.Empty)
                {
                    AccountsManager.SetLastLoginIdentifier(localAccountId);
                }
                NavigationManager.MainMenuSelection = menuItem;
                await HandleNormalLaunchActivation();
            }
        }

        public async Task HandleViewClassActivation(Guid localAccountId, Guid classId)
        {
            if (CurrentAccount != null && CurrentAccount.LocalAccountId == localAccountId)
            {
                var mainScreen = GetMainScreenViewModel();
                if (mainScreen != null)
                {
                    Popups.Clear();
                    mainScreen.Popups.Clear();
                    var dontWait = mainScreen.SelectClass(classId);
                }
            }

            else
            {
                AccountsManager.SetLastLoginIdentifier(localAccountId);
                NavigationManager.MainMenuSelection = NavigationManager.MainMenuSelections.Classes;
                NavigationManager.ClassSelection = classId;
                await HandleNormalLaunchActivation();
            }
        }

        public async Task HandleViewHomeworkActivation(Guid localAccountId, Guid homeworkId)
        {
            if (CurrentAccount == null || CurrentAccount.LocalAccountId != localAccountId)
            {
                AccountsManager.SetLastLoginIdentifier(localAccountId);
                await HandleNormalLaunchActivation();
            }
            
            var mainScreen = GetMainScreenViewModel();
            if (mainScreen != null && mainScreen.CurrentAccount != null)
            {
                Popups.Clear();
                var singleHomework = await SingleHomeworkViewItemsGroup.LoadAsync(localAccountId, homeworkId);
                if (singleHomework != null && singleHomework.Item != null)
                {
                    mainScreen.ShowItem(singleHomework.Item);

                    // Remove all but latest popup
                    // Have to do this AFTER showing the popup since on iOS if we first clear and then show immediately, iOS freaks out
                    while (mainScreen.Popups.Count > 1)
                    {
                        mainScreen.Popups.RemoveAt(0);
                    }
                }
            }
        }

        public async Task HandleViewExamActivation(Guid localAccountId, Guid examId)
        {
            if (CurrentAccount == null || CurrentAccount.LocalAccountId != localAccountId)
            {
                AccountsManager.SetLastLoginIdentifier(localAccountId);
                await HandleNormalLaunchActivation();
            }

            var mainScreen = GetMainScreenViewModel();
            if (mainScreen != null && mainScreen.CurrentAccount != null)
            {
                Popups.Clear();
                var singleExam = await SingleExamViewItemsGroup.LoadAsync(localAccountId, examId);
                if (singleExam != null && singleExam.Item != null)
                {
                    mainScreen.ShowItem(singleExam.Item);

                    // Remove all but latest popup
                    // Have to do this AFTER showing the popup since on iOS if we first clear and then show immediately, iOS freaks out
                    while (mainScreen.Popups.Count > 1)
                    {
                        mainScreen.Popups.RemoveAt(0);
                    }
                }
            }
        }

        public async Task HandleViewHolidayActivation(Guid localAccountId, Guid holidayId)
        {
            DataItemMegaItem holiday = await Task.Run(async delegate
            {
                using (await Locks.LockDataForReadAsync("HandleViewHolidayActivation"))
                {
                    var dataStore = await AccountDataStore.Get(localAccountId);
                    if (dataStore == null)
                    {
                        return null;
                    }

                    return dataStore.TableMegaItems.FirstOrDefault(i =>
                        i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday
                        && i.Identifier == holidayId);
                }
            });

            if (holiday != null)
            {
                var holidayDate = DateTime.SpecifyKind(holiday.Date, DateTimeKind.Local);
                var desiredDisplayMonth = holidayDate;
                var mainScreen = GetMainScreenViewModel();
                if (mainScreen != null && mainScreen.CurrentAccount != null && mainScreen.CurrentAccount.LocalAccountId == localAccountId && mainScreen.Content is CalendarViewModel)
                {
                    (mainScreen.Content as CalendarViewModel).DisplayMonth = desiredDisplayMonth;
                    (mainScreen.Content as CalendarViewModel).SelectedDate = holidayDate;
                }
                else
                {
                    NavigationManager.SetDisplayMonth(desiredDisplayMonth);
                    NavigationManager.SetSelectedDate(holidayDate);
                    await HandleSelectMenuItemActivation(localAccountId, NavigationManager.MainMenuSelections.Calendar);
                }
            }
        }

        public async Task HandleQuickAddActivation(Guid localAccountId)
        {
            // If there's no current account, we need to first initialize that
            if (CurrentAccount == null)
            {
                // If we have a specific account, use it
                if (localAccountId != Guid.Empty)
                {
                    AccountsManager.SetLastLoginIdentifier(localAccountId);
                }

                await HandleNormalLaunchActivation();
            }

            // Else if we're logged into the wrong account
            else if (localAccountId != Guid.Empty && CurrentAccount.LocalAccountId != localAccountId)
            {
                AccountsManager.SetLastLoginIdentifier(localAccountId);
                await HandleNormalLaunchActivation();
            }

            // If there's still no account, stop
            if (CurrentAccount == null)
            {
                return;
            }

            var mainScreen = GetMainScreenViewModel();
            if (mainScreen != null && mainScreen.CurrentAccount != null && mainScreen.Classes != null && mainScreen.Classes.Count > 0)
            {
                Popups.Clear();
                mainScreen.Popups.Clear();
                mainScreen.ShowPopup(new QuickAddViewModel(mainScreen));
            }
        }

        public Task HandleQuickAddHomework()
        {
            return HandleQuickAddItem(AddHomeworkViewModel.ItemType.Homework);
        }

        public Task HandleQuickAddExam()
        {
            return HandleQuickAddItem(AddHomeworkViewModel.ItemType.Exam);
        }

        private async Task HandleQuickAddItem(AddHomeworkViewModel.ItemType type)
        {
            if (CurrentAccount == null)
            {
                await HandleNormalLaunchActivation();
            }

            var mainScreen = GetMainScreenViewModel();
            if (mainScreen != null && mainScreen.CurrentAccount != null && mainScreen.Classes != null && mainScreen.Classes.Count > 0)
            {
                Popups.Clear();
                var newModel = AddHomeworkViewModel.CreateForAdd(mainScreen, new AddHomeworkViewModel.AddParameter()
                {
                    Classes = mainScreen.Classes.ToArray(),
                    DueDate = DateTime.Today,
                    Type = type
                });

                // For iOS, we can't clear and then add, we need to replace
                if (mainScreen.Popups.Count >= 1)
                {
                    mainScreen.Popups[0] = newModel;
                    while (mainScreen.Popups.Count > 1)
                    {
                        mainScreen.Popups.RemoveAt(1);
                    }
                }
                else
                {
                    mainScreen.ShowPopup(newModel);
                }
            }
        }

        private DateTime _timeLeftAt = DateTime.Now;
        public void HandleBeingLeft()
        {
            _timeLeftAt = DateTime.Now;
            PageViewTelemetryHelper.TrackLeavingApp();
        }

        public async Task HandleBeingReturnedTo()
        {
            // Update the time left at just like when we're left, since on iOS in certain cases
            // returned to gets called when being left didn't get called, so it permanently thinks
            // we left the app a long time ago otherwise
            var timeLeftAt = _timeLeftAt;
            _timeLeftAt = DateTime.Now;

            // If the day has changed (went from yesterday to today), reset the entire view model
            if (timeLeftAt.Date != DateTime.Now.Date || AccountDataStore.RetrieveAndResetWasUpdatedByBackgroundTask())
            {
                await HandleNormalLaunchActivation();
                return;
            }

            // If it's been gone for more than a minute, do a sync
            if (DateTime.Now.AddMinutes(-1) > timeLeftAt)
            {
                try
                {
                    if (CurrentAccount != null)
                    {
                        try
                        {
                            var dontWait = SyncLayer.Sync.SyncAccountAsync(CurrentAccount);
                        }
                        catch { }
                    }
                }

                catch { }
            }

            try
            {
                if (RemindersExtension.Current != null && CurrentAccount != null)
                {
                    var dontWait = RemindersExtension.Current.ClearCurrentReminders(CurrentAccount.LocalAccountId);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            PageViewTelemetryHelper.TrackReturningToApp();
        }

        public MainScreenViewModel GetMainScreenViewModel()
        {
            return Content as MainScreenViewModel;
        }
    }
}
