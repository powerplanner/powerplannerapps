using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppAuthLibrary;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class LoginViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public LoginViewModel(BaseViewModel parent) : base(parent)
        {
            Initialize();
        }

        private const string STORED_PASS = "Stored password, hidden for security";

        /// <summary>
        /// A message to be displayed at the top, used to inform user that default offline account will be wiped
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The default offline account that should be deleted after successful login
        /// </summary>
        public AccountDataItem DefaultAccountToDelete { get; set; }

        public MyObservableList<AccountDataItem> Accounts
        {
            get { return GetValue<MyObservableList<AccountDataItem>>(); }
            private set { SetValue(value); }
        }

        public MyObservableList<AccountDataItem> AccountsWithRememberUsername
        {
            get { return GetValue<MyObservableList<AccountDataItem>>(); }
            private set { SetValue(value); }
        }
        
        public string Username
        {
            get { return GetValueOrDefault<string>(""); }
            set
            {
                if (!object.Equals(Username, value))
                {
                    SetValue(value);
                    OnUsernameChanged();
                }
            }
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set
            {
                if (!object.Equals(_password, value))
                {
                    SetProperty(ref _password, value, nameof(Password));
                    OnPasswordChanged();
                }
            }
        }

        public bool IsCheckingOnlinePassword
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsLoggingInOnline
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        private WeakReference<Action<string>> _alertUserUpgradeAccountNeeded;
        public Action<string> AlertUserUpgradeAccountNeeded
        {
            get { _alertUserUpgradeAccountNeeded.TryGetTarget(out Action<string> act); return act; }
            set { _alertUserUpgradeAccountNeeded = new WeakReference<Action<string>>(value); }
        }

        private async void Initialize()
        {
            Accounts = new MyObservableList<AccountDataItem>(await AccountsManager.GetAllAccounts());

            // TODO: What if RememberUsername is edited? That's quite a minor case, not worth building something for, but it's potentially a flaw
            AccountsWithRememberUsername = Accounts.Sublist(i => i.RememberUsername && !i.IsDefaultOfflineAccount);

            AccountsManager.OnAccountDeleted += new WeakEventHandler<Guid>(AccountsManager_OnAccountDeleted).Handler;
            AccountsManager.OnAccountAdded += new WeakEventHandler<AccountDataItem>(AccountsManager_OnAccountAdded).Handler;

            var lastLoginLocalId = AccountsManager.GetLastLoginLocalId();
            if (lastLoginLocalId != Guid.Empty)
            {
                var lastLogin = Accounts.FirstOrDefault(i => i.LocalAccountId == lastLoginLocalId);

                if (lastLogin != null && lastLogin.RememberUsername && !lastLogin.IsDefaultOfflineAccount)
                    Username = lastLogin.Username;
            }
        }

        private void OnUsernameChanged()
        {
            FillInPassword(Username);
        }

        private void OnPasswordChanged()
        {
            // If we previously auto filled and the user is changing it, remove the auto fill so they have to type their correct password.
            // We check if it equals stored pass, since when we programmatically set the password to STORED_PASS, that'll trigger this event,
            // but since it will equal STORED_PASS, we won't immediately disable it
            if (_autoFilledLocalToken != null && !Password.Equals(STORED_PASS))
            {
                _autoFilledLocalToken = null;
            }
        }

        private void AccountsManager_OnAccountAdded(object sender, AccountDataItem e)
        {
            Dispatcher.Run(delegate
            {
                Accounts.Add(e);
            });
        }

        private void AccountsManager_OnAccountDeleted(object sender, Guid e)
        {
            Dispatcher.Run(delegate
            {
                Accounts.RemoveWhere(i => i.LocalAccountId == e);
            });
        }

        public bool CanLogin()
        {
            return _autoFilledLocalToken != null;
        }

        public async System.Threading.Tasks.Task LoginAsync()
        {
            try
            {
                string username = getUsername();
                string password = getPassword();

                var matching = await FindAccountByUsername(username);

                if (matching == null)
                    localNotFound(username);

                else
                {
                    if ((_autoFilledLocalToken != null && matching.LocalToken.Equals(_autoFilledLocalToken))
                        || PowerPlannerAuth.ValidatePasswordLocally(
                            password: password,
                            localUsername: username,
                            localToken: matching.LocalToken))
                    {
                        ToMainPage(matching);
                    }

                    //if a local transferred account
                    else if (!matching.IsOnlineAccount && matching.LocalToken.Equals(Variables.OLD_PASSWORD))
                    {
                        //update password with the new password
                        matching.LocalToken = password;
                        await AccountsManager.Save(matching);
                        ToMainPage(matching);
                    }

                    else
                        incorrectLocalPassword(matching, password);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                await new PortableMessageDialog("Error logging in. Your issue has been sent to the developer.").ShowAsync();
            }
        }

        private string getUsername()
        {
            return Username;
        }

        private string getPassword()
        {
            return Password;
        }

        private string _autoFilledLocalToken;

        private void FillInPassword(string username)
        {
            try
            {
                var matching = FindAccountByUsername(Accounts, username);

                // If matching, we potentially can fill in password if the user has selected to remember password
                if (matching != null && matching.RememberPassword)
                {
                    _autoFilledLocalToken = matching.LocalToken;
                    _password = STORED_PASS;
                    OnPropertyChanged(nameof(Password));
                }

                // Otherwise, we should clear the password box (if it's displaying our Stored Password message)
                else
                {
                    if (_autoFilledLocalToken != null)
                    {
                        _password = "";
                        OnPropertyChanged(nameof(Password));
                    }
                }
            }

            catch { }
        }

        /// <summary>
        /// Loads all accounts, and then finds
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<AccountDataItem> FindAccountByUsername(string username)
        {
            var allAccounts = await GetAllAccounts();

            return FindAccountByUsername(allAccounts, username);
        }

        private static AccountDataItem FindAccountByUsername(IEnumerable<AccountDataItem> allAccounts, string username)
        {
            return allAccounts.FirstOrDefault(i => i.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
        }

        private async void incorrectLocalPassword(AccountDataItem account, string password)
        {
            if (account.IsOnlineAccount)
            {
                try
                {
                    IsCheckingOnlinePassword = true;
                    var resp = await PowerPlannerAuth.CheckUpdatedCredentialsAsync(
                        accountId: account.AccountId,
                        username: account.Username,
                        password: password);

                    if (resp.Error != null)
                        ShowMessage(resp.Error, "Password error");

                    else
                    {
                        //update to new password
                        account.LocalToken = resp.LocalToken;
                        account.Token = resp.Token;
                        await AccountsManager.Save(account);

                        //then log them in
                        ToMainPage(account);
                    }
                }

                catch
                {
                    ShowMessage(PowerPlannerResources.GetString("LoginPage_String_ExplanationIncorrectPasswordAndOffline"), PowerPlannerResources.GetString("LoginPage_String_IncorrectPassword"));
                }

                finally
                {
                    IsCheckingOnlinePassword = false;
                }
            }

            else
            {
                ShowMessage(PowerPlannerResources.GetString("LoginPage_String_ExplanationIncorrectPasswordAndLocalAccount"), PowerPlannerResources.GetString("LoginPage_String_IncorrectPassword"));
            }
        }

        public async void ToMainPage(AccountDataItem account, bool existingAccount = true)
        {
            AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);

            await base.FindAncestor<MainWindowViewModel>().SetCurrentAccount(account, syncAccount: true);

            if (existingAccount)
            {
                account.ExecuteOnLoginTasks();
            }

            if (DefaultAccountToDelete != null)
            {
                StartDeleteDefaultAccount();
            }
        }

        private async void StartDeleteDefaultAccount()
        {
            try
            {
                await AccountsManager.Delete(DefaultAccountToDelete.LocalAccountId);
            }
            catch
            {
                // We don't really care if delete fails
            }
        }

        private async void localNotFound(string username)
        {
            string password = getPassword();

            try
            {
                IsLoggingInOnline = true;

                OnlineLoginResponse resp;

                try
                {
                    resp = await PowerPlannerAuth.LoginOnlineAndAddDeviceAsync(username, password);
                }

                catch
                {
                    IsLoggingInOnline = false;
                    throw;
                }

                IsLoggingInOnline = false;

                if (resp.Error != null)
                {
                    ShowMessage(resp.Error, "Error logging in");
                }

                else
                {
                    AccountDataItem account = await CreateAccount(username, resp.LocalToken, resp.Token, resp.AccountId, resp.DeviceId);

                    if (account != null)
                    {
                        TelemetryExtension.Current?.TrackEvent("LoggedInToOnlineAccount");

                        AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);

                        ToMainPage(account, existingAccount: false);
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Failed logging into online account: " + ex.ToString());

                ShowMessage(PowerPlannerResources.GetString("LoginPage_String_ExplanationOfflineAndNoLocalAccountFound"), PowerPlannerResources.GetString("LoginPage_String_NoAccountFoundHeader"));
            }

            finally
            {
                IsLoggingInOnline = false;
            }
        }

        /// <summary>
        /// Creates an online account
        /// </summary>
        /// <param name="username"></param>
        /// <param name="localToken"></param>
        /// <param name="token"></param>
        /// <param name="accountId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public System.Threading.Tasks.Task<AccountDataItem> CreateAccount(string username, string localToken, string token, long accountId, int deviceId)
        {
            return CreateAccountHelper.CreateAccountLocally(username, localToken, token, accountId, deviceId, needsInitialSync: true);
        }

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }

        private async Task<List<AccountDataItem>> GetAllAccounts()
        {
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                return await AccountsManager.GetAllAccounts();
            });
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
