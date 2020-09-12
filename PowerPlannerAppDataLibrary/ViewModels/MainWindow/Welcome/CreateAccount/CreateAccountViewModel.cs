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
            set { SetProperty(ref _username, value, nameof(Username)); UpdateUsernameState(); }
        }

        private bool _usernameHasFocus;
        public bool UsernameHasFocus
        {
            get => _usernameHasFocus;
            set
            {
                _usernameHasFocus = value;

                if (!value)
                {
                    UpdateUsernameState();
                }
            }
        }

        private InputValidationState _usernameState;
        public InputValidationState UsernameState
        {
            get => _usernameState;
            set => SetProperty(ref _usernameState, value, nameof(UsernameState));
        }

        private SimpleAsyncWorkerQueue _updateUsernameErrorsQueue = new SimpleAsyncWorkerQueue();
        private void UpdateUsernameState()
        {
            _updateUsernameErrorsQueue.QueueOrMergeAsync("username", async delegate
            {
                try
                {
                    var username = Username.Trim();
                    var ex = await AccountsManager.GetUsernameErrorAsync(username);

                    // If we have focus and currently unset, keep unset till either error kicks in or invalid value typed
                    if (UsernameHasFocus && UsernameState == null && ex == null)
                    {
                        return;
                    }

                    UsernameState = ex != null ? InputValidationState.Invalid(ex.FriendlyMessage) : InputValidationState.Valid;
                }
                catch { }
            });
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, nameof(Password)); UpdatePasswordState(); }
        }

        private bool _passwordHasFocus;
        public bool PasswordHasFocus
        {
            get => _passwordHasFocus;
            set
            {
                _passwordHasFocus = value;

                if (!value)
                {
                    UpdatePasswordState();
                }
            }
        }

        private InputValidationState _passwordState;
        public InputValidationState PasswordState
        {
            get => _passwordState;
            set => SetProperty(ref _passwordState, value, nameof(PasswordState));
        }

        private void UpdatePasswordState()
        {
            if (PasswordHasFocus)
            {
                if (PasswordState == null)
                {
                    ConfirmPasswordState = null;
                    return;
                }
                else if (PasswordState == InputValidationState.Valid)
                {
                    PasswordState = null;
                    ConfirmPasswordState = null;
                    return;
                }
            }

            if (isPasswordTooShort())
            {
                PasswordState = InputValidationState.Invalid(PowerPlannerResources.GetString("PasswordInvalid_TooShort"));
            }
            else
            {
                PasswordState = InputValidationState.Valid;
            }

            if (ConfirmPasswordState != null)
            {
                UpdateConfirmPasswordState();
            }
        }

        private string _confirmPassword = "";
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetProperty(ref _confirmPassword, value, nameof(ConfirmPassword)); UpdateConfirmPasswordState(); }
        }

        private bool _confirmPasswordHasFocus;
        public bool ConfirmPasswordHasFocus
        {
            get => _confirmPasswordHasFocus;
            set
            {
                _confirmPasswordHasFocus = value;

                if (!value)
                {
                    UpdateConfirmPasswordState();
                }
            }
        }

        private InputValidationState _confirmPasswordState;
        public InputValidationState ConfirmPasswordState
        {
            get => _confirmPasswordState;
            set => SetProperty(ref _confirmPasswordState, value, nameof(ConfirmPasswordState));
        }

        private void UpdateConfirmPasswordState()
        {
            if (PasswordState != InputValidationState.Valid)
            {
                ConfirmPasswordState = null ;
            }
            else
            {
                if (Password == ConfirmPassword)
                {
                    ConfirmPasswordState = InputValidationState.Valid;
                }
                else
                {
                    if (ConfirmPasswordHasFocus)
                    {
                        if (ConfirmPasswordState == null)
                        {
                            return;
                        }
                        else if (ConfirmPasswordState == InputValidationState.Valid)
                        {
                            ConfirmPasswordState = null;
                            return;
                        }
                    }

                    else
                    {
                        ConfirmPasswordState = InputValidationState.Invalid(PowerPlannerResources.GetString("ConfirmPasswordInvalid"));
                    }
                }
            }
        }

        private string _email = "";
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); UpdateEmailState(); }
        }

        private InputValidationState _emailState;
        public InputValidationState EmailState
        {
            get => _emailState;
            set => SetProperty(ref _emailState, value, nameof(EmailState));
        }

        private void UpdateEmailState(bool forceUpdate = false)
        {
            if (Email.Length > 150)
            {
                EmailState = InputValidationState.Invalid(PowerPlannerResources.GetString("EmailInvalid_TooLong"));
                return;
            }

            if (EmailState == null && !forceUpdate)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailState = InputValidationState.Invalid(PowerPlannerResources.GetString("EmailInvalid_Empty"));
            }
            else if (!StringTools.IsEmailValid(Email))
            {
                EmailState = InputValidationState.Invalid(PowerPlannerResources.GetString("EmailInvalid_Invalid"));
            }
            else
            {
                EmailState = InputValidationState.Valid;
            }
        }

        private bool isPasswordTooShort()
        {
            return Password.Length < 5;
        }

        private bool isPasswordOkay()
        {
            return PasswordState == InputValidationState.Valid && ConfirmPasswordState == InputValidationState.Valid;
        }

        private bool isUsernameOkay()
        {
            return UsernameState == InputValidationState.Valid;
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

            if (EmailState != InputValidationState.Valid)
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
            UpdateEmailState(forceUpdate: true);

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

                TelemetryExtension.Current?.TrackEvent("CreatedAccountFromDefault", new Dictionary<string, string>()
                {
                    { "NewOnlineAccountId", accountId.ToString() }
                });

                // Make sure to update user info for telemetry
                TelemetryExtension.Current?.UpdateCurrentUser(DefaultAccountToUpgrade);

                // Transfer the settings
                try
                {
                    await SavedGradeScalesManager.TransferToOnlineAccountAsync(DefaultAccountToUpgrade.LocalAccountId, DefaultAccountToUpgrade.AccountId);
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                try
                {
                    // Trigger a sync so all their content uploads
                    DateTime start = DateTime.UtcNow;

                    var syncResult = await Sync.SyncAccountAsync(DefaultAccountToUpgrade);

                    bool retried = false;
                    if (syncResult.Error != null)
                    {
                        // Try one more time
                        retried = true;
                        syncResult = await Sync.SyncAccountAsync(DefaultAccountToUpgrade);
                    }

                    var duration = DateTime.UtcNow - start;

                    Dictionary<string, string> properties = new Dictionary<string, string>()
                    {
                        { "Duration", ((int)Math.Ceiling(duration.TotalSeconds)).ToString() }
                    };

                    if (retried)
                    {
                        properties["Retried"] = "true";
                    }

                    if (syncResult.Error != null)
                    {
                        properties["Error"] = syncResult.Error;
                    }

                    TelemetryExtension.Current?.TrackEvent("InitialSyncAfterCreatingAccount", properties);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                // Remove this popup, and show a new one saying success!
                // We have to show first before removing otherwise iOS never shows it
                var parent = Parent;
                parent.ShowPopup(new SuccessfullyCreatedAccountViewModel(parent)
                {
                    Username = username,
                    Email = email
                });
                base.RemoveViewModel();
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
