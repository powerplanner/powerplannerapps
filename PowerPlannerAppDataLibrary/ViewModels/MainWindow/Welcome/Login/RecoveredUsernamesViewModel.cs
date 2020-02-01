using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login
{
    public class RecoveredUsernamesViewModel : BaseViewModel
    {
        public RecoveredUsernamesViewModel(BaseViewModel parent, string[] usernames) : base(parent)
        {
            Usernames = usernames;

            var loginViewModel = parent.GetPopupViewModelHost()?.Popups.OfType<LoginViewModel>().FirstOrDefault();
            if (loginViewModel != null && usernames.Length > 0)
            {
                loginViewModel.Username = usernames[0];
            }
        }

        public string[] Usernames { get; private set; }
    }
}
