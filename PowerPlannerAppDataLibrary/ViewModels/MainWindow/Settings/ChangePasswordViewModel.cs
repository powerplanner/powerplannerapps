using BareMvvm.Core;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangePasswordViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public event EventHandler<string> ActionError;
        public event EventHandler<string> ActionPasswordsDidNotMatch;

        public AccountDataItem Account { get; private set; }

        public ChangePasswordViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_ChangePasswordPage.Title");

            Account = account;

            Password = CreateAccountViewModel.GeneratePasswordTextField();
            ConfirmPassword = new TextField(required: true, mustMatch: Password);
        }

        protected override View Render()
        {
            bool isEnabled = !IsUpdatingPassword;

            return RenderGenericPopupContent(
                new PasswordBox
                {
                    Header = PowerPlannerResources.GetString("Settings_ChangePasswordPage_TextBoxNewPassword.Header"),
                    PlaceholderText = PowerPlannerResources.GetString("Settings_ChangePasswordPage_TextBoxNewPassword.PlaceholderText"),
                    Text = Bind<string>(nameof(Password.Text), Password),
                    ValidationState = Password.ValidationState,
                    HasFocusChanged = f => Password.HasFocus = f,
                    AutoFocus = true,
                    IsEnabled = isEnabled,
                    OnSubmit = Update
                },

                new PasswordBox
                {
                    Header = PowerPlannerResources.GetString("Settings_ChangePasswordPage_TextBoxConfirmPassword.Header"),
                    PlaceholderText = PowerPlannerResources.GetString("Settings_ChangePasswordPage_TextBoxConfirmPassword.PlaceholderText"),
                    Text = Bind<string>(nameof(ConfirmPassword.Text), ConfirmPassword),
                    ValidationState = ConfirmPassword.ValidationState,
                    HasFocusChanged = f => ConfirmPassword.HasFocus = f,
                    Margin = new Thickness(0, 18, 0, 0),
                    IsEnabled = isEnabled,
                    OnSubmit = Update
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_ChangePasswordPage_ButtonUpdatePassword.Content"),
                    Click = Update,
                    Margin = new Thickness(0, 24, 0, 0),
                    IsEnabled = isEnabled
                }
            );
        }

        [VxSubscribe]
        public TextField Password { get; private set; }

        [VxSubscribe]
        public TextField ConfirmPassword { get; private set; }

        private bool _isUpdatingPassword;

        public bool IsUpdatingPassword
        {
            get { return _isUpdatingPassword; }
            set { SetProperty(ref _isUpdatingPassword, value, nameof(IsUpdatingPassword)); }
        }
        
        public async void Update()
        {
            IsUpdatingPassword = true;

            try
            {
                if (!ValidateAllInputs())
                {
                    return;
                }

                if (Account.IsOnlineAccount)
                {
                    try
                    {
                        var resp = await PowerPlannerAuth.ChangeOnlineAccountPasswordAsync(
                            accountId: Account.AccountId,
                            username: Account.Username,
                            newPassword: Password.Text,
                            currentToken: Account.Token);

                        if (resp.Error != null)
                        {
                            SetError(resp.Error);
                            return;
                        }

                        await updateToken(Account, resp.LocalToken, resp.Token);
                        GoBack();
                    }

                    catch { SetError(PowerPlannerResources.GetString("Settings_ChangePasswordPage_Errors_FailedUpdateOnline")); }

                }

                else
                {
                    var newLocalToken = PowerPlannerAuth.ChangeOfflineAccountPassword(Account.Username, Password.Text, Account.LocalToken);
                    await updateToken(Account, newLocalToken, null);
                    GoBack();
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                SetError("Unknown error");
            }

            finally { IsUpdatingPassword = false; }
        }

        private void SetError(string error)
        {
            Password.SetError(error);
        }

        private async System.Threading.Tasks.Task updateToken(AccountDataItem account, string newLocalToken, string newToken)
        {
            account.LocalToken = newLocalToken;
            account.Token = newToken;
            await AccountsManager.Save(account);
        }
    }
}
