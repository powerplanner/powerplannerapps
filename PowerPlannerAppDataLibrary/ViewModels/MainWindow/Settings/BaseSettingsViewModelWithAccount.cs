using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class BaseSettingsViewModelWithAccount : BaseViewModel
    {
        public BaseSettingsViewModelWithAccount(BaseViewModel parent) : base(parent)
        {
            Account = FindAncestor<MainWindowViewModel>().CurrentAccount;
            if (Account != null)
            {
                IsEnabled = true;
            }
        }

        public AccountDataItem Account { get; private set; }

        public bool IsEnabled { get; set; }
    }
}
