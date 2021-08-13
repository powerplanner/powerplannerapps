using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class UpdateCredentialsViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public UpdateTypes UpdateType { get; private set; }
        public AccountDataItem Account { get; private set; }

        private UpdateCredentialsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_UpdateCredentialsPage.Title");
        }

        protected override View Render()
        {
            bool isEnabled = !IsLoggingIn;

            return RenderGenericPopupContent(

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_UpdateCredentialsPage_Description.Text")
                },

                !string.IsNullOrWhiteSpace(Error) ? new TextBlock
                {
                    Text = Error,
                    FontWeight = FontWeights.Bold,
                    TextColor = Color.Red,
                    Margin = new Thickness(0, 12, 0, 0)
                } : null,

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("TextBox_Username.Header"),
                    Text = VxValue.Create(Username, v => Username = v),
                    Margin = new Thickness(0, 24, 0, 0),
                    InputScope = InputScope.Username,
                    OnSubmit = LogIn,
                    IsEnabled = isEnabled
                },

                new PasswordBox
                {
                    Header = PowerPlannerResources.GetString("TextBox_Password.Header"),
                    Text = VxValue.Create(Password, v => Password = v),
                    Margin = new Thickness(0, 16, 0, 0),
                    OnSubmit = LogIn,
                    IsEnabled = isEnabled
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString(IsLoggingIn ? "LoginPage_String_LoggingIn" : "WelcomePage_ButtonLogin.Content"),
                    Click = LogIn,
                    Margin = new Thickness(0, 24, 0, 0),
                    IsEnabled = isEnabled
                },

                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 24, 0, 0),
                            Children =
                            {
                                new TextButton
                                {
                                    Text = PowerPlannerResources.GetString("LoginPage_TextBlockForgotUsername.Text"),
                                    Click = ForgotUsername,
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Margin = new Thickness(0, 0, 12, 0),
                                    IsEnabled = isEnabled
                                }.LinearLayoutWeight(1),

                                new Border
                                {
                                    BackgroundColor = Theme.Current.SubtleForegroundColor,
                                    Width = 1
                                },

                                new TextButton
                                {
                                    Text = PowerPlannerResources.GetString("LoginPage_TextBlockForgotPassword.Text"),
                                    Click = ForgotPassword,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Margin = new Thickness(12, 0, 0, 0),
                                    IsEnabled = isEnabled
                                }.LinearLayoutWeight(1)
                            }
                        }

            );
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
