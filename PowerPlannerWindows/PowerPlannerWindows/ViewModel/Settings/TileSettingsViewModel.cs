using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerUWP.ViewModel.Settings
{
    public class TileSettingsViewModel : BaseSettingsSplitViewModel
    {
        public TileSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Items = new BaseViewModel[]
            {
                new MainTileViewModel(this),
                new ScheduleTileViewModel(this),
                new ClassTilesPagedHostViewModel(this),
                new QuickAddTileViewModel(this)
            };
        }
    }
}
