using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class UpdateCredentialsViewController : PopupViewControllerWithScrolling<UpdateCredentialsViewModel>
    {
        private BareUIVisibilityContainer _errorContainer;
        private UILabel _labelError;

        public UpdateCredentialsViewController()
        {
            Title = "Update Credentials";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            AddTopSectionDivider();

            AddSpacing(8);
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Your online account's username or password has changed. Please log back in to continue using your online account.",
                Lines = 0
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);
            AddSpacing(8);

            AddDivider();

            _errorContainer = new BareUIVisibilityContainer()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                IsVisible = false
            };
            {
                var stackViewIncorrect = new UIStackView()
                {
                    Axis = UILayoutConstraintAxis.Vertical
                };
                stackViewIncorrect.AddArrangedSubview(new UIView() { TranslatesAutoresizingMaskIntoConstraints = false }.SetHeight(8));
                _labelError = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Error",
                    Lines = 0,
                    TextColor = UIColor.Red
                };
                stackViewIncorrect.AddArrangedSubview(_labelError);
                _labelError.StretchWidth(stackViewIncorrect, left: 16, right: 16);
                stackViewIncorrect.AddArrangedSubview(new UIView() { TranslatesAutoresizingMaskIntoConstraints = false }.SetHeight(8));

                AddDivider(stackViewIncorrect);

                _errorContainer.Child = stackViewIncorrect;
            }
            StackView.AddArrangedSubview(_errorContainer);
            _errorContainer.StretchWidth(StackView);

            var textFieldUsername = new UITextField()
            {
                Placeholder = "Username",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                KeyboardType = UIKeyboardType.ASCIICapable,
                ReturnKeyType = UIReturnKeyType.Next
            };
            textFieldUsername.AddTarget(new WeakEventHandler<EventArgs>(delegate
            {
                _errorContainer.IsVisible = false;
            }).Handler, UIControlEvent.EditingChanged);
            AddTextField(textFieldUsername, nameof(ViewModel.Username));

            AddDivider();

            var textFieldPassword = new UITextField()
            {
                Placeholder = "Password",
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Done
            };
            textFieldPassword.AddTarget(new WeakEventHandler<EventArgs>(delegate
            {
                _errorContainer.IsVisible = false;
            }).Handler, UIControlEvent.EditingChanged);
            AddTextField(textFieldPassword, nameof(ViewModel.Password), firstResponder: true);

            AddSectionDivider();

            var buttonLogin = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonLogin.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.LogIn(); }).Handler;
            buttonLogin.SetTitle("Log In", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonLogin);
            buttonLogin.StretchWidth(StackView);
            buttonLogin.SetHeight(44);

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

            BindingHost.SetBinding(nameof(ViewModel.Error), delegate
            {
                if (string.IsNullOrWhiteSpace(ViewModel.Error))
                {
                    _errorContainer.IsVisible = false;
                }
                else
                {
                    _errorContainer.IsVisible = true;
                    _labelError.Text = ViewModel.Error;
                }
            });

            BindingHost.SetBinding(nameof(ViewModel.IsLoggingIn), delegate
            {
                IsLoadingOverlayVisible = ViewModel.IsLoggingIn;
            });

            base.OnViewModelLoadedOverride();
        }
    }
}