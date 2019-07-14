using Foundation;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlanneriOS.Controllers;
using PowerPlanneriOS.Helpers;
using System;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS
{
    public partial class CreateAccountViewController : PopupViewControllerWithScrolling<CreateAccountViewModel>
    {
        public CreateAccountViewController()
        {
            Title = "Create Account";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Username",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                KeyboardType = UIKeyboardType.ASCIICapable,
                ReturnKeyType = UIReturnKeyType.Next,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Username), firstResponder: true);

            AddDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Email address (for recovery purposes)",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                KeyboardType = UIKeyboardType.EmailAddress,
                ReturnKeyType = UIReturnKeyType.Next,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Email));

            AddDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Password",
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Next,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Password));

            AddDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Confirm password",
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Go
            }, nameof(ViewModel.ConfirmPassword));

            AddSectionDivider();

            var buttonCreateAccount = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonCreateAccount.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { CreateAccount(); }).Handler;
            buttonCreateAccount.SetTitle("Create Account", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonCreateAccount);
            buttonCreateAccount.StretchWidth(StackView);
            buttonCreateAccount.SetHeight(44);

            AddDivider(fullWidth: true);

            var bottomView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = ColorResources.InputSectionDividers
            };
            {
                var buttonCreateLocalAccount = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1
                };
                buttonCreateLocalAccount.TouchUpInside += new WeakEventHandler(ButtonCreateLocalAccount_TouchUpInside).Handler;
                buttonCreateLocalAccount.SetTitle("No internet? Create offline account", UIControlState.Normal);
                bottomView.Add(buttonCreateLocalAccount);
                buttonCreateLocalAccount.PinToRight(bottomView, right: 16);
                buttonCreateLocalAccount.StretchHeight(bottomView, top: 8);
            }
            StackView.AddArrangedSubview(bottomView);
            bottomView.StretchWidth(StackView);

            BindingHost.SetBinding(nameof(ViewModel.IsCreatingOnlineAccount), delegate
            {
                if (ViewModel.IsCreatingOnlineAccount)
                {
                    ShowLoadingOverlay(loadingText: "Creating account...");
                }
                else
                {
                    HideLoadingOverlay();
                }
            });

            base.OnViewModelLoadedOverride();
        }

        protected override bool HandleKeyboardAction(UIReturnKeyType returnKeyType)
        {
            if (returnKeyType == UIReturnKeyType.Go)
            {
                CreateAccount();
                return true;
            }

            return base.HandleKeyboardAction(returnKeyType);
        }

        private void CreateAccount()
        {
            ViewModel.CreateAccount();
        }

        private void ButtonCreateLocalAccount_TouchUpInside(object sender, EventArgs eArgs)
        {
            IUIAlertViewDelegate del = null;
            var alertView = new UIAlertView(PowerPlannerResources.GetString("CreateAccountPage_String_WarningOfflineAccount"), PowerPlannerResources.GetString("CreateAccountPage_String_WarningOfflineAccountExplanation").Replace("Windows Phone", "other devices"), del, PowerPlannerResources.GetString("String_GoBack"), PowerPlannerResources.GetString("String_Create"));
            alertView.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == 1)
                {
                    ViewModel.CreateLocalAccount();
                }
            };
            alertView.Show();
        }
    }
}