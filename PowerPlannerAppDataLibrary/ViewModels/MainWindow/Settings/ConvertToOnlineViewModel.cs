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
using PowerPlannerAppDataLibrary.Serialization;
using PowerPlannerAppDataLibrary.SyncLayer;
using BareMvvm.Core;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using Vx.Views;
using PowerPlannerAppAuthLibrary;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public partial class ConvertToOnlineViewModel : PopupComponentViewModel
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

                new PasswordBox
                {
                    Text = VxValue.Create(Password, t => { Password = t; IncorrectPassword = false; }),
                    Header = PowerPlannerResources.GetString("LoginPage_TextBoxPassword.Header"),
                    PlaceholderText = PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_TextBoxCurrentPassword.PlaceholderText"),
                    Margin = new Thickness(0, 18, 0, 0),
                    OnSubmit = () => _ = CreateOnlineAccountAsync(),
                    IsEnabled = isEnabled,
                    ValidationState = IncorrectPassword ? InputValidationState.Invalid(PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_Errors_IncorrectPassword")) : null
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

        public TextField Email { get; private set; } = CreateAccountViewModel.GenerateEmailTextField();

        public string Password { get => GetState<string>(); set => SetState(value); }

        protected override void RegisterPropertySubscriptions()
        {
            base.RegisterPropertySubscriptions();
            Subscribe(Email);
        }

        private bool _showConfirmMergeExisting;
        public bool ShowConfirmMergeExisting
        {
            get { return _showConfirmMergeExisting; }
            set { SetProperty(ref _showConfirmMergeExisting, value, nameof(ShowConfirmMergeExisting)); }
        }

        public bool IsConverting { get => GetState<bool>(); set => SetState(value); }

        public bool IncorrectPassword { get => GetState<bool>(); set => SetState(value); }

        public async System.Threading.Tasks.Task CreateOnlineAccountAsync()
        {
            if (!ValidateAllInputs(new[] { Email }))
            {
                return;
            }

            var currAccount = Account;

            string email = Email.Text.Trim();
            string password = Password;

            try
            {

                if (!PowerPlannerAuth.ValidatePasswordLocally(Password, currAccount.Username, currAccount.LocalToken))
                {
                    IncorrectPassword = true;
                    return;
                }

                IsConverting = true;

                var resp = await PowerPlannerAuth.CreateAccountAndAddDeviceAsync(
                    username: currAccount.Username,
                    password: password,
                    email: email);

                if (resp == null)
                    ShowMessage(PowerPlannerResources.GetStringOfflineExplanation(), "Error creating account");

                if (resp.Error != null)
                {
                    TelemetryExtension.Current?.TrackEvent("Error_ConvertToOnline", new Dictionary<string, string>()
                    {
                        { "Error", resp.Error }
                    });

                    ShowMessage(resp.Error, "Error creating account");
                }

                else
                {
                    await finishConvertingToOnline(currAccount, resp.AccountId, resp.DeviceId, resp.Token);
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

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }

        private async System.Threading.Tasks.Task finishConvertingToOnline(AccountDataItem account, long accountId, int deviceId, string token)
        {
            account.AccountId = accountId;
            account.DeviceId = deviceId;
            account.Token = token;
            account.NeedsToSyncSettings = true;

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
