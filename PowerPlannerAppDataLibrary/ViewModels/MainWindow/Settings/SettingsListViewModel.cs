using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsListViewModel : BaseViewModel
    {
        public DataLayer.AccountDataItem Account { get; private set; }

        public const string HelpUrl = "https://powerplanner.freshdesk.com/support/home";

        /// <summary>
        /// Android sets this to true to have subpages appear as popups
        /// </summary>
        public bool ShowAsPopups { get; set; }

        public SettingsListViewModel(BaseViewModel parent) : base(parent)
        {
            MainScreenViewModel = FindAncestor<MainScreenViewModel>();
            Account = MainScreenViewModel?.CurrentAccount;
            HasAccount = Account != null;
            if (HasAccount)
            {
                Account.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Account_PropertyChanged).Handler;
                UpdateFromAccount();
            }
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Account.IsOnlineAccount):
                case nameof(Account.IsDefaultOfflineAccount):
                    UpdateFromAccount();
                    break;
            }
        }

        private void UpdateFromAccount()
        {
            IsOnlineAccount = Account.IsOnlineAccount;
            IsDefaultOfflineAccount = Account.IsDefaultOfflineAccount;
        }

        public MainScreenViewModel MainScreenViewModel { get; private set; }

        /// <summary>
        /// This is only false if they enter the settings page from the login page. This will still be true for default offline accounts.
        /// </summary>
        public bool HasAccount { get; private set; }

        private bool _isOnlineAccount;
        public bool IsOnlineAccount
        {
            get => _isOnlineAccount;
            private set => SetProperty(ref _isOnlineAccount, value, nameof(IsOnlineAccount));
        }

        private bool _isDefaultOfflineAccount;
        public bool IsDefaultOfflineAccount
        {
            get => _isDefaultOfflineAccount;
            private set => SetProperty(ref _isDefaultOfflineAccount, value, nameof(IsDefaultOfflineAccount));
        }

        public bool IsCreateAccountVisible => CachedComputation(delegate
        {
            return IsDefaultOfflineAccount;
        }, new string[] { nameof(IsDefaultOfflineAccount) });

        public bool IsLogInVisible => CachedComputation(delegate
        {
            return IsDefaultOfflineAccount;
        }, new string[] { nameof(IsDefaultOfflineAccount) });

        public bool IsMyAccountVisible => CachedComputation(delegate
        {
            // We hide when it's the default account, only options for them are create or log in
            return HasAccount && !IsDefaultOfflineAccount;
        }, new string[] { nameof(IsDefaultOfflineAccount) });

        /// <summary>
        /// Should be visible for default offline account too, clicking it will tell users they need to create an account first
        /// </summary>
        public bool IsGoogleCalendarIntegrationVisible => CachedComputation(delegate
        {
            return IsOnlineAccount || IsDefaultOfflineAccount;
        }, new string[] { nameof(IsOnlineAccount), nameof(IsDefaultOfflineAccount) });

        public bool IsRemindersVisible => HasAccount;

        public bool IsSyncOptionsVisible => CachedComputation(delegate
        {
            return IsOnlineAccount;
        }, new string[] { nameof(IsOnlineAccount) });

        public bool IsTwoWeekScheduleVisible => HasAccount;

        public bool IsSchoolTimeZoneVisible => HasAccount;

        public bool IsViewYearsAndSemestersVisible => HasAccount && MainScreenViewModel != null;

        private bool _initializedCurrentSemesterName;
        private string _currentSemesterName;
        public string CurrentSemesterName
        {
            private set => SetProperty(ref _currentSemesterName, value, nameof(CurrentSemesterName));
            get
            {
                if (!_initializedCurrentSemesterName)
                {
                    _initializedCurrentSemesterName = true;

                    if (MainScreenViewModel != null && MainScreenViewModel.CurrentSemester != null)
                    {
                        MainScreenViewModel.CurrentSemester.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(CurrentSemester_PropertyChanged).Handler;
                    }

                    _currentSemesterName = MainScreenViewModel?.CurrentSemester?.Name;
                }

                return _currentSemesterName;
            }
        }

        public string CurrentSemesterText => CachedComputation(delegate
        {
            return PowerPlannerResources.GetStringWithParameters("String_CurrentSemester", CurrentSemesterName);
        }, new string[] { nameof(CurrentSemesterName) });

        private void CurrentSemester_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewItemSemester.Name))
            {
                try
                {
                    CurrentSemesterName = MainScreenViewModel.CurrentSemester?.Name;
                }
                catch { }
            }
        }

        private bool _initializedSyncStatus;
        private string _syncStatusText;
        public string SyncStatusText
        {
            get
            {
                if (!_initializedSyncStatus)
                {
                    _initializedSyncStatus = true;

                    if (MainScreenViewModel != null)
                    {
                        MainScreenViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(MainScreenViewModel_PropertyChanged).Handler;
                    }

                    UpdateSyncStatus();
                }

                return _syncStatusText;
            }
            set => SetProperty(ref _syncStatusText, value, nameof(SyncStatusText));
        }

        private string _syncButtonText;
        public string SyncButtonText
        {
            get => _syncButtonText;
            set => SetProperty(ref _syncButtonText, value, nameof(SyncButtonText));
        }

        private bool _syncButtonIsEnabled;
        public bool SyncButtonIsEnabled
        {
            get => _syncButtonIsEnabled;
            set => SetProperty(ref _syncButtonIsEnabled, value, nameof(SyncButtonIsEnabled));
        }

        private bool _syncHasError;
        public bool SyncHasError
        {
            get => _syncHasError;
            set => SetProperty(ref _syncHasError, value, nameof(SyncHasError));
        }

        private void UpdateSyncStatus()
        {
            var account = MainScreenViewModel?.CurrentAccount;

            if (account == null)
            {
                SyncStatusText = PowerPlannerResources.GetString("String_NoAccountToSync");
                SyncButtonText = PowerPlannerResources.GetString("String_SyncNow");
                SyncButtonIsEnabled = false;
            }

            if (MainScreenViewModel.SyncState == MainScreenViewModel.SyncStates.Done)
            {
                SyncButtonText = PowerPlannerResources.GetString("String_SyncNow");
                SyncButtonIsEnabled = true;
            }
            else
            {
                SyncButtonText = PowerPlannerResources.GetString("String_Syncing");
                SyncButtonIsEnabled = false;
            }

            if (MainScreenViewModel.HasSyncErrors)
            {
                SyncHasError = true;
                SyncStatusText = PowerPlannerResources.GetString("String_SyncError");
            }
            else if (MainScreenViewModel.IsOffline)
            {
                SyncHasError = false;

                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SyncStatusText = PowerPlannerResources.GetStringWithParameters("String_OfflineLastSync", FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SyncStatusText = PowerPlannerResources.GetString("String_OfflineCouldntSync");
                }
            }
            else
            {
                SyncHasError = false;

                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SyncStatusText = PowerPlannerResources.GetStringWithParameters("String_LastSync", FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SyncStatusText = PowerPlannerResources.GetString("String_SyncNeeded");
                }
            }
        }

        private static string FriendlyLastSyncTime(DateTime time)
        {
            if (time.Date == DateTime.Today)
            {
                return time.ToString("t");
            }
            else
            {
                return time.ToString("d");
            }
        }

        private void MainScreenViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainScreenViewModel.SyncState):
                case nameof(MainScreenViewModel.HasSyncErrors):
                case nameof(MainScreenViewModel.IsOffline):
                    UpdateSyncStatus();
                    break;
            }
        }

        public void OpenYears()
        {
            MainScreenViewModel?.OpenYears();
        }

        public void StartSync()
        {
            MainScreenViewModel?.SyncCurrentAccount();
        }

        public void OpenMyAccount()
        {
            Show(MyAccountViewModel.Load(ParentForSubviews));
        }

        public void OpenAbout()
        {
            Show(new AboutViewModel(ParentForSubviews));
        }

        public void OpenPremiumVersion()
        {
            PowerPlannerApp.Current.PromptPurchase(null);
        }

        public void OpenReminderSettings()
        {
            Show(new ReminderSettingsViewModel(ParentForSubviews));
        }

        public void OpenSyncOptions()
        {
            Show(new SyncOptionsViewModel(ParentForSubviews));
        }

        /// <summary>
        /// If UI app doesn't want to use the split model view model, then use this approach
        /// </summary>
        public void OpenSyncOptionsSimple()
        {
            Show(new SyncOptionsSimpleViewModel(ParentForSubviews));
        }

        public void OpenCalendarIntegration()
        {
            Show(new CalendarIntegrationViewModel(ParentForSubviews));
        }

        public void OpenTwoWeekScheduleSettings()
        {
            Show(new TwoWeekScheduleSettingsViewModel(ParentForSubviews));
        }

        public void OpenGoogleCalendarIntegration()
        {
            TelemetryExtension.Current?.TrackEvent("Action_OpenGoogleCalendarIntegration");

            try
            {
                if (AlertIfGoogleCalendarIntegrationNotPossible())
                {
                    return;
                }
                else
                {
                    MainScreenViewModel.ShowPopup(new GoogleCalendarIntegrationViewModel(MainScreenViewModel, Account));
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public bool AlertIfGoogleCalendarIntegrationNotPossible()
        {
            if (IsDefaultOfflineAccount)
            {
                new PortableMessageDialog(PowerPlannerResources.GetString("Settings_GoogleCalendar_NoAccountMessage"), PowerPlannerResources.GetString("String_NoAccount")).Show();
                return true;
            }

            return false;
        }

        private const string ContributeUrl = "https://powerplanner.net/contribute";

        /// <summary>
        /// Returns the url, caller must navigate to the url
        /// </summary>
        /// <returns></returns>
        public string OpenContribute()
        {
            TelemetryExtension.Current?.TrackEvent("Action_OpenContribute");

            return ContributeUrl;
        }

        public void OpenCreateAccount()
        {
            try
            {
                ShowPopup(CreateAccountViewModel.CreateForUpgradingDefaultAccount(this, Account));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void OpenLogIn()
        {
            try
            {
                ShowPopup(new LoginViewModel(this)
                {
                    Message = PowerPlannerResources.GetString("Settings_LogInFromDefaultAccountMessage"),
                    DefaultAccountToDelete = Account.IsDefaultOfflineAccount ? Account : null // It should always be the default account, but just in case
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void OpenSchoolTimeZone()
        {
            Show(new SchoolTimeZoneSettingsViewModel(ParentForSubviews));
        }

        public void OpenLanguageSettings()
        {
            Show(new LanguageSettingsViewModel(ParentForSubviews));
        }

        public void ViewSyncErrors()
        {
            if (MainScreenViewModel != null && MainScreenViewModel.HasSyncErrors)
            {
                MainScreenViewModel.ViewSyncErrors();
            }
        }

        private BaseViewModel ParentForSubviews => GetParentForSubviews(this);

        public static BaseViewModel GetParentForSubviews(BaseViewModel thisViewModel)
        {
            if (PowerPlannerApp.ShowSettingsPagesAsPopups)
            {
                return thisViewModel.FindAncestor<PagedViewModelWithPopups>();
            }
            else
            {
                return thisViewModel.FindAncestor<PagedViewModel>();
            }
        }

        public static void Show(BaseViewModel viewModel)
        {
            if (PowerPlannerApp.ShowSettingsPagesAsPopups)
            {
                viewModel.Parent.ShowPopup(viewModel);
            }
            else
            {
                viewModel.Parent.FindAncestorOrSelf<PagedViewModel>()?.Navigate(viewModel);
            }
        }
    }
}
