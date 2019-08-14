using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class ExistingUserViewModel : BaseViewModel
    {
        public ExistingUserViewModel(BaseViewModel parent) : base(parent) { }

        public void HasAccount()
        {
            ShowPopup(new LoginViewModel(this));
        }

        public void NoAccount()
        {
            ShowPopup(new ConnectAccountViewModel(this));
        }
    }
}
