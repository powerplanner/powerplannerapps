using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlanneriOS.ViewModels;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class AboutViewControllerAsPopup : PopupViewControllerWithScrolling<AboutViewModelAsPopup>
    {
        public AboutViewControllerAsPopup()
        {
            Title = "About";

            AboutViewController.ConfigureUI(StackView);
        }

        protected override int AdditionalTopPadding => 16;
        protected override int LeftPadding => 16;
        protected override int RightPadding => 16;
    }
}