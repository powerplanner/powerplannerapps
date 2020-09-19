using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SyncOptionsSimpleViewModel : BaseViewModel
    {
        public SyncOptionsSimpleViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public void OpenImageUploadOptions()
        {
            SettingsListViewModel.Show(new ImageUploadOptionsViewModel(SettingsListViewModel.GetParentForSubviews(this)));
        }
    }
}
