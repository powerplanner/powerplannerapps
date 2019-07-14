using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.DataLayer.TileSettings;

namespace PowerPlannerUWP.ViewModel.Settings
{
    public class ClassTileViewModel : BaseSettingsViewModelWithAccount
    {
        public ViewItemClass Class { get; private set; }

        private ClassTileSettings _settings;
        public ClassTileSettings Settings
        {
            get { return _settings; }
            set { SetProperty(ref _settings, value, nameof(Settings)); }
        }

        public ClassTileViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;
        }

        protected override async Task LoadAsyncOverride()
        {
            await base.LoadAsyncOverride();

            Settings = await Account.GetClassTileSettings(Class.Identifier);
        }
    }
}
