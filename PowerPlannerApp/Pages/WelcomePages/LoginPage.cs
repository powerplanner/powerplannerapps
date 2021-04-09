using PowerPlannerApp.App;
using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Extensions;
using PowerPlannerApp.Views;
using PowerPlannerAppAuthLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages.WelcomePages
{
    public class LoginPage : VxPage
    {
        private VxState<string> _username = new VxSilentState<string>("");
        private VxState<string> _password = new VxSilentState<string>("");
        private VxState<bool> _loggingIn = new VxState<bool>(false);

        protected override View Render()
        {
            return new PopupWindow
            {
                Title = "Login",
                AutoScrollAndPad = true,
                Content = new StackLayout
                {
                    Children =
                    {
                        new VxEntry
                        {
                            Title = "Username",
                            Text = _username
                        },

                        new VxEntry
                        {
                            Title = "Password",
                            Text = _password,
                            Margin = new Thickness(0,12,0,0)
                        },

                        new Button
                        {
                            Text = _loggingIn.Value ? "Logging in..." : "Log in",
                            IsEnabled = _loggingIn.Value,
                            Command = CreateCommand(Login),
                            Margin = new Thickness(0,24,0,0)
                        }
                    }
                }
            };
        }

        /// <summary>
        /// The default offline account that should be deleted after successful login
        /// </summary>
        public AccountDataItem DefaultAccountToDelete { get; set; }

        private string _autoFilledLocalToken;

        private async void Login()
        {
            _loggingIn.Value = true;

            try
            {
                string username = _username.Value;
                string password = _password.Value;

                var matching = await FindAccountByUsername(username);

                if (matching == null)
                    LocalNotFound(username);

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
                        IncorrectLocalPassword(matching, password);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                _loggingIn.Value = false;
                await new PortableMessageDialog("Error logging in. Your issue has been sent to the developer.").ShowAsync();
            }
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

        private async Task<List<AccountDataItem>> GetAllAccounts()
        {
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                return await AccountsManager.GetAllAccounts();
            });
        }

        public async void ToMainPage(AccountDataItem account, bool existingAccount = true)
        {
            AccountsManager.SetLastLoginIdentifier(account.LocalAccountId);

            await PowerPlannerVxApp.Current.SetCurrentAccount(account, syncAccount: true);

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
        private async void LocalNotFound(string username)
        {
            string password = _password.Value;

            try
            {
                OnlineLoginResponse resp;

                try
                {
                    resp = await PowerPlannerAuth.LoginOnlineAndAddDeviceAsync(username, password);
                }

                catch
                {
                    throw;
                }

                _loggingIn.Value = false;

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
                _loggingIn.Value = false;
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

        private async void IncorrectLocalPassword(AccountDataItem account, string password)
        {
            //if (account.IsOnlineAccount)
            //{
            //    try
            //    {
            //        IsCheckingOnlinePassword = true;
            //        var resp = await PowerPlannerAuth.CheckUpdatedCredentialsAsync(
            //            accountId: account.AccountId,
            //            username: account.Username,
            //            password: password);

            //        if (resp.Error != null)
            //            ShowMessage(resp.Error, "Password error");

            //        else
            //        {
            //            //update to new password
            //            account.LocalToken = resp.LocalToken;
            //            account.Token = resp.Token;
            //            await AccountsManager.Save(account);

            //            //then log them in
            //            ToMainPage(account);
            //        }
            //    }

            //    catch
            //    {
            //        ShowMessage(PowerPlannerResources.GetString("LoginPage_String_ExplanationIncorrectPasswordAndOffline"), PowerPlannerResources.GetString("LoginPage_String_IncorrectPassword"));
            //    }

            //    finally
            //    {
            //        IsCheckingOnlinePassword = false;
            //    }
            //}

            //else
            //{
            //    ShowMessage(PowerPlannerResources.GetString("LoginPage_String_ExplanationIncorrectPasswordAndLocalAccount"), PowerPlannerResources.GetString("LoginPage_String_IncorrectPassword"));
            //}
        }
    }
}
