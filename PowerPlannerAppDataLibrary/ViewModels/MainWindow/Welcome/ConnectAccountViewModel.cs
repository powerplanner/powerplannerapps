using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class ConnectAccountViewModel : BaseViewModel
    {
        public ConnectAccountViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public void LogIn()
        {
            ShowPopup(new LoginViewModel(this));
        }
    }
}
