using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using UIKit;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class TwoWeekScheduleSettingsViewController : BareMvvmUIViewController<TwoWeekScheduleSettingsViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;
        private object _tabBarHeightListener;

        public TwoWeekScheduleSettingsViewController()
        {
            Title = "Two week schedule";

            _tableView = new BareUIStaticGroupedTableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_tableView);
            _tableView.StretchWidthAndHeight(View);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _tableView.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });
        }

        public override void OnViewModelLoadedOverride()
        {
            _tableView.AddDescriptionCell(PowerPlannerResources.GetString("Settings_TwoWeekSchedule_Description.Text"));

            _tableView.StartNewGroup();

            var currentWeek = new BareUIInlinePickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Current week",
                ItemsSource = ViewModel.AvailableCurrentWeekStrings
            };
            BindingHost.SetSelectedItemBinding(currentWeek, nameof(ViewModel.CurrentWeekString));
            var cellCurrentWeek = new BareUITableViewCell("currentWeek");
            cellCurrentWeek.ContentView.Add(currentWeek);
            currentWeek.StretchWidth(cellCurrentWeek.ContentView);
            currentWeek.SetHeight(44);
            _tableView.AddCell(cellCurrentWeek, delegate { }); // Have to provide action so it remains clickable

            _tableView.StartNewGroup();

            var weekChangesOn = new BareUIInlinePickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Week changes on",
                ItemsSource = ViewModel.AvailableWeekChangesOnStrings
            };
            BindingHost.SetSelectedItemBinding(weekChangesOn, nameof(ViewModel.WeekChangesOnString));
            var cellWeekChangesOn = new BareUITableViewCell("weekChangesOn");
            cellWeekChangesOn.ContentView.Add(weekChangesOn);
            weekChangesOn.StretchWidth(cellWeekChangesOn.ContentView);
            weekChangesOn.SetHeight(44);
            _tableView.AddCell(cellWeekChangesOn, delegate { });

            _tableView.Compile();

            base.OnViewModelLoadedOverride();
        }
    }
}