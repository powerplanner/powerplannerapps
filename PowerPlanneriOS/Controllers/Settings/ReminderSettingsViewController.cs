using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class ReminderSettingsViewController : BareMvvmUIViewController<ReminderSettingsViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;
        private object _tabBarHeightListener;

        public ReminderSettingsViewController()
        {
            Title = "Reminders";

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
            _tableView.AddDescriptionCell("Power Planner automatically sets reminders for each task and event.");

            _tableView.StartNewGroup();

            var switchDayBefore = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Day Before"
            };
            BindingHost.SetSwitchBinding(switchDayBefore, nameof(ViewModel.RemindersDayBefore));
            var cellDayBefore = new BareUITableViewCell("dayBefore");
            cellDayBefore.ContentView.Add(switchDayBefore);
            switchDayBefore.StretchWidthAndHeight(cellDayBefore.ContentView);
            _tableView.AddCell(cellDayBefore, delegate { }); // Have to provide action so it remains clickable

            _tableView.AddCaptionDescriptionCell("This reminds you the day before your incomplete tasks or events are due. It will remind you 10 minutes after your last class, or at 3:00 PM if you don't have any classes that day.");

            _tableView.StartNewGroup();

            var switchDayOf = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Day Of"
            };
            BindingHost.SetSwitchBinding(switchDayOf, nameof(ViewModel.RemindersDayOf));
            var cellDayOf = new BareUITableViewCell("dayOf");
            cellDayOf.ContentView.Add(switchDayOf);
            switchDayOf.StretchWidthAndHeight(cellDayOf.ContentView);
            _tableView.AddCell(cellDayOf, delegate { });

            _tableView.AddCaptionDescriptionCell("This reminds you on the same day that your incomplete tasks or events are due. It will remind you 1 hour before your class starts, or at 6:00 PM if your class doesn't meet on that day.");

            _tableView.Compile();

            base.OnViewModelLoadedOverride();
        }
    }
}