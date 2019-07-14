using BareMvvm.Core.ViewModels;
using InterfacesiOS.Binding;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassGradesListViewController : BareMvvmUIViewController<ConfigureClassGradesListViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;

        private BindingHost m_classBindingHost = new BindingHost();

        public ConfigureClassGradesListViewController()
        {
            AutomaticallyAdjustsScrollViewInsets = false;

            // Set title to class name
            m_classBindingHost.SetBinding(nameof(ViewModel.Class.Name), delegate
            {
                Title = ViewModel.Class.Name;
            });

            // Creating a table view programmatically: https://developer.apple.com/library/content/documentation/UserExperience/Conceptual/TableView_iPhone/CreateConfigureTableView/CreateConfigureTableView.html#//apple_ref/doc/uid/TP40007451-CH6-SW4
            // Xamarin: https://developer.xamarin.com/guides/ios/user_interface/controls/tables/populating-a-table-with-data/
            _tableView = new BareUIStaticGroupedTableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_tableView);
            _tableView.StretchWidthAndHeight(View);

            var backButton = new UIBarButtonItem()
            {
                Title = "Back"
            };
            backButton.Clicked += new WeakEventHandler<EventArgs>(BackButton_Clicked).Handler;

            NavigationItem.LeftBarButtonItem = backButton;
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.FindAncestor<PagedViewModel>().RemoveViewModel();
        }

        public override void OnViewModelSetOverride()
        {
            m_classBindingHost.BindingObject = ViewModel.Class;

            ViewModel.Class.PropertyChanged += new WeakEventHandler<System.ComponentModel.PropertyChangedEventArgs>(Class_PropertyChanged).Handler;

            UpdateTable();

            base.OnViewModelSetOverride();
        }

        private void Class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Class.GpaType))
            {
                UpdateTable();
            }
        }

        private void UpdateTable()
        {
            _tableView.ClearAll();

            // Display summary info
            _tableView.AddDescriptionCell("Edit grade options for this class.");

            _tableView.StartNewGroup();

            // Add items
            _tableView.AddValueCell(PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header"),
                bindingHost: m_classBindingHost,
                bindingValuePropertyName: nameof(ViewModel.Class.Credits),
                converter: ConvertCreditsToString,
                invokeAction: ViewModel.ConfigureCredits);

            _tableView.AddCell(PowerPlannerResources.GetString("ConfigureClassGrades_Items_WeightCategories.Title"), ViewModel.ConfigureWeightCategories);

            _tableView.AddValueCell(PowerPlannerResources.GetString("Settings_GradeOptions_ListItemGpaType.Title"),
                bindingHost: m_classBindingHost,
                bindingValuePropertyName: nameof(ViewModel.Class.GpaType),
                converter: ConvertGpaTypeToString,
                invokeAction: ViewModel.ConfigureGpaType);

            if (ViewModel.Class.GpaType == PowerPlannerSending.GpaType.PassFail)
            {
                _tableView.AddValueCell(PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title"),
                    bindingHost: m_classBindingHost,
                    bindingValuePropertyName: nameof(ViewModel.Class.PassingGrade),
                    converter: (val) => { return ((double)val).ToString("0.##%"); },
                    invokeAction: ViewModel.ConfigurePassingGrade);
            }
            else
            {
                _tableView.AddCell(PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Title"), ViewModel.ConfigureGradeScale);
            }

            _tableView.AddValueCell(PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text"),
                bindingHost: m_classBindingHost,
                bindingValuePropertyName: nameof(ViewModel.Class.ShouldAverageGradeTotals),
                converter: ConvertBooleanToYesNo,
                invokeAction: ViewModel.ConfigureAverageGrades);

            _tableView.AddValueCell(PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text"),
                bindingHost: m_classBindingHost,
                bindingValuePropertyName: nameof(ViewModel.Class.DoesRoundGradesUp),
                converter: ConvertBooleanToYesNo,
                invokeAction: ViewModel.ConfigureRoundGradesUp);

            _tableView.Compile();
        }

        public string ConvertBooleanToYesNo(object value)
        {
            if (value is bool && (bool)value)
            {
                return "Yes";
            }

            return "No";
        }

        public string ConvertCreditsToString(object value)
        {
            if (value is double credits)
            {
                if (credits == -1)
                {
                    return "None";
                }
            }

            return value?.ToString();
        }

        public string ConvertGpaTypeToString(object value)
        {
            if (value is PowerPlannerSending.GpaType gpaType)
            {
                return GpaTypeToStringConverter.Convert(gpaType);
            }

            return "Unknown";
        }
    }
}
