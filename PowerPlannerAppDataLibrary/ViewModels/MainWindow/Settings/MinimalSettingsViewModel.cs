using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class MinimalSettingsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;
        public MinimalSettingsViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
             _account = account;
            Title = R.S("MainMenuItem_Settings");
        }

        protected override View Render()
        {
            var layout = new LinearLayout();

            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Children =
                    {
                        _account != null && !_account.IsDefaultOfflineAccount ? SettingsListViewModel.RenderOption(
                            MaterialDesign.MaterialDesignIcons.AccountCircle,
                            PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Title"),
                            PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Subtitle"),
                            OpenMyAccount) : null,

                        SettingsListViewModel.RenderOption(
                            MaterialDesign.MaterialDesignIcons.Help,
                            PowerPlannerResources.GetString("Settings_MainPage_HelpItem.Title"),
                            PowerPlannerResources.GetString("Settings_MainPage_HelpItem.Subtitle"),
                            SettingsListViewModel.OpenHelp),

                        SettingsListViewModel.RenderOption(
                            MaterialDesign.MaterialDesignIcons.Info,
                            PowerPlannerResources.GetString("Settings_MainPage_AboutItem.Title"),
                            "BareBones Dev",
                            OpenAbout)
                    }
                }
            };
        }

        private void OpenMyAccount()
        {
            ShowPopup(MyAccountViewModel.Load(Parent));
        }

        private void OpenAbout()
        {
            ShowPopup(new AboutViewModel(Parent));
        }
    }
}
