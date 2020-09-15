using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using ToolsPortable;
using System.ComponentModel;
using InterfacesiOS.Binding;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Controllers.Settings
{
    public class MyAccountViewController : BareMvvmUIViewController<MyAccountViewModel>
    {
        private UITableViewCell _cellUsername;
        private BareUIStaticGroupedTableView _tableView;
        private BindingHost _accountBindingHost;
        private object _tabBarHeightListener;

        public MyAccountViewController()
        {
            Title = "My Account";

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

        public override void OnViewModelAndViewLoadedOverride()
        {
            _accountBindingHost = new BindingHost()
            {
                DataContext = ViewModel.CurrentAccount
            };

            InitializeTable();

            // Watch for online account changing
            _accountBindingHost.SetBinding(nameof(ViewModel.CurrentAccount.IsOnlineAccount), OnIsOnlineAccountChanged);

            base.OnViewModelAndViewLoadedOverride();
        }

        private void InitializeTable()
        {
            _cellUsername = new UITableViewCell(UITableViewCellStyle.Default, "TableCell");
            _cellUsername.TextLabel.TextColor = UIColor.LightGray;
            _accountBindingHost.SetLabelTextBinding(_cellUsername.TextLabel, nameof(ViewModel.CurrentAccount.Username));
            _tableView.AddCell(_cellUsername, null);

            _tableView.StartNewGroup();

            _tableView.AddCell("Log Out", ViewModel.LogOut);
            _tableView.AddCell("Change Username", ViewModel.ChangeUsername);
            _tableView.AddCell("Change Password", ViewModel.ChangePassword);

            if (ViewModel.CurrentAccount.IsOnlineAccount)
            {
                _tableView.AddCell("Change Email Address", ViewModel.ChangeEmail);
            }

            if (!ViewModel.CurrentAccount.IsOnlineAccount)
            {
                _tableView.AddCell("Convert to Online Account", ViewModel.ConvertToOnline);
            }

            _tableView.StartNewGroup();

            var cellDelete = new UITableViewCell(UITableViewCellStyle.Default, "TableCell");
            cellDelete.TextLabel.Text = "Delete Account";
            cellDelete.TextLabel.TextColor = UIColor.Red;
            cellDelete.TextLabel.TextAlignment = UITextAlignment.Center;
            _tableView.AddCell(cellDelete, ViewModel.PromptConfirmDelete);

            _tableView.Compile();
        }

        private void OnIsOnlineAccountChanged()
        {
            try
            {
                _tableView.ClearAll();
                InitializeTable();
            }
            catch { }
        }
    }
}