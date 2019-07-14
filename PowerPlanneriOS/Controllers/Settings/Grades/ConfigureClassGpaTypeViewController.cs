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

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassGpaTypeViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassGpaTypeViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;

        public ConfigureClassGpaTypeViewController()
        {
            Title = PowerPlannerResources.GetString("Settings_GradeOptions_GpaType");

            _tableView = new BareUIStaticGroupedTableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_tableView);
            _tableView.StretchWidthAndHeight(View);
        }

        public override void OnViewModelLoadedOverride()
        {
            BindingHost.SetIsEnabledBinding(_tableView, nameof(ViewModel.IsEnabled));

            _tableView.AddCheckableCellWithDescription(PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_Standard.Text"), PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_StandardExplanation.Text"), BindingHost, nameof(ViewModel.IsStandard));

            _tableView.AddCheckableCellWithDescription(PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_PassFail.Text"), PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_PassFailExplanation.Text"), BindingHost, nameof(ViewModel.IsPassFail));

            _tableView.Compile();

            base.OnViewModelSetOverride();
        }
    }
}