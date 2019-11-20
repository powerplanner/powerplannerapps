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
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount
{
    public class CreateAccountViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public Action AlertPasswordTooShort = delegate { ShowMessage("Your password is too short.", "Password too short"); };
        public Action AlertConfirmationPasswordDidNotMatch = delegate { ShowMessage("Your confirmation password didn't match.", "Invalid password"); };
        public Action AlertNoUsername = delegate { ShowMessage("You must provide a username!", "No username"); };
        public Action AlertNoEmail = delegate { ShowMessage("You must provide an email!", "No email"); };

        public CreateAccountViewModel(BaseViewModel parent) : base(parent) { }

        public AccountDataItem DefaultAccountToUpgrade { get; private set; }

        /// <summary>
        /// Creating a local account should only be allowed when not upgrading the default account
        /// </summary>
        public bool IsCreateLocalAccountVisible => DefaultAccountToUpgrade == null;

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

            if (DefaultAccountToUpgrade != null && DefaultAccountToUpgrade.IsOnlineAccount)
            {
                // This shouldn't ever happen, but would happen if it failed to hide the create account page after successfully creating.
                TelemetryExtension.Current?.TrackException(new Exception("Trying to upgrade default account when it's already online"));
                return false;
            }

            return true;
        }


        public async void CreateLocalAccount()
        {
            if (DefaultAccountToUpgrade != null)
            {
                // This code should never be hit. If it does get hit, that implies the UI wasn't correctly hiding the option for
                // creating the local account (it should be hidden when upgrading a default account, only allowing online account).
                TelemetryExtension.Current?.TrackException(new Exception("Tried to create local account for default account"));
                return;
            }

            if (!isOkayToCreateLocal())
                return;

            string localToken = PowerPlannerAuth.CreateOfflineAccount(Username, Password);

            await FinishCreateAccount(Username, localToken, null, 0, 0, "");
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
                    await FinishCreateAccount(username, resp.LocalToken, resp.Token, resp.AccountId, resp.DeviceId, email);
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

        private async System.Threading.Tasks.Task FinishCreateAccount(string username, string localToken, string token, long accountId, int deviceId, string email)
        {
            if (DefaultAccountToUpgrade != null)
            {
                if (DefaultAccountToUpgrade.IsOnlineAccount)
                {
                    throw new Exception("Should be an offline account. This implies it got created but for some reason stayed on this page.");
                }

                DefaultAccountToUpgrade.Username = username;
                DefaultAccountToUpgrade.LocalToken = localToken;
                DefaultAccountToUpgrade.Token = token;
                DefaultAccountToUpgrade.AccountId = accountId;
                DefaultAccountToUpgrade.DeviceId = deviceId;
                DefaultAccountToUpgrade.NeedsToSyncSettings = true; // Make sure we sync settings so things like week info uploads

                await AccountsManager.Save(DefaultAccountToUpgrade);

                TelemetryExtension.Current?.TrackEvent("CreatedAccountFromDefault");

                // Transfer the settings
                try
                {
                    await SavedGradeScalesManager.TransferToOnlineAccountAsync(DefaultAccountToUpgrade.LocalAccountId, DefaultAccountToUpgrade.AccountId);
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                // Make sure to update user info for telemetry
                TelemetryExtension.Current?.UpdateCurrentUser(DefaultAccountToUpgrade);

                // Remove this popup, and show a new one saying success!
                // We have to show first before removing otherwise iOS never shows it
                var parent = Parent;
                parent.ShowPopup(new SuccessfullyCreatedAccountViewModel(parent)
                {
                    Username = username,
                    Email = email
                });
                base.RemoveViewModel();

                // Trigger a sync (without waiting) so all their content uploads
                Sync.StartSyncAccount(DefaultAccountToUpgrade);
            }
            else
            {
                var account = await AccountsManager.CreateAndInitializeAccountAsync(username, localToken, token, accountId, deviceId);

                if (account != null)
                {
                    // Take us to the account
                    var dontWait = FindAncestor<MainWindowViewModel>().SetCurrentAccount(account);
                }
            }
        }

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }

        public static CreateAccountViewModel CreateForUpgradingDefaultAccount(BaseViewModel parent, AccountDataItem account)
        {
            if (account.IsOnlineAccount)
            {
                throw new Exception("Should be an offline account");
            }

            return new CreateAccountViewModel(parent)
            {
                DefaultAccountToUpgrade = account
            };
        }
    }
}
