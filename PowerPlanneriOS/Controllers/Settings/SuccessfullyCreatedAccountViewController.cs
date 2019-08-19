using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class SuccessfullyCreatedAccountViewController : PopupViewControllerWithScrolling<SuccessfullyCreatedAccountViewModel>
    {
        public SuccessfullyCreatedAccountViewController()
        {
            Title = "Account Created";
        }

        public override void OnViewModelLoadedOverride()
        {
            AddSpacing(16);

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetString("Settings_SuccessfullyCreatedAccountPage_Message.Text"),
                Lines = 0 // Wraps text
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);

            AddHeader(PowerPlannerResources.GetString("TextBox_Username.Header"));
            AddText(ViewModel.Username);

            AddHeader(PowerPlannerResources.GetString("CreateAccountPage_TextBoxEmail.Header"));
            AddText(ViewModel.Email);

            AddSpacing(24);

            var buttonContinue = PowerPlannerUIHelper.CreatePowerPlannerBlueButton("Continue");
            buttonContinue.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonContinue.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.Continue(); }).Handler;
            StackView.AddArrangedSubview(buttonContinue);
            buttonContinue.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(16);

            base.OnViewModelLoadedOverride();
        }

        private void AddHeader(string text)
        {
            AddSpacing(16);

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = text,
                Font = UIFont.PreferredBody.Bold()
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);
        }

        private void AddText(string text)
        {
            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = text,
                Lines = 0 // Wrap text
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);
        }
    }
}