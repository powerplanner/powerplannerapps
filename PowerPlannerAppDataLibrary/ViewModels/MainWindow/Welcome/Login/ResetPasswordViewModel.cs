using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class ResetPasswordViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public ResetPasswordViewModel(BaseViewModel parent, string username) : base(parent)
        {
            Username = username;
        }

        public string Username { get; set; }

        private string _email = ForgotUsernameViewModel.StoredEmail;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); ForgotUsernameViewModel.StoredEmail = value; }
        }

        public bool IsResettingPassword { get; set; }
        
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
