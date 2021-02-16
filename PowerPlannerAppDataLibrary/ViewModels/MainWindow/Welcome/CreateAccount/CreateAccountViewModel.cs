﻿using System;
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
using BareMvvm.Core;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount
{
    public class CreateAccountViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        /// <summary>
        /// Views can set this to false
        /// </summary>
        public bool IsUsingConfirmPassword { get; set; } = true;

        private List<AccountDataItem> _accounts;

        public CreateAccountViewModel(BaseViewModel parent) : base(parent)
        {
            Username = new TextField(required: true, maxLength: 50, inputValidator: new CustomInputValidator(ValidateUsername), ignoreOuterSpaces: true, reportValidatorInvalidInstantly: true);
            Email = new TextField(required: true, maxLength: 150, inputValidator: EmailInputValidator.Instance, ignoreOuterSpaces: true);
            Password = new TextField(required: true, maxLength: 50, minLength: 5);
            ConfirmPassword = new TextField(required: true, mustMatch: Password);

            LoadAccounts();
        }

        private async void LoadAccounts()
        {
            try
            {
                _accounts = await AccountsManager.GetAllAccounts();
            }
            catch
            {
                _accounts = new List<AccountDataItem>();
            }
        }

        public AccountDataItem DefaultAccountToUpgrade { get; private set; }

        /// <summary>
        /// Creating a local account should only be allowed when not upgrading the default account
        /// </summary>
        public bool IsCreateLocalAccountVisible => DefaultAccountToUpgrade == null;

        public TextField Username { get; private set; }

        private InputValidationState ValidateUsername(string username)
        {
            if (username.Contains(' '))
                return InputValidationState.Invalid(PowerPlannerResources.GetString("UsernameInvalid_ContainsSpace"));

            if (!StringTools.IsStringFilenameSafe(username) || !StringTools.IsStringUrlSafe(username))
            {
                var characters = username.ToCharArray().Distinct().ToArray();
                var validSpecialChars = StringTools.VALID_SPECIAL_FILENAME_CHARS.Intersect(StringTools.VALID_SPECIAL_URL_CHARS).ToArray();

                var validCharacters = characters.Where(i => Char.IsLetterOrDigit(i) || validSpecialChars.Contains(i)).ToArray();
                var invalidCharacters = characters.Except(validCharacters).ToArray();

                try
                {
                    return InputValidationState.Invalid(PowerPlannerResources.GetStringWithParameters("UsernameInvalid_InvalidCharacters", string.Join(", ", invalidCharacters)));
                }
                catch
                {
                    return InputValidationState.Invalid("Invalid");
                }
            }

            if (_accounts == null)
            {
                return null;
            }

            if (_accounts.Any(i => i.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)))
            {
                return InputValidationState.Invalid(PowerPlannerResources.GetString("UsernameInvalid_UsernameExists"));
            }

            return InputValidationState.Valid;
        }

        public TextField Password { get; private set; }

        public TextField ConfirmPassword { get; private set; }

        public TextField Email { get; private set; }

        private bool isOkayToCreate()
        {
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

            if (!ValidateAllInputs(customValidators: new Dictionary<string, Action<TextField>>()
            {
                // Email isn't required for local accounts
                { nameof(Email), f => f.Validate(overrideRequired: false) },
                
                // Confirm password may not be required by some views
                { nameof(ConfirmPassword), f => f.Validate(overrideRequired: IsUsingConfirmPassword) }
            }))
            {
                return;
            }

            string localToken = PowerPlannerAuth.CreateOfflineAccount(Username.Text.Trim(), Password.Text);

            await FinishCreateAccount(Username.Text.Trim(), localToken, null, 0, 0, "");
        }

        private bool _isCreatingOnlineAccount;
        public bool IsCreatingOnlineAccount
        {
            get { return _isCreatingOnlineAccount; }
            set { SetProperty(ref _isCreatingOnlineAccount, value, nameof(IsCreatingOnlineAccount)); }
        }

        public async void CreateAccount()
        {
            if (!ValidateAllInputs(customValidators: new Dictionary<string, Action<TextField>>()
            {
                // Confirm password may not be required by some views
                { nameof(ConfirmPassword), f => f.Validate(overrideRequired: IsUsingConfirmPassword) }
            }))
            {
                return;
            }

            if (!isOkayToCreate())
                return;

            string username = Username.Text.Trim();
            string password = Password.Text;
            string email = Email.Text.Trim();

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
                    _ = FindAncestor<MainWindowViewModel>().SetCurrentAccount(account);
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
