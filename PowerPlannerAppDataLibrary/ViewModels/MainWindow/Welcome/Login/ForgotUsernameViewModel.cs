using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerSending;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class ForgotUsernameViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public static string StoredEmail = "";

        public ForgotUsernameViewModel(BaseViewModel parent) : base(parent)
        {
        }

        private bool _isRecoveringUsernames;
        public bool IsRecoveringUsernames
        {
            get { return _isRecoveringUsernames; }
            private set { SetProperty(ref _isRecoveringUsernames, value, nameof(IsRecoveringUsernames)); }
        }

        private string _email = StoredEmail;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); ForgotUsernameViewModel.StoredEmail = value; }
        }

        public async void Recover()
        {
            IsRecoveringUsernames = true;

            try
            {
                var email = Email.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    var dontWait = new PortableMessageDialog("You must enter an email address!", PowerPlannerResources.GetString("ForgotUsername_String_ErrorFindingUsername")).ShowAsync();
                    return;
                }

                ForgotUsernameResponse response = await WebHelper.Download<ForgotUsernameRequest, ForgotUsernameResponse>(
                        Website.URL + "forgotusernamemodern",
                        new ForgotUsernameRequest() { Email = email },
                        Website.ApiKey);

                if (response == null)
                {
                    var dontWait = new PortableMessageDialog(response.Error, PowerPlannerResources.GetString("ForgotUsername_String_ErrorFindingUsername")).ShowAsync();
                    return;
                }

                if (response.Usernames.Count == 0)
                {
                    var dontWait = new PortableMessageDialog(string.Format(PowerPlannerResources.GetString("ForgotUsername_String_NoUsernameFoundExplanation"), email), PowerPlannerResources.GetString("ForgotUsername_String_NoUsernameFoundHeader")).ShowAsync();
                    return;
                }

                base.ShowPopup(new RecoveredUsernamesViewModel(Parent, response.Usernames.ToArray()));
                base.RemoveViewModel();
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                var dontWait = new PortableMessageDialog(PowerPlannerResources.GetStringOfflineExplanation(), PowerPlannerResources.GetString("ForgotUsername_String_ErrorFindingUsername")).ShowAsync();
            }

            finally
            {
                IsRecoveringUsernames = false;
            }
        }
    }
}
