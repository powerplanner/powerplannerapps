using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class ExistingUserViewModel : BaseMainScreenViewModelDescendant
    {
        public ExistingUserViewModel(BaseViewModel parent) : base(parent) { }

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
