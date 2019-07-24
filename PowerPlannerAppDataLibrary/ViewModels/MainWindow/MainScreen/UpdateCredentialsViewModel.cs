using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class UpdateCredentialsViewModel : BaseViewModel
    {
        public UpdateTypes UpdateType { get; private set; }
        public AccountDataItem Account { get; private set; }

        private UpdateCredentialsViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public static UpdateCredentialsViewModel Create(BaseViewModel parent, AccountDataItem account, UpdateTypes updateType)
        {
            return new MainScreen.UpdateCredentialsViewModel(parent)
            {
                UpdateType = updateType,
                Account = account,
                Username = account.Username
            };
        }

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

        private string _error = "";

        public string Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value, nameof(Error)); }
        }

        private bool _isLoggingIn;

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set { SetProperty(ref _isLoggingIn, value, nameof(IsLoggingIn)); }
        }

        public async void LogIn()
        {
            IsLoggingIn = true;

            try
            {
                string username = Username.Trim();
                string password = Password;

                if (UpdateType == UpdateTypes.NoDevice)
                {
                    var resp = await PowerPlannerAuth.AddDeviceAsync(
                        username: username,
                        password: password);

                    if (resp.Error != null)
                    {
                        SetError(resp.Error);
                        return;
                    }

                    // Set their new device ID and token
                    Account.DeviceId = resp.DeviceId;
                    Account.LocalToken = resp.LocalToken;
                    Account.Token = resp.Token;

                    await AccountsManager.Save(Account);

                    // Trigger a sync but don't await it
                    try
                    {
                        var dontWait = SyncLayer.Sync.SyncAccountAsync(Account);
                    }
                    catch { }
                }

                else
                {
                    var resp = await PowerPlannerAuth.CheckUpdatedCredentialsAsync(
                        accountId: Account.AccountId,
                        username: username,
                        password: password);

                    if (resp.Error != null)
                    {
                        SetError(resp.Error);
                        return;
                    }

                    //update their local account
                    Account.Username = username;
                    Account.LocalToken = resp.LocalToken;
                    Account.Token = resp.Token;

                    await AccountsManager.Save(Account);

                    // Trigger a sync but don't await it
                    try
                    {
                        var dontWait = SyncLayer.Sync.SyncAccountAsync(Account);
                    }
                    catch { }
                }

                GoBack();
            }

            catch
            {
                SetError("Failed to check your new credentials. Maybe you're offline?");
            }

            finally { IsLoggingIn = false; }
        }

        private void SetError(string message)
        {
            Error = message;
        }

        public enum UpdateTypes
        {
            Normal,
            NoDevice
        }

        public void ForgotUsername()
        {
            ShowPopup(new ForgotUsernameViewModel(Parent));
        }

        public void ForgotPassword()
        {
            ShowPopup(new ResetPasswordViewModel(Parent, Username));
        }
    }
}
