using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class ConvertToOnlineViewController : PopupViewControllerWithScrolling<ConvertToOnlineViewModel>
    {
        private BareUIVisibilityContainer _errorContainer;
        private UILabel _labelError;

        public ConvertToOnlineViewController()
        {
            Title = "Convert to Online";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

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
                Placeholder = "Provide your email",
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

            var buttonConvert = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonConvert.TouchUpInside += new WeakEventHandler<EventArgs>(async delegate
            {
                ShowLoadingOverlay();
                await ViewModel.CreateOnlineAccountAsync();
                HideLoadingOverlay();
            }).Handler;
            buttonConvert.SetTitle("Convert to Online Account", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonConvert);
            buttonConvert.StretchWidth(StackView);
            buttonConvert.SetHeight(44);

            var mergeExistingContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                mergeExistingContainer.AddSectionDivider();

                mergeExistingContainer.AddSpacing(16);
                var labelMergeExistingExplanation = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = PowerPlannerResources.GetString("Settings_ConvertToOnline_TextBlockConfirmMergeExisting.Text"),
                    Lines = 0,
                    TextColor = UIColor.Red
                };
                mergeExistingContainer.AddArrangedSubview(labelMergeExistingExplanation);
                labelMergeExistingExplanation.StretchWidth(mergeExistingContainer, left: 16, right: 16);
                mergeExistingContainer.AddSpacing(16);

                mergeExistingContainer.AddDivider();

                var buttonContinue = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TintColor = UIColor.Red
                };
                buttonContinue.TouchUpInside += new WeakEventHandler<EventArgs>(async delegate
                {
                    ShowLoadingOverlay();
                    await ViewModel.MergeExisting();
                    HideLoadingOverlay();
                }).Handler;
                buttonContinue.SetTitle(PowerPlannerResources.GetString("Settings_ConfirmIdentityPage_ButtonContinue.Content"), UIControlState.Normal);
                mergeExistingContainer.AddArrangedSubview(buttonContinue);
                buttonContinue.StretchWidth(mergeExistingContainer);
                buttonContinue.SetHeight(44);

                mergeExistingContainer.AddDivider();

                var buttonCancel = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                buttonCancel.TouchUpInside += new WeakEventHandler<EventArgs>(delegate
                {
                    ViewModel.CancelMergeExisting();
                }).Handler;
                buttonCancel.SetTitle(PowerPlannerResources.GetString("Buttons_Cancel.Content"), UIControlState.Normal);
                mergeExistingContainer.AddArrangedSubview(buttonCancel);
                buttonCancel.StretchWidth(mergeExistingContainer);
                buttonCancel.SetHeight(44);
            }
            AddUnderVisiblity(mergeExistingContainer, nameof(ViewModel.ShowConfirmMergeExisting));

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

            base.OnViewModelLoadedOverride();
        }
    }
}