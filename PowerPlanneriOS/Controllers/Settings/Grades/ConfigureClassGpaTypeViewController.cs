using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Binding;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassGpaTypeViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassGpaTypeViewModel>
    {
        public ConfigureClassGpaTypeViewController()
        {
            Title = PowerPlannerResources.GetString("Settings_GradeOptions_GpaType");
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(renderedComponent);
            renderedComponent.StretchWidthAndHeight(View);
        }
    }
}