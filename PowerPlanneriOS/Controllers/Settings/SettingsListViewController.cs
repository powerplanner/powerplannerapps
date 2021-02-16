using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using ToolsPortable;
using System.ComponentModel;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesiOS.Helpers;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class SettingsListViewController : BareMvvmUIViewController<SettingsListViewModel>
    {
        private BareUIStaticGroupedTableView _tableView;
        private object _tabBarHeightListener;

        private UITableViewCell _cellSyncStatus;
        private UITableViewCell _cellSyncNow;

        public SettingsListViewController()
        {
            AutomaticallyAdjustsScrollViewInsets = false;
            Title = "More";

            // Creating a table view programmatically: https://developer.apple.com/library/content/documentation/UserExperience/Conceptual/TableView_iPhone/CreateConfigureTableView/CreateConfigureTableView.html#//apple_ref/doc/uid/TP40007451-CH6-SW4
            // Xamarin: https://developer.xamarin.com/guides/ios/user_interface/controls/tables/populating-a-table-with-data/
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
            base.OnViewModelLoadedOverride();

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            RedrawItems();
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsOnlineAccount):
                    // Take a break off this thread to let the other properties we depend on like IsCreateAccountVisible update first
                    await System.Threading.Tasks.Task.Delay(1);
                    RedrawItems();
                    break;
            }
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            try
            {
                if (_isFullVersion != await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    RedrawItems();
                }
            }
            catch { }
        }

        private bool _isListeningToSemester;
        private bool _isListeningToMain;
        private bool _isFullVersion;
        private UITableViewCell _cellCurrentSemester;
        private async void RedrawItems()
        {
            _tableView.ClearAll();

            if (ViewModel.HasAccount)
            {
                var mainScreenViewModel = ViewModel.FindAncestor<MainScreenViewModel>();
                var account = ViewModel.Account;

                _isFullVersion = await PowerPlannerApp.Current.IsFullVersionAsync();
                if (!_isFullVersion)
                {
                    _tableView.AddCell("Upgrade to Premium", ViewModel.OpenPremiumVersion);

                    _tableView.StartNewGroup();
                }

                if (ViewModel.IsViewYearsAndSemestersVisible)
                {
                    _cellCurrentSemester = new UITableViewCell(UITableViewCellStyle.Default, "TableCellCurrentSemester");

                    if (mainScreenViewModel.CurrentSemester != null && !_isListeningToSemester)
                    {
                        _isListeningToSemester = true;
                        mainScreenViewModel.CurrentSemester.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(CurrentSemester_PropertyChanged).Handler;
                    }

                    UpdateCurrentSemesterText(mainScreenViewModel.CurrentSemester);
                    _tableView.AddCell(_cellCurrentSemester, null);

                    _tableView.AddCell("View Years & Semesters", () => ViewModel.OpenYears());

                    _tableView.StartNewGroup();
                }

                if (ViewModel.IsOnlineAccount && mainScreenViewModel != null)
                {
                    if (!_isListeningToMain)
                    {
                        _isListeningToMain = true;
                        mainScreenViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(MainScreenViewModel_PropertyChanged).Handler;
                    }

                    _cellSyncStatus = new UITableViewCell(UITableViewCellStyle.Default, "TableCellSyncStatus");
                    _tableView.AddCell(_cellSyncStatus, delegate
                    {
                        ViewModel.ViewSyncErrors();
                    });

                    _cellSyncNow = new UITableViewCell(UITableViewCellStyle.Default, "TableCellSyncNow");
                    _tableView.AddCell(_cellSyncNow, delegate
                    {
                        ViewModel.StartSync();
                    });

                    UpdateSyncStatuses(mainScreenViewModel);

                    _tableView.StartNewGroup();
                }
            }

            if (ViewModel.IsCreateAccountVisible)
            {
                _tableView.AddCell("Create Account", ViewModel.OpenCreateAccount);
            }

            if (ViewModel.IsLogInVisible)
            {
                _tableView.AddCell("Log In", ViewModel.OpenLogIn);
            }

            if (ViewModel.IsMyAccountVisible)
            {
                _tableView.AddCell("My Account", ViewModel.OpenMyAccount);
            }

            if (ViewModel.IsRemindersVisible)
            {
                _tableView.AddCell("Reminders", ViewModel.OpenReminderSettings);
            }

            if (ViewModel.IsGoogleCalendarIntegrationVisible)
            {
                _tableView.AddCell("Google Calendar Integration", OpenGoogleCalendarIntegration);
            }

            if (ViewModel.IsTwoWeekScheduleVisible)
            {
                _tableView.AddCell("Two Week Schedule", ViewModel.OpenTwoWeekScheduleSettings);
            }

            if (ViewModel.IsSchoolTimeZoneVisible)
            {
                _tableView.AddCell("Time Zone", ViewModel.OpenSchoolTimeZone);
            }

            _tableView.AddCell("Help", OpenHelp);

            _tableView.AddCell("About", ViewModel.OpenAbout);

            _tableView.Compile();
        }

        private void CurrentSemester_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PowerPlannerAppDataLibrary.ViewItems.ViewItemSemester.Name):
                    UpdateCurrentSemesterText(sender as PowerPlannerAppDataLibrary.ViewItems.ViewItemSemester);
                    break;
            }
        }

        private void UpdateCurrentSemesterText(PowerPlannerAppDataLibrary.ViewItems.ViewItemSemester semester)
        {
            try
            {
                string currentSemesterText = semester?.Name ?? "None";

                SetDisabled(_cellCurrentSemester, "Current semester: " + currentSemesterText);
            }
            catch { }
        }

        private void OpenHelp()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_OpenHelp");

                UIApplication.SharedApplication.OpenUrl(new NSUrl(SettingsListViewModel.HelpUrl));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                var dontWait = new PortableMessageDialog("Failed to open web browser", "Error").ShowAsync();
            }
        }

        private void OpenGoogleCalendarIntegration()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_OpenGoogleCalendarIntegration");

                if (ViewModel.AlertIfGoogleCalendarIntegrationNotPossible())
                {
                    return;
                }

                UIApplication.SharedApplication.OpenUrl(new NSUrl(GoogleCalendarIntegrationViewModel.Url));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                var dontWait = new PortableMessageDialog("Failed to open web browser", "Error").ShowAsync();
            }
        }

        private void MainScreenViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainScreenViewModel.SyncState):
                case nameof(MainScreenViewModel.HasSyncErrors):
                case nameof(MainScreenViewModel.IsOffline):
                    UpdateSyncStatuses(sender as MainScreenViewModel);
                    break;
            }
        }
        private void UpdateSyncStatuses(MainScreenViewModel mainScreenViewModel)
        {
            var account = mainScreenViewModel.CurrentAccount;
            if (account == null)
            {
                SetDisabled(_cellSyncStatus, "No account to sync");
                SetDisabled(_cellSyncNow, "Sync Now");
                return;
            }

            if (mainScreenViewModel.SyncState == MainScreenViewModel.SyncStates.Done)
            {
                SetNormal(_cellSyncNow, "Sync Now");
            }
            else
            {
                SetDisabled(_cellSyncNow, "Syncing...");
            }

            if (mainScreenViewModel.HasSyncErrors)
            {
                SetAttention(_cellSyncStatus, "Sync error");
            }
            else if (mainScreenViewModel.IsOffline)
            {
                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SetDisabled(_cellSyncStatus, "Offline, last sync " + FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SetDisabled(_cellSyncStatus, "Offline, couldn't sync");
                }
            }
            else
            {
                if (account.LastSyncOn != DateTime.MinValue)
                {
                    SetDisabled(_cellSyncStatus, "Last sync: " + FriendlyLastSyncTime(account.LastSyncOn));
                }
                else
                {
                    SetDisabled(_cellSyncStatus, "Sync needed");
                }
            }
        }

        private static string FriendlyLastSyncTime(DateTime time)
        {
            if (time.Date == DateTime.Today)
            {
                return time.ToString("t");
            }
            else
            {
                return time.ToString("d");
            }
        }

        private void SetDisabled(UITableViewCell cell, string text)
        {
            cell.TextLabel.Text = text;
            cell.UserInteractionEnabled = false;
            cell.TextLabel.TextColor = UIColorCompat.SecondaryLabelColor;
        }

        private void SetNormal(UITableViewCell cell, string text)
        {
            cell.TextLabel.Text = text;
            cell.UserInteractionEnabled = true;
            cell.TextLabel.TextColor = UIColorCompat.LabelColor;
        }

        private void SetAttention(UITableViewCell cell, string text)
        {
            cell.TextLabel.Text = text;
            cell.UserInteractionEnabled = true;
            cell.TextLabel.TextColor = UIColor.Red;
        }
    }
}