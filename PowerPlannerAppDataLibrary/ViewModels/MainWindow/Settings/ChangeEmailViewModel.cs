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
        public override bool ImportantForAutofill => true;

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

        private bool _isRetrievingEmail = true;

        public bool IsRetrievingEmail
        {
            get { return _isRetrievingEmail; }
            set { SetProperty(ref _isRetrievingEmail, value, nameof(IsRetrievingEmail)); }
        }

        private bool _isUpdatingEmail;

        public bool IsUpdatingEmail
        {
            get { return _isUpdatingEmail; }
            set { SetProperty(ref _isUpdatingEmail, value, nameof(IsUpdatingEmail)); }
        }

        private string _email = "";

        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value, nameof(Email)); }
        }

        private void SetError(string error)
        {
            Error = error;
        }

        private string _error = "";

        public string Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value, nameof(Error)); }
        }

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
