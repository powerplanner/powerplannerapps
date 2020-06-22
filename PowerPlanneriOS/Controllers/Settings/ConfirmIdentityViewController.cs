using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class ConfirmIdentityViewController : PopupViewControllerWithScrolling<ConfirmIdentityViewModel>
    {
        private BareUIVisibilityContainer _incorrectViewContainer;

        public ConfirmIdentityViewController()
        {
            Title = "Confirm Identity";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            AddTopSectionDivider();

            AddSpacing(8);
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Please confirm your identity by entering your Power Planner account password.",
                Lines = 0
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);
            AddSpacing(8);

            AddDivider();

            _incorrectViewContainer = new BareUIVisibilityContainer()
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
                var labelIncorrect = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Incorrect password. Please try again.",
                    Lines = 0,
                    TextColor = UIColor.Red
                };
                stackViewIncorrect.AddArrangedSubview(labelIncorrect);
                labelIncorrect.StretchWidth(stackViewIncorrect, left: 16, right: 16);
                stackViewIncorrect.AddArrangedSubview(new UIView() { TranslatesAutoresizingMaskIntoConstraints = false }.SetHeight(8));

                AddDivider(stackViewIncorrect);

                _incorrectViewContainer.Child = stackViewIncorrect;
            }
            StackView.AddArrangedSubview(_incorrectViewContainer);
            _incorrectViewContainer.StretchWidth(StackView);

            var textField = new UITextField()
            {
                Placeholder = "Current password",
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Done
            };
            textField.AddTarget(new WeakEventHandler<EventArgs>(delegate
            {
                _incorrectViewContainer.IsVisible = false;
            }).Handler, UIControlEvent.EditingChanged);
            AddTextField(textField, nameof(ViewModel.Password), firstResponder: true);

            AddSectionDivider();

            var buttonContinue = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonContinue.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.Continue(); }).Handler;
            buttonContinue.SetTitle("Continue", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonContinue);
            buttonContinue.StretchWidth(StackView);
            buttonContinue.SetHeight(44);

            if (ViewModel.ShowForgotPassword)
            {
                AddDivider(fullWidth: true);

                var forgotViews = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = ColorResources.InputSectionDividers
                };

                {
                    var buttonForgotPassword = new UIButton(UIButtonType.System)
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                        Font = UIFont.PreferredCaption1
                    };
                    buttonForgotPassword.TouchUpInside += new WeakEventHandler(delegate { ViewModel.ForgotPassword(); }).Handler;
                    buttonForgotPassword.SetTitle("Forgot Password", UIControlState.Normal);
                    forgotViews.Add(buttonForgotPassword);
                    buttonForgotPassword.StretchWidth(forgotViews);
                    buttonForgotPassword.StretchHeight(forgotViews, top: 16, bottom: 16);
                }

                StackView.AddArrangedSubview(forgotViews);
                forgotViews.StretchWidth(StackView);
                forgotViews.SetHeight(44);
            }

            else
            {
                AddBottomSectionDivider();
            }

            ViewModel.ActionIncorrectPassword += new WeakEventHandler(ViewModel_ActionIncorrectPassword).Handler;

            base.OnViewModelLoadedOverride();
        }

        private void ViewModel_ActionIncorrectPassword(object sender, EventArgs e)
        {
            _incorrectViewContainer.IsVisible = true;
        }
    }
}