using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassDetailsViewController : BareMvvmUIViewControllerWithScrolling<ClassDetailsViewModel>
    {
        public ClassDetailsViewController()
        {
            Title = PowerPlannerResources.GetString("ClassPage_PivotItemDetails.Header");

            var textViewDetails = new UITextView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredBody,
                Editable = false,
                ScrollEnabled = false,

                // Link detection: http://iosdevelopertips.com/user-interface/creating-clickable-hyperlinks-from-a-url-phone-number-or-address.html
                DataDetectorTypes = UIDataDetectorType.All
            };

            // Lose the padding: https://stackoverflow.com/questions/746670/how-to-lose-margin-padding-in-uitextview
            textViewDetails.TextContainerInset = UIEdgeInsets.Zero;
            textViewDetails.TextContainer.LineFragmentPadding = 0;

            BindingHost.SetTextViewTextBinding(textViewDetails, nameof(ViewModel.Details));
            StackView.AddArrangedSubview(textViewDetails);
            textViewDetails.StretchWidth(StackView);

            var labelNothingHere = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
                Font = UIFont.PreferredCallout,
                Text = PowerPlannerResources.GetString("ClassPage_Details_NothingHereString"),
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