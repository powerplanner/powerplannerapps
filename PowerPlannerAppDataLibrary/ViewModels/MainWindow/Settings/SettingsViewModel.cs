using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SettingsViewModel : PagedViewModel
    {
        public SettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Navigate(new SettingsListViewModel(this));
        }
    }
}
