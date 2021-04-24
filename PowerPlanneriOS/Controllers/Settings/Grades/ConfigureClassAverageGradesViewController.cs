using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassAverageGradesViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassAverageGradesViewModel>
    {
        public ConfigureClassAverageGradesViewController()
        {
            Title = "Average Grades";
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(renderedComponent);
            renderedComponent.StretchWidthAndHeight(View);
        }
    }
}
