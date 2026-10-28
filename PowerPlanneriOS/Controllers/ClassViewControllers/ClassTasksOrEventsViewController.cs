using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using System.Collections.Specialized;
using ToolsPortable;
using PowerPlanneriOS.Views;
using System.ComponentModel;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlanneriOS.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Helpers;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassTasksOrEventsViewController : BareMvvmUIViewController<ClassTasksOrEventsViewModel>
    {
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