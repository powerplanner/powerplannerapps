using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using InterfacesiOS.Views;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers.Welcome
{
    public class ForgotUsernameViewController : PopupViewControllerWithScrolling<ForgotUsernameViewModel>
    {
        public ForgotUsernameViewController()
        {
            Title = "Forgot Username";

            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            AddTopSectionDivider();

            AddSpacing(8);
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Enter your email address to recover your username.",
                Lines = 0
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);
            AddSpacing(8);

            AddDivider();

            var textField = new UITextField()
            {
                Placeholder = "Email address",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                KeyboardType = UIKeyboardType.EmailAddress,
                ReturnKeyType = UIReturnKeyType.Go,
                EnablesReturnKeyAutomatically = true
            };
            AddTextField(textField, nameof(ViewModel.Email), firstResponder: true);

            AddSectionDivider();

            var buttonRecover = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonRecover.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { Recover(); }).Handler;
            buttonRecover.SetTitle("Recover", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonRecover);
            buttonRecover.StretchWidth(StackView);
            buttonRecover.SetHeight(44);

            AddBottomSectionDivider();

            BindingHost.SetBinding(nameof(ViewModel.IsRecoveringUsernames), delegate
            {
                if (ViewModel.IsRecoveringUsernames)
                {
                    ShowLoadingOverlay(false);
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
                Recover();
                return true;
            }

            return base.HandleKeyboardAction(returnKeyType);
        }

        private void Recover()
        {
            ViewModel.Recover();
        }
    }
}