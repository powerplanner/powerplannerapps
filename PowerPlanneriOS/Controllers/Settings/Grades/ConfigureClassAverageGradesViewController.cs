using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassAverageGradesViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassAverageGradesViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;

        public ConfigureClassAverageGradesViewController()
        {
            Title = "Average Grades";

            _tableView = new BareUIStaticGroupedTableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_tableView);
            _tableView.StretchWidthAndHeight(View);
        }

        public override void OnViewModelSetOverride()
        {
            var switchAverageGrades = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Average Grades"
            };
            BindingHost.SetSwitchBinding(switchAverageGrades, nameof(ViewModel.AverageGrades));
            BindingHost.SetIsEnabledBinding(switchAverageGrades, nameof(ViewModel.IsEnabled));
            var cellAverageGrades = new BareUITableViewCell("averageGrades");
            cellAverageGrades.ContentView.Add(switchAverageGrades);
            switchAverageGrades.StretchWidthAndHeight(cellAverageGrades.ContentView);
            _tableView.AddCell(cellAverageGrades, delegate { }); // Have to provide action so it remains clickable

            _tableView.AddCaptionDescriptionCell(PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpBody.Text"));

            _tableView.Compile();

            base.OnViewModelSetOverride();
        }
    }
}
