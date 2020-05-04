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
    public class SchoolTimeZoneSettingsViewController : BareMvvmUIViewController<SchoolTimeZoneSettingsViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;
        private object _tabBarHeightListener;

        public SchoolTimeZoneSettingsViewController()
        {
            Title = "School's time zone";

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
            _tableView.AddDescriptionCell(PowerPlannerResources.GetString("Settings_SchoolTimeZone_Description.Text"));

            _tableView.StartNewGroup();

            var selectedTimeZone = new BareUIInlinePickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Time zone",
                ItemsSource = ViewModel.AvailableTimeZones,
                ItemToPreviewStringConverter = item => SchoolTimeZoneSettingsViewModel.Format(item as TimeZoneInfo),
                ItemToViewConverter = ConvertTimeZoneToInlineView
            };
            BindingHost.SetSelectedItemBinding(selectedTimeZone, nameof(ViewModel.SelectedSchoolTimeZone));
            var cellSelectedTimeZone = new BareUITableViewCell("selectedTimeZone");
            cellSelectedTimeZone.ContentView.Add(selectedTimeZone);
            selectedTimeZone.StretchWidth(cellSelectedTimeZone.ContentView);
            selectedTimeZone.SetHeight(44);
            _tableView.AddCell(cellSelectedTimeZone, delegate { }); // Have to provide action so it remains clickable

            _tableView.StartNewGroup();

            _tableView.AddCell("Save changes", ViewModel.Save);

            _tableView.AddCaptionDescriptionCell(PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote.Text"));

            _tableView.Compile();

            base.OnViewModelLoadedOverride();
        }

        private static UIView ConvertTimeZoneToInlineView(object item, UIView recycledView)
        {
            var tz = item as TimeZoneInfo;

            var view = recycledView as UILabel;
            if (view == null)
            {
                view = new UILabel();
            }
            view.Text = SchoolTimeZoneSettingsViewModel.Format(tz);
            return view;
        }
    }
}