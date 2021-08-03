using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class ExistingUserViewModel : PopupComponentViewModel
    {
        public ExistingUserViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Welcome_ExistingUserPage.Title");
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("Welcome_ExistingUserPage_Message.Text")
                        },

                        new AccentButton
                        {
                            Text = PowerPlannerResources.GetString("Welcome_ExistingUserPage_ButtonHasAccount.Content"),
                            Margin = new Thickness(0, 24, 0, 0),
                            Click = HasAccount
                        },

                        new AccentButton
                        {
                            Text = PowerPlannerResources.GetString("Welcome_ExistingUserPage_ButtonNoAccount.Content"),
                            Margin = new Thickness(0, 12, 0, 0),
                            Click = NoAccount
                        }
                    }
                }
            };
        }

        public void HasAccount()
        {
            ShowPopup(new LoginViewModel(this)
            {
                // It should always be the default account, but just in case
                DefaultAccountToDelete = MainScreenViewModel.CurrentAccount.IsDefaultOfflineAccount ? MainScreenViewModel.CurrentAccount : null
            });
        }

        public void NoAccount()
        {
            ShowPopup(new ConnectAccountViewModel(this));
        }
    }
}
