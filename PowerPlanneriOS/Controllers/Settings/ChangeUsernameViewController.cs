using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Views;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class ChangeUsernameViewController : PopupViewControllerWithScrolling<ChangeUsernameViewModel>
    {
        private BareUIVisibilityContainer _errorContainer;
        private UILabel _labelError;

        public ChangeUsernameViewController()
        {
            Title = "Change Username";

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
                Placeholder = "New username",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                KeyboardType = UIKeyboardType.ASCIICapable,
                ReturnKeyType = UIReturnKeyType.Done
            };
            textField.AddTarget(new WeakEventHandler<EventArgs>(delegate
            {
                _errorContainer.IsVisible = false;
            }).Handler, UIControlEvent.EditingChanged);
            AddTextField(textField, nameof(ViewModel.Username), firstResponder: true);

            AddSectionDivider();

            var buttonContinue = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonContinue.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.Update(); }).Handler;
            buttonContinue.SetTitle("Update Username", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonContinue);
            buttonContinue.StretchWidth(StackView);
            buttonContinue.SetHeight(44);

            AddBottomSectionDivider();

            ViewModel.ActionError += new WeakEventHandler<string>(ViewModel_ActionError).Handler;

            BindingHost.SetBinding(nameof(ViewModel.IsUpdatingUsername), delegate
            {
                IsLoadingOverlayVisible = ViewModel.IsUpdatingUsername;
            });

            base.OnViewModelLoadedOverride();
        }

        private void ViewModel_ActionError(object sender, string e)
        {
            _errorContainer.IsVisible = true;
            _labelError.Text = e;
        }
    }
}