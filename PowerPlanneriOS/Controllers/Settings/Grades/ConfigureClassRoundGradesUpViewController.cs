using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassRoundGradesUpViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassRoundGradesUpViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;

        public ConfigureClassRoundGradesUpViewController()
        {
            Title = "Round Grades Up";

            _tableView = new BareUIStaticGroupedTableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_tableView);
            _tableView.StretchWidthAndHeight(View);
        }

        public override void OnViewModelSetOverride()
        {
            var uiSwitch = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Round Grades Up"
            };
            BindingHost.SetSwitchBinding(uiSwitch, nameof(ViewModel.RoundGradesUp));
            BindingHost.SetIsEnabledBinding(uiSwitch, nameof(ViewModel.IsEnabled));
            var cell = new BareUITableViewCell("roundGradesUp");
            cell.ContentView.Add(uiSwitch);
            uiSwitch.StretchWidthAndHeight(cell.ContentView);
            _tableView.AddCell(cell, delegate { }); // Have to provide action so it remains clickable

            _tableView.AddCaptionDescriptionCell(PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpBody.Text"));

            _tableView.Compile();

            base.OnViewModelSetOverride();
        }
    }
}
