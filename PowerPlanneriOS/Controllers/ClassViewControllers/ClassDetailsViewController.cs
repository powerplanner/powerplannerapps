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
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassDetailsViewController : BareMvvmUIViewController<ClassDetailsViewModel>
    {
        private object _tabBarHeightListener;

        public ClassDetailsViewController()
        {
            Title = PowerPlannerResources.GetString("ClassPage_PivotItemDetails.Header");
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            var rendered = ViewModel.Render();
            rendered.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(rendered);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                rendered.RemoveAllConstraints();
                rendered.StretchWidthAndHeight(View, 0, 0, 0, (float)MainScreenViewController.TAB_BAR_HEIGHT);
            });
        }
    }
}