﻿using BareMvvm.Core;
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

        public static event EventHandler<string> EmailChanged;

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

                new TextBox(Email)
                {
                    Header = PowerPlannerResources.GetString("Settings_ChangeEmailPage_TextBoxEmail.Header"),
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
                Email.Text = (await GetEmailAsync(Account)).Item1;
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }

            IsRetrievingEmail = false;
        }

        public static async Task<Tuple<string, bool>> GetEmailAsync(AccountDataItem account)
        {
            try
            {
                GetEmailResponse response = await account.PostAuthenticatedAsync<GetEmailRequest, GetEmailResponse>(
                    Website.ClientApiUrl + "getemailmodern",
                    new GetEmailRequest());

                if (response != null)
                {
                    if (response.Error != null)
                    {
                        throw new Exception(response.Error);
                    }

                    else
                    {
                        return new Tuple<string, bool>(response.Email, response.EmailVerified);
                    }
                }
            }

            catch
            {

            }

            throw new Exception(R.S("Settings_ChangeEmailPage_Errors_FailedGrabEmail"));
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
                    Website.ClientApiUrl + "changeemailmodern",
                    new ChangeEmailRequest()
                    {
                        NewEmail = Email.Text.Trim()
                    });

                if (response.Error != null)
                {
                    SetError(response.Error);
                }

                else
                {
                    try
                    {
                        EmailChanged?.Invoke(this, Email.Text.Trim());
                    }
                    catch { }

                    GoBack();
                }
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
