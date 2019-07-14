using Foundation;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlanneriOS.Controllers;
using System;
using ToolsPortable;
using UIKit;
using InterfacesiOS.Views;
using System.ComponentModel;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Welcome
{
    public partial class LoginViewController : PopupViewControllerWithScrolling<LoginViewModel>
    {
        private UIButton _buttonLogin;

        public LoginViewController()
        {
            Title = "Log In";

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
                Placeholder = "Password",
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Go,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Password));

            AddSectionDivider();

            _buttonLogin = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _buttonLogin.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { Login(); }).Handler;
            _buttonLogin.SetTitle("Log In", UIControlState.Normal);
            StackView.AddArrangedSubview(_buttonLogin);
            _buttonLogin.StretchWidth(StackView);
            _buttonLogin.SetHeight(44);

            AddDivider(fullWidth: true);

            var forgotViews = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = ColorResources.InputSectionDividers
            };
            {
                var leftSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                forgotViews.Add(leftSpacer);
                var rightSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                forgotViews.Add(rightSpacer);

                var buttonForgotUsername = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HorizontalAlignment = UIControlContentHorizontalAlignment.Right,
                    Font = UIFont.PreferredCaption1
                };
                buttonForgotUsername.TouchUpInside += new WeakEventHandler(delegate { ViewModel.ForgotUsername(); }).Handler;
                buttonForgotUsername.SetTitle("Forgot Username", UIControlState.Normal);
                forgotViews.Add(buttonForgotUsername);
                buttonForgotUsername.StretchHeight(forgotViews);

                var verticalDivider = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = UIColor.FromWhiteAlpha(0.7f, 1)
                };
                forgotViews.Add(verticalDivider);
                verticalDivider.StretchHeight(forgotViews, top: 16, bottom: 16);

                var buttonForgotPassword = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HorizontalAlignment = UIControlContentHorizontalAlignment.Left,
                    Font = UIFont.PreferredCaption1
                };
                buttonForgotPassword.TouchUpInside += new WeakEventHandler(delegate { ViewModel.ForgotPassword(); }).Handler;
                buttonForgotPassword.SetTitle("Forgot Password", UIControlState.Normal);
                forgotViews.Add(buttonForgotPassword);
                buttonForgotPassword.StretchHeight(forgotViews);

                forgotViews.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[leftSpacer][forgotUsername]-8-[verticalDivider(1)]-8-[forgotPassword][rightSpacer(==leftSpacer)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                    "forgotUsername", buttonForgotUsername,
                    "verticalDivider", verticalDivider,
                    "forgotPassword", buttonForgotPassword,
                    "leftSpacer", leftSpacer,
                    "rightSpacer", rightSpacer)));
            }
            StackView.AddArrangedSubview(forgotViews);
            forgotViews.StretchWidth(StackView);
            forgotViews.SetHeight(44);

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            base.OnViewModelLoadedOverride();
        }

        protected override bool HandleKeyboardAction(UIReturnKeyType returnKeyType)
        {
            if (returnKeyType == UIReturnKeyType.Go)
            {
                Login();
                return true;
            }

            return base.HandleKeyboardAction(returnKeyType);
        }

        private void Login()
        {
            var dontWait = ViewModel.LoginAsync();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsLoggingInOnline):
                case nameof(ViewModel.IsSyncingAccount):
                    UpdateStatus();
                    break;
            }
        }

        private void UpdateStatus()
        {
            if (ViewModel.IsLoggingInOnline)
            {
                ShowLoadingOverlay(loadingText: "Logging in...");
            }
            else if (ViewModel.IsSyncingAccount)
            {
                ShowLoadingOverlay(loadingText: "Syncing account...");
            }
            else
            {
                HideLoadingOverlay();
            }
        }
    }
}