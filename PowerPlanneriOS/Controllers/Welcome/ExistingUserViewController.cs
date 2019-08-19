using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers.Welcome
{
    public class ExistingUserViewController : PopupViewControllerWithScrolling<ExistingUserViewModel>
    {
        public ExistingUserViewController()
        {
            Title = "Existing User";
        }

        public override void OnViewModelLoadedOverride()
        {
            AddSpacing(16);

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetString("Welcome_ExistingUserPage_Message.Text"),
                Lines = 0 // Wraps text
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(24);

            var buttonHasAccount = PowerPlannerUIHelper.CreatePowerPlannerBlueButton(PowerPlannerResources.GetString("Welcome_ExistingUserPage_ButtonHasAccount.Content"));
            buttonHasAccount.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonHasAccount.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.HasAccount(); }).Handler;
            StackView.AddArrangedSubview(buttonHasAccount);
            buttonHasAccount.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(8);

            var buttonNoAccount = PowerPlannerUIHelper.CreatePowerPlannerBlueButton(PowerPlannerResources.GetString("Welcome_ExistingUserPage_ButtonNoAccount.Content"));
            buttonNoAccount.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonNoAccount.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.NoAccount(); }).Handler;
            StackView.AddArrangedSubview(buttonNoAccount);
            buttonNoAccount.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(16);

            base.OnViewModelLoadedOverride();
        }
    }
}