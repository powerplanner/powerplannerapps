using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassDetailsViewController : BareMvvmUIViewControllerWithScrolling<ClassDetailsViewModel>
    {
        public ClassDetailsViewController()
        {
            Title = "Details";

            var labelDetails = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
                Font = UIFont.PreferredBody
            };
            BindingHost.SetLabelTextBinding(labelDetails, nameof(ViewModel.Details));
            StackView.AddArrangedSubview(labelDetails);
            labelDetails.StretchWidth(StackView);

            var labelNothingHere = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
                Font = UIFont.PreferredCallout,
                Text = "Tap the edit button in the top right to add details like the teacher's contact info, office hours, and anything else!",
                TextColor = UIColor.LightGray,
                TextAlignment = UITextAlignment.Center
            };
            View.Add(labelNothingHere);
            labelNothingHere.StretchWidth(View, left: 16, right: 16);
            labelNothingHere.StretchHeight(View, top: 16, bottom: 16);
            BindingHost.SetBinding(nameof(ViewModel.Details), delegate
            {
                labelNothingHere.Hidden = !string.IsNullOrWhiteSpace(ViewModel.Details);
            });
        }

        protected override int TopPadding => 16;
        protected override int LeftPadding => 16;
        protected override int RightPadding => 16;
    }
}