using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;
using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ConvertToOnlineViewModel : BaseViewModel
    {
        public AccountDataItem Account { get; private set; }

        public ConvertToOnlineViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); }
        }

        private string _error;
        public string Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value, nameof(Error)); }
        }

        private bool _showConfirmMergeExisting;
        public bool ShowConfirmMergeExisting
        {
            get { return _showConfirmMergeExisting; }
            set { SetProperty(ref _showConfirmMergeExisting, value, nameof(ShowConfirmMergeExisting)); }
        }

        private CreateAccountResponse _response;
        public async System.Threading.Tasks.Task CreateOnlineAccountAsync()
        {
            var currAccount = Account;
            
            string email = Email;

            try
            {
                _response = await WebHelper.Download<CreateAccountRequest, CreateAccountResponse>(
                    Website.URL + "createaccountmodern",
                    new CreateAccountRequest()
                    {
                        Username = currAccount.Username,
                        Password = currAccount.LocalToken,
                        Email = email,
                        AddDevice = true
                    }, Website.ApiKey);

                if (_response.Error != null)
                {
                    TelemetryExtension.Current?.TrackEvent("Error_ConvertToOnline", new Dictionary<string, string>()
                    {
                        { "Error", _response.Error }
                    });

                    Error = _response.Error;
                }

                else
                {
                    if (_response.ExistsButCredentialsMatched)
                    {
                        ShowConfirmMergeExisting = true;
                        return;
                    }

                    await finishConvertingToOnline(currAccount);
                    RemoveViewModel();
                    return;
                }
                
                return;
            }

            catch { }

            Error = PowerPlannerResources.GetString("Settings_ConvertToOnline_Errors_FailedCreateOnline");
        }

        public async System.Threading.Tasks.Task MergeExisting()
        {
            if (_response == null)
            {
                return;
            }

            await finishConvertingToOnline(Account);
            RemoveViewModel();
        }

        public void CancelMergeExisting()
        {
            _response = null;
            ShowConfirmMergeExisting = false;
        }

        private async System.Threading.Tasks.Task finishConvertingToOnline(AccountDataItem account)
        {
            account.AccountId = _response.AccountId;
            account.DeviceId = _response.DeviceId;
            //await login.Account.ConvertToOnline();

            await AccountsManager.Save(account);

            // Transfer the settings
            try
            {
                await SavedGradeScalesManager.TransferToOnlineAccountAsync(account.LocalAccountId, account.AccountId);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            //have it sync
            SyncWithoutBlocking(account);
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
    }
}
