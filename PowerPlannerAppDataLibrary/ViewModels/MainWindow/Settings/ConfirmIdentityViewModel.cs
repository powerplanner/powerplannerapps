using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ConfirmIdentityViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;
        public override bool ImportantForAutofill => true;

        private AccountDataItem _currAccount;
        public event EventHandler OnIdentityConfirmed;
        public event EventHandler ActionIncorrectPassword;

        public bool ShowForgotPassword { get; private set; }

        public ConfirmIdentityViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            _currAccount = account;

            if (_currAccount == null)
            {
                throw new InvalidOperationException("There's no current account.");
            }

            ShowForgotPassword = account.IsOnlineAccount;
            Title = PowerPlannerResources.GetString("Settings_ConfirmIdentityPage.Title");
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
                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("Settings_ConfirmIdentitiyPage_Description.Text")
                        },

                        new PasswordBox
                        {
                            Text = VxValue.Create(Password, t => { Password = t; IncorrectPassword = false; }),
                            Header = PowerPlannerResources.GetString("LoginPage_TextBoxPassword.Header"),
                            PlaceholderText = PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_TextBoxCurrentPassword.PlaceholderText"),
                            Margin = new Thickness(0, 12, 0, 0),
                            AutoFocus = true,
                            OnSubmit = Continue,
                            ValidationState = IncorrectPassword ? InputValidationState.Invalid(PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_Errors_IncorrectPassword")) : null
                        },

                        ShowForgotPassword ? new TextButton
                        {
                            Text = PowerPlannerResources.GetString("LoginPage_TextBlockForgotPassword.Text"),
                            Margin = new Thickness(0, 6, 0, 0),
                            Click = ForgotPassword,
                            HorizontalAlignment = HorizontalAlignment.Left
                        } : null,

                        new AccentButton
                        {
                            Text = PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_ButtonContinue.Content"),
                            Click = Continue,
                            Margin = new Thickness(0, 12, 0, 0)
                        }
                    }
                }
            };
        }

        public string Password { get => GetState<string>(); set => SetState(value); }


        public bool IncorrectPassword { get => GetState<bool>(); set => SetState(value); }

        public void Continue()
        {
            if (PowerPlannerAuth.ValidatePasswordLocally(Password, _currAccount.Username, _currAccount.LocalToken))
            {
                IncorrectPassword = false;

                OnIdentityConfirmed?.Invoke(this, new EventArgs());

                RemoveViewModel();
            }

            else
            {
                IncorrectPassword = true;
            }
        }

        public void ForgotPassword()
        {
            ShowPopup(new ResetPasswordViewModel(GetPopupViewModelHost(), _currAccount.Username));
        }
    }
}
