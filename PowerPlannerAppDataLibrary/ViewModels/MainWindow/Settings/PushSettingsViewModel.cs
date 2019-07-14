using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class PushSettingsViewModel : BaseSettingsViewModelWithAccount
    {
        public PushSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            if (!Account.IsOnlineAccount)
            {
                IsEnabled = false;
            }
            else
            {
                IsEnabled = true;
                _isPushEnabled = !Account.IsPushDisabled;
            }
        }

        private bool _isPushEnabled;

        public bool IsPushEnabled
        {
            get { return _isPushEnabled; }
            set
            {
                if (_isPushEnabled != value)
                {
                    _isPushEnabled = value;
                    OnPropertyChanged(nameof(IsPushEnabled));
                }

                if (Account.IsPushDisabled != !value)
                {
                    Account.IsPushDisabled = !value;
                    SaveAndUpdate();
                }
            }
        }

        private async void SaveAndUpdate()
        {
            try
            {
                base.IsEnabled = false;

                await AccountsManager.Save(Account);
                base.IsEnabled = true;

                // Sync so that push channel gets uploaded/removed
                await Sync.SyncAccountAsync(Account);
            }

            catch (OperationCanceledException) { }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                base.IsEnabled = true;
            }
        }
    }
}
