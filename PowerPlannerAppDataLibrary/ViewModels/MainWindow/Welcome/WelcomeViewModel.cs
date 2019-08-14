using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome
{
    public class WelcomeViewModel : BaseViewModel
    {
        public WelcomeViewModel(BaseViewModel parent) : base(parent) { }

        public void Login()
        {
            ShowPopup(new LoginViewModel(this));
        }

        public void CreateAccount()
        {
            ShowPopup(new CreateAccountViewModel(this));
        }

        public void GetStarted()
        {
            ShowPopup(new GetStartedViewModel(this));
        }

        public void OpenSettings()
        {
            var mainWindowViewModel = this.FindAncestor<MainWindowViewModel>();
            mainWindowViewModel.Navigate(new SettingsViewModel(mainWindowViewModel));
        }
    }
}
