using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using ToolsPortable;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers.Welcome
{
    public class ResetPasswordViewController : PopupViewControllerWithScrolling<ResetPasswordViewModel>
    {
        public ResetPasswordViewController()
        {
            Title = "Forgot Password";

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
                Placeholder = "Email address",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                KeyboardType = UIKeyboardType.EmailAddress,
                ReturnKeyType = UIReturnKeyType.Go,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Email));

            AddSectionDivider();

            var buttonResetPassword = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonResetPassword.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ResetPassword(); }).Handler;
            buttonResetPassword.SetTitle("Reset password", UIControlState.Normal);
            StackView.AddArrangedSubview(buttonResetPassword);
            buttonResetPassword.StretchWidth(StackView);
            buttonResetPassword.SetHeight(44);

            AddBottomSectionDivider();

            BindingHost.SetBinding(nameof(ViewModel.IsResettingPassword), delegate
            {
                if (ViewModel.IsResettingPassword)
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
                ResetPassword();
                return true;
            }

            return base.HandleKeyboardAction(returnKeyType);
        }

        private void ResetPassword()
        {
            ViewModel.ResetPassword();
        }
    }
}