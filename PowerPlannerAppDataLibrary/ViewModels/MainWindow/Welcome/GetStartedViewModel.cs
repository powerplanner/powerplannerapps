using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class GetStartedViewModel : BaseViewModel
    {
        public GetStartedViewModel(BaseViewModel parent) : base(parent) { }

        /// <summary>
        /// For returning users
        /// </summary>
        public void LogIn()
        {
            ShowPopup(new ExistingUserViewModel(this));
        }

        /// <summary>
        /// For new users, creates an offline account and logs them in
        /// </summary>
        public async Task GetStartedAsync()
        {
            try
            {
                var account = await AccountsManager.CreateAndInitializeAccountAsync(AccountsManager.DefaultOfflineAccountUsername, "", null, 0, 0);

                if (account != null)
                {
                    // Take us to the account
                    var dontWait = FindAncestor<MainWindowViewModel>().SetCurrentAccount(account);
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
    }
}
