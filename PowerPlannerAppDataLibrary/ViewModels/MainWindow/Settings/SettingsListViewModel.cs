using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                IsOnlineAccount = Account.IsOnlineAccount;
            }
            _pagedViewModel = FindAncestor<PagedViewModel>();
        }

        public bool HasAccount { get; private set; }

        public bool IsOnlineAccount { get; private set; }

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

            var popupHost = GetPopupViewModelHost();
            popupHost.ShowPopup(new GoogleCalendarIntegrationViewModel(popupHost, Account));
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
    }
}
