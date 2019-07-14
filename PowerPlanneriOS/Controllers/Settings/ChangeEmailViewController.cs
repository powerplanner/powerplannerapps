using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using ToolsPortable;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class ChangeEmailViewController : PopupViewControllerWithScrolling<ChangeEmailViewModel>
    {
        private BareUIVisibilityContainer _errorContainer;
        private UILabel _labelError;

        public ChangeEmailViewController()
        {
            Title = "Change Email";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            AddTopSectionDivider();

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

            var textField = new UITextField()
            {
                Placeholder = "New email",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                KeyboardType = UIKeyboardType.EmailAddress,
                ReturnKeyType = UIReturnKeyType.Done
            };
            textField.AddTarget(new WeakEventHandler<EventArgs>(delegate
            {
                _errorContainer.IsVisible = false;
            }).Handler, UIControlEvent.EditingChanged);
            AddTextField(textField, nameof(ViewModel.Email), firstResponder: true);

            AddSectionDivider();

            var buttonContinue = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonContinue.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.UpdateEmail(); }).Handler;
            buttonContinue.SetTitle("Update Email", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonContinue);
            buttonContinue.StretchWidth(StackView);
            buttonContinue.SetHeight(44);

            AddBottomSectionDivider();

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

            BindingHost.SetBinding(nameof(ViewModel.IsUpdatingEmail), UpdateLoadingOverlay);
            BindingHost.SetBinding(nameof(ViewModel.IsRetrievingEmail), UpdateLoadingOverlay);

            base.OnViewModelLoadedOverride();
        }

        private void UpdateLoadingOverlay()
        {
            if (ViewModel.IsRetrievingEmail)
            {
                ShowLoadingOverlay(coverTopNavBar: false);
            }
            else if (ViewModel.IsUpdatingEmail)
            {
                ShowLoadingOverlay();
            }
            else
            {
                HideLoadingOverlay();
            }
        }
    }
}