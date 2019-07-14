using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SyncOptionsViewModel : BaseSettingsSplitViewModel
    {
        public SyncOptionsViewModel(BaseViewModel parent) : base(parent)
        {
            Items = new BaseViewModel[]
            {
                new ImageUploadOptionsViewModel(this),
                new PushSettingsViewModel(this)
            };
        }
    }
}
