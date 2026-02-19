using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.ViewModelPresenters;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class SettingsViewController : PagedViewModelPresenter
    {
        public SettingsViewController()
        {
            // Only purpose of this is to show the navigation bar, otherwise this is just a normal paged view model host
            MyNavigationController.NavigationBarHidden = false;

            ColorResources.ConfigureNavBar(MyNavigationController.NavigationBar);
        }
    }
}