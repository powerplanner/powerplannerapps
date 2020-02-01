using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ImageUploadOptionsViewModel : BaseSettingsViewModelWithAccount
    {
        public ImageUploadOptionsViewModel(BaseViewModel parent) : base(parent)
        {
            if (Account != null)
            {
                _selectedUploadOption = Account.ImageUploadOption;
            }
        }

        public ImageUploadOptions[] UploadOptions { get; private set; } = new ImageUploadOptions[]
        {
            ImageUploadOptions.Always,
            ImageUploadOptions.WifiOnly,
            ImageUploadOptions.Never
        };

        private ImageUploadOptions _selectedUploadOption;
        public ImageUploadOptions SelectedUploadOption
        {
            get { return _selectedUploadOption; }
            set
            {
                if (_selectedUploadOption != value)
                {
                    _selectedUploadOption = value;
                    OnPropertyChanged(nameof(SelectedUploadOption));
                }

                if (Account.ImageUploadOption != value)
                {
                    Account.ImageUploadOption = value;
                    SaveAndUpdate();
                }
            }
        }

        private async void SaveAndUpdate()
        {
            try
            {
                await AccountsManager.Save(Account);

                // Sync so that images will be uploaded
                if (Account.ImageUploadOption != ImageUploadOptions.Never)
                {
                    try
                    {
                        var dontWait = Sync.SyncAccountAsync(Account);
                    }
                    catch { }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
