using BareMvvm.Core;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ChangeEmailViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        public AccountDataItem Account { get; private set; }

        public ChangeEmailViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_ChangeEmailPage.Title");

            Account = account;
            InitializeEmail();
        }

        protected override View Render()
        {
            bool isEnabled = !IsRetrievingEmail && !IsUpdatingEmail;

            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("Settings_ChangeEmailPage_TextBoxEmail.Header"),
                    Text = Bind<string>(nameof(Email.Text), Email),
                    ValidationState = Email.ValidationState,
                    HasFocusChanged = f => Email.HasFocus = f,
                    IsEnabled = isEnabled,
                    AutoFocus = true,
                    OnSubmit = UpdateEmail
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_ChangeEmailPage_ButtonUpdateEmail.Content"),
                    Click = UpdateEmail,
                    Margin = new Thickness(0, 24, 0, 0)
                }

            );
        }

        private async void InitializeEmail()
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
                        Email.Text = response.Email;
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

        [VxSubscribe]
        public TextField Email { get; private set; } = CreateAccountViewModel.GenerateEmailTextField();

        private void SetError(string error)
        {
            Email.SetError(error);
        }

        public async void UpdateEmail()
        {
            if (!ValidateAllInputs())
            {
                return;
            }

            IsUpdatingEmail = true;

            try
            {
                ChangeEmailResponse response = await Account.PostAuthenticatedAsync<ChangeEmailRequest, ChangeEmailResponse>(
                    Website.URL + "changeemailmodern",
                    new ChangeEmailRequest()
                    {
                        NewEmail = Email.Text.Trim()
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
