using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsListViewModel : BaseViewModel
    {
        private PagedViewModel _pagedViewModel;
        public DataLayer.AccountDataItem Account { get; private set; }

        public SettingsListViewModel(BaseViewModel parent) : base(parent)
        {
            Account = FindAncestor<MainScreenViewModel>()?.CurrentAccount;
            HasAccount = Account != null;
            if (HasAccount)
            {
                Account.PropertyChanged += Account_PropertyChanged;
                UpdateFromAccount();
            }
            _pagedViewModel = FindAncestor<PagedViewModel>();
        }

        private void Account_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
            return IsOnlineAccount;
        }, new string[] { nameof(IsOnlineAccount) });

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

        public void OpenMyAccount()
        {
            _pagedViewModel.Navigate(MyAccountViewModel.Load(_pagedViewModel));
        }

        public void OpenAbout()
        {
            _pagedViewModel.Navigate(new AboutViewModel(_pagedViewModel));
        }

        public void OpenPremiumVersion()
        {
            PowerPlannerApp.Current.PromptPurchase(null);
        }

        public void OpenReminderSettings()
        {
            _pagedViewModel.Navigate(new ReminderSettingsViewModel(_pagedViewModel));
        }

        public void OpenSyncOptions()
        {
            _pagedViewModel.Navigate(new SyncOptionsViewModel(_pagedViewModel));
        }

        /// <summary>
        /// If UI app doesn't want to use the split model view model, then use this approach
        /// </summary>
        public void OpenSyncOptionsSimple()
        {
            _pagedViewModel.Navigate(new SyncOptionsSimpleViewModel(_pagedViewModel));
        }

        public void OpenCalendarIntegration()
        {
            _pagedViewModel.Navigate(new CalendarIntegrationViewModel(_pagedViewModel));
        }

        public void OpenTwoWeekScheduleSettings()
        {
            _pagedViewModel.Navigate(new TwoWeekScheduleSettingsViewModel(_pagedViewModel));
        }

        public void OpenGoogleCalendarIntegration()
        {
            TelemetryExtension.Current?.TrackEvent("Action_OpenGoogleCalendarIntegration");

            if (IsDefaultOfflineAccount)
            {
                new PortableMessageDialog(PowerPlannerResources.GetString("Settings_GoogleCalendar_NoAccountMessage"), PowerPlannerResources.GetString("String_NoAccount")).Show();
            }
            else
            {
                var popupHost = GetPopupViewModelHost();
                popupHost.ShowPopup(new GoogleCalendarIntegrationViewModel(popupHost, Account));
            }
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
            ShowPopup(CreateAccountViewModel.CreateForUpgradingDefaultAccount(this, Account));
        }

        public void OpenLogIn()
        {
            ShowPopup(new LoginViewModel(this)
            {
                Message = PowerPlannerResources.GetString("Settings_LogInFromDefaultAccountMessage"),
                DefaultAccountToDelete = Account.IsDefaultOfflineAccount ? Account : null // It should always be the default account, but just in case
            });
        }
    }
}
