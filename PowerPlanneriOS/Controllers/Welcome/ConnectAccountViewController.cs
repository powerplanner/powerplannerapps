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
    public class ConnectAccountViewController : PopupViewControllerWithScrolling<ConnectAccountViewModel>
    {
        public ConnectAccountViewController()
        {
            Title = "Connect Account";
        }

        public override void OnViewModelLoadedOverride()
        {
            AddSpacing(16);

            var label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetString("Welcome_ConnectAccountPage_Message.Text"),
                Lines = 0 // Wraps text
            };
            StackView.AddArrangedSubview(label);
            label.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(24);

            var buttonLogIn = PowerPlannerUIHelper.CreatePowerPlannerBlueButton("Log In");
            buttonLogIn.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonLogIn.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.LogIn(); }).Handler;
            StackView.AddArrangedSubview(buttonLogIn);
            buttonLogIn.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(6);

            var labelHelp = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetString("Welcome_ConnectAccountPage_NeedHelp.Text"),
                Lines = 0,
                Font = UIFont.PreferredCaption1
            };
            StackView.AddArrangedSubview(labelHelp);
            labelHelp.StretchWidth(StackView, left: 16, right: 16);

            AddSpacing(16);

            base.OnViewModelLoadedOverride();
        }
    }
}