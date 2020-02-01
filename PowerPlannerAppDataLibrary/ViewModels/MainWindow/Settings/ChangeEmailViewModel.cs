using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangeEmailViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public AccountDataItem Account { get; private set; }

        public ChangeEmailViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                GetEmailResponse response = await Account.PostAuthenticatedAsync<GetEmailRequest, GetEmailResponse>(
                    Website.URL + "getemailmodern",
                    new GetEmailRequest());

                if (response != null)
                {
                    if (response.Error != null)
                        SetError(response.Error);

                    else
                    {
                        Email = response.Email;
                        IsRetrievingEmail = false;
                    }

                    return;
                }
            }

            catch
            {

            }

            SetError(PowerPlannerResources.GetString("Settings_ChangeEmailPage_Errors_FailedGrabEmail"));
        }


        public bool IsRetrievingEmail { get; set; } = true;


        public bool IsUpdatingEmail { get; set; }


        public string Email { get; set; } = "";

        private void SetError(string error)
        {
            Error = error;
        }


        public string Error { get; set; } = "";

        public async void UpdateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                SetError("You must provide an email address!");
                return;
            }

            IsUpdatingEmail = true;

            try
            {
                ChangeEmailResponse response = await Account.PostAuthenticatedAsync<ChangeEmailRequest, ChangeEmailResponse>(
                    Website.URL + "changeemailmodern",
                    new ChangeEmailRequest()
                    {
                        NewEmail = Email
                    });

                if (response.Error != null)
                {
                    SetError(response.Error);
                }

                else
                    GoBack();
            }

            catch
            {
                SetError(PowerPlannerResources.GetString("Settings_ChangeEmailPage_Errors_FailedUpdateOnline"));
            }

            finally
            {
                IsUpdatingEmail = false;
            }
        }
    }
}
