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
        private BareUIStaticGroupedTableView _tableView;

        public CreateAccountViewController()
        {
            Title = "Create Account";

            _tableView = new BareUIStaticGroupedTableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ScrollEnabled = false
            };

            StackView.AddArrangedSubview(_tableView);
            _tableView.StretchWidthAndHeight(View);
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.IsUsingConfirmPassword = false;

            _tableView.AddTextFieldCell("Username", BindingHost, nameof(ViewModel.Username), textField =>
            {
                textField.AutocapitalizationType = UITextAutocapitalizationType.None;
                textField.AutocorrectionType = UITextAutocorrectionType.No;
                textField.KeyboardType = UIKeyboardType.ASCIICapable;
                textField.ReturnKeyType = UIReturnKeyType.Next;
                textField.EnablesReturnKeyAutomatically = true;
                textField.BecomeFirstResponder();
            });

            _tableView.AddTextFieldCell("Email", BindingHost, nameof(ViewModel.Email), textField =>
            {
                textField.AutocapitalizationType = UITextAutocapitalizationType.None;
                textField.AutocorrectionType = UITextAutocorrectionType.Yes;
                textField.KeyboardType = UIKeyboardType.EmailAddress;
                textField.ReturnKeyType = UIReturnKeyType.Next;
                textField.EnablesReturnKeyAutomatically = true;
            });

            _tableView.AddTextFieldCell("Password", BindingHost, nameof(ViewModel.Password), textField =>
            {
                textField.SecureTextEntry = true;
                textField.ReturnKeyType = UIReturnKeyType.Go;
            });

            _tableView.Compile();

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

            if (ViewModel.IsCreateLocalAccountVisible)
            {
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
            }

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