using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.App;
using ToolsPortable;
using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppAuthLibrary;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount
{
    public class CreateAccountViewModel : BaseViewModel
    {
        public Action AlertPasswordTooShort = delegate { ShowMessage("Your password is too short.", "Password too short"); };
        public Action AlertConfirmationPasswordDidNotMatch = delegate { ShowMessage("Your confirmation password didn't match.", "Invalid password"); };
        public Action AlertNoUsername = delegate { ShowMessage("You must provide a username!", "No username"); };
        public Action AlertNoEmail = delegate { ShowMessage("You must provide an email!", "No email"); };

        public CreateAccountViewModel(BaseViewModel parent) : base(parent) { }

        private string _username = "";
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, nameof(Username)); }
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

        private string _email = "";
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); }
        }

        private bool isPasswordOkay()
        {
            if (Password.Length < 5)
            {
                AlertPasswordTooShort?.Invoke();
                return false;
            }

            if (!ConfirmPassword.Equals(Password))
            {
                AlertConfirmationPasswordDidNotMatch?.Invoke();
                return false;
            }

            return true;
        }

        private bool isUsernameOkay()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                AlertNoUsername?.Invoke();
                return false;
            }

            return true;
        }

        private bool isOkayToCreateLocal()
        {
            if (!isUsernameOkay())
                return false;

            if (!isPasswordOkay())
                return false;

            return true;
        }

        private bool isOkayToCreate()
        {
            if (!isOkayToCreateLocal())
                return false;

            if (string.IsNullOrWhiteSpace(Email))
            {
                AlertNoEmail?.Invoke();
                return false;
            }

            return true;
        }


        public async void CreateLocalAccount()
        {
            if (!isOkayToCreateLocal())
                return;

            string localToken = PowerPlannerAuth.CreateOfflineAccount(Username, Password);

            await FinishCreateAccount(Username, localToken, null, 0, 0);
        }

        private bool _isCreatingOnlineAccount;
        public bool IsCreatingOnlineAccount
        {
            get { return _isCreatingOnlineAccount; }
            set { SetProperty(ref _isCreatingOnlineAccount, value, nameof(IsCreatingOnlineAccount)); }
        }

        public async void CreateAccount()
        {
            if (!isOkayToCreate())
                return;

            string username = Username.Trim();
            string password = Password;
            string email = Email.Trim();

            IsCreatingOnlineAccount = true;

            try
            {
                var resp = await PowerPlannerAuth.CreateAccountAndAddDeviceAsync(
                    username: username,
                    password: password,
                    email: email);

                if (resp == null)
                    ShowMessage(PowerPlannerResources.GetStringOfflineExplanation(), "Error creating account");

                else if (resp.Error != null)
                    ShowMessage(resp.Error, "Error creating account");

                else
                {
                    await FinishCreateAccount(username, resp.LocalToken, resp.Token, resp.AccountId, resp.DeviceId);
                }
            }

            catch
            {
                ShowMessage(PowerPlannerResources.GetStringOfflineExplanation(), "Error creating account");
            }

            finally
            {
                IsCreatingOnlineAccount = false;
            }
        }

        private async System.Threading.Tasks.Task FinishCreateAccount(string username, string localToken, string token, long accountId, int deviceId)
        {
            var account = await AccountsManager.CreateAndInitializeAccountAsync(username, localToken, token, accountId, deviceId);

            if (account != null)
            {
                // Take us to the account
                var dontWait = FindAncestor<MainWindowViewModel>().SetCurrentAccount(account);
            }
        }

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }
    }
}
