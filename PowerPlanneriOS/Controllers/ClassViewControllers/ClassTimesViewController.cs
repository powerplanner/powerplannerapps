using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using Vx.Extensions;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassTimesViewController : BareMvvmUIViewController<ClassTimesViewModel>
    {
        public ClassTimesViewController()
        {
            Title = PowerPlannerResources.GetString("ClassPage_PivotItemTimes.Header");
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            var rendered = ViewModel.Render();
            rendered.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(rendered);
            rendered.StretchWidthAndHeight(View);
        }
    }
}