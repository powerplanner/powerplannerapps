using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class ResetPasswordViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public ResetPasswordViewModel(BaseViewModel parent, string username) : base(parent)
        {
            Title = PowerPlannerResources.GetString("LoginPage_TextBlockForgotPassword.Text");
            Username = username;
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBox
                        {
                            Header = PowerPlannerResources.GetString("ForgotPassword_TextBoxUsername.Header"),
                            PlaceholderText = PowerPlannerResources.GetString("ForgotPassword_TextBoxUsername.PlaceholderText"),
                            Text = VxValue.Create(Username, t => Username = t),
                            InputScope = InputScope.Username,
                            AutoFocus = true,
                            OnSubmit = ResetPassword,
                            IsEnabled = !IsResettingPassword
                        },

                        new TextBox
                        {
                            Header = PowerPlannerResources.GetString("ForgotPassword_TextBoxEmail.Header"),
                            PlaceholderText = PowerPlannerResources.GetString("ForgotPassword_TextBoxEmail.PlaceholderText"),
                            Text = VxValue.Create(Email, t => Email = t),
                            InputScope = InputScope.Email,
                            Margin = new Thickness(0, 16, 0, 0),
                            OnSubmit = ResetPassword,
                            IsEnabled = !IsResettingPassword
                        },

                        new AccentButton
                        {
                            Text = PowerPlannerResources.GetString("ForgotPassword_ButtonReset.Content"),
                            Click = ResetPassword,
                            Margin = new Thickness(0, 24, 0, 0),
                            IsEnabled = !IsResettingPassword
                        }
                    }
                }
            };
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, nameof(Username)); }
        }

        private string _email = ForgotUsernameViewModel.StoredEmail;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); ForgotUsernameViewModel.StoredEmail = value; }
        }

        private bool _isResettingPassword;
        public bool IsResettingPassword
        {
            get { return _isResettingPassword; }
            set { SetProperty(ref _isResettingPassword, value, nameof(IsResettingPassword)); }
        }
        
        public async void ResetPassword()
        {
            IsResettingPassword = true;

            try
            {
                var email = Email.Trim();
                var username = Username.Trim();

                ResetPasswordResponse resp = await WebHelper.Download<ResetPasswordRequest, ResetPasswordResponse>(
                    Website.URL + "resetpasswordmodern",
                    new ResetPasswordRequest() { Username = username, Email = email },
                    Website.ApiKey);

                if (resp == null)
                    return;

                if (resp.Error != null)
                {
                    var dontWait = new PortableMessageDialog(resp.Error, PowerPlannerResources.GetString("ResetPassword_String_ErrorResettingPassword")).ShowAsync();
                }

                else
                {
                    IsResettingPassword = false;
                    var loginViewModel = Parent.GetPopupViewModelHost()?.Popups.OfType<LoginViewModel>().FirstOrDefault();
                    if (loginViewModel != null)
                    {
                        loginViewModel.Username = username;
                    }
                    await new PortableMessageDialog(resp.Message, PowerPlannerResources.GetString("ResetPassword_String_ResetSuccessHeader")).ShowAsync();
                    base.RemoveViewModel();
                }
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                var dontWait = new PortableMessageDialog(PowerPlannerResources.GetStringOfflineExplanation(), PowerPlannerResources.GetString("ResetPassword_String_ErrorResettingPassword")).ShowAsync();
            }

            finally
            {
                IsResettingPassword = false;
            }
        }
    }
}
