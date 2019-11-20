using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public event EventHandler<string> ActionError;
        public event EventHandler<string> ActionPasswordsDidNotMatch;

        public AccountDataItem Account { get; private set; }

        public ChangePasswordViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
        }

        private string _password = "";

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, nameof(Password)); }
        }

        private string _confirmPassword = "";

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetProperty(ref _confirmPassword, value, nameof(ConfirmPassword)); }
        }

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
                if (string.IsNullOrWhiteSpace(Password))
                {
                    SetError(PowerPlannerResources.GetString("Settings_ChangePasswordPage_Errors_MustEnterPassword"));
                    return;
                }

                if (Password.Length < 5)
                {
                    SetError(PowerPlannerResources.GetString("String_InvalidPasswordTooShortExplanation"));
                    return;
                }

                if (!Password.Equals(ConfirmPassword))
                {
                    ActionPasswordsDidNotMatch?.Invoke(this, PowerPlannerResources.GetString("Settings_ChangePasswordPage_Errors_PasswordsDidNotMatch"));
                    return;
                }

                if (Account.IsOnlineAccount)
                {
                    try
                    {
                        var resp = await PowerPlannerAuth.ChangeOnlineAccountPasswordAsync(
                            accountId: Account.AccountId,
                            username: Account.Username,
                            newPassword: Password,
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
                    var newLocalToken = PowerPlannerAuth.ChangeOfflineAccountPassword(Account.Username, Password, Account.LocalToken);
                    await updateToken(Account, newLocalToken, null);
                    GoBack();
                }
            }

            finally { IsUpdatingPassword = false; }
        }

        private void SetError(string error)
        {
            ActionError?.Invoke(this, error);
        }

        private async System.Threading.Tasks.Task updateToken(AccountDataItem account, string newLocalToken, string newToken)
        {
            account.LocalToken = newLocalToken;
            account.Token = newToken;
            await AccountsManager.Save(account);
        }
    }
}
