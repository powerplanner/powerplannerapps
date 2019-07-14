using BareMvvm.Core.Windows;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Windows
{
    public class MainAppWindow : PortableAppWindow
    {
        public MainAppWindow() : base()
        {
            ViewModel = new MainWindowViewModel(null);
        }

        public MainWindowViewModel GetViewModel()
        {
            return ViewModel as MainWindowViewModel;
        }

        public AccountDataItem GetCurrentAccount()
        {
            return GetViewModel().CurrentAccount;
        }

        public void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsType updateType)
        {
            GetViewModel().ShowPopupUpdateCredentials(account, updateType);
        }

        public MainScreenViewModel GetMainScreenViewModel()
        {
            return GetViewModel()?.GetMainScreenViewModel();
        }
    }
}
