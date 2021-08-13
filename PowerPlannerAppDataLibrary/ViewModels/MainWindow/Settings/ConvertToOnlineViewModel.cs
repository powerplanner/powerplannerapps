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
using BareMvvm.Core;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ConvertToOnlineViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public AccountDataItem Account { get; private set; }

        public ConvertToOnlineViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;

            Title = PowerPlannerResources.GetString("Settings_ConvertToOnlinePage.Title");
        }

        protected override View Render()
        {
            bool isEnabled = !IsConverting;

            return RenderGenericPopupContent(

                new TextBox(Email)
                {
                    Header = PowerPlannerResources.GetString("Settings_ConvertToOnlinePage_TextBoxEmail.Header"),
                    PlaceholderText = PowerPlannerResources.GetString("Settings_ConvertToOnlinePage_TextBoxEmail.PlaceholderText"),
                    AutoFocus = true,
                    InputScope = InputScope.Email,
                    OnSubmit = () => _ = CreateOnlineAccountAsync(),
                    IsEnabled = isEnabled
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_ConvertToOnlinePage_ButtonConvert.Content"),
                    Margin = new Thickness(0, 24, 0, 0),
                    Click = () => _ = CreateOnlineAccountAsync(),
                    IsEnabled = isEnabled
                }

            );
        }

        [VxSubscribe]
        public TextField Email { get; private set; } = CreateAccountViewModel.GenerateEmailTextField();

        private bool _showConfirmMergeExisting;
        public bool ShowConfirmMergeExisting
        {
            get { return _showConfirmMergeExisting; }
            set { SetProperty(ref _showConfirmMergeExisting, value, nameof(ShowConfirmMergeExisting)); }
        }

        public bool IsConverting { get => GetState<bool>(); set => SetState(value); }

        private CreateAccountResponse _response;
        public async System.Threading.Tasks.Task CreateOnlineAccountAsync()
        {
            if (!ValidateAllInputs())
            {
                return;
            }

            var currAccount = Account;
            
            string email = Email.Text;

            try
            {
                IsConverting = true;

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

                    Email.SetError(_response.Error);
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

            finally
            {
                IsConverting = false;
            }

            Email.SetError(PowerPlannerResources.GetString("Settings_ConvertToOnline_Errors_FailedCreateOnline"));
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

            //have it sync, without waiting
            Sync.StartSyncAccount(account);
        }
    }
}
