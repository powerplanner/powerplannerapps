using InterfacesiOS.ViewModelPresenters;
using PowerPlanneriOS.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassGradesViewController: PagedViewModelPresenter
    {
        public ConfigureClassGradesViewController()
        {
            AutomaticallyAdjustsScrollViewInsets = false;

            // Only purpose of this is to show the navigation bar, otherwise this is just a normal paged view model host
            MyNavigationController.NavigationBarHidden = false;

            ColorResources.ConfigureNavBar(MyNavigationController.NavigationBar);
        }
    }
}
