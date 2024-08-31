using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class PushSettingsViewModel : PopupComponentViewModel
    {
        private AccountDataItem Account;

        public PushSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = R.S("Settings_PushPage_Header.Text");
            Account = MainScreenViewModel.CurrentAccount;

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

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, nameof(IsEnabled)); }
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
                IsEnabled = false;

                await AccountsManager.Save(Account);
                IsEnabled = true;

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
                IsEnabled = true;
            }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new Switch
                {
                    IsEnabled = IsEnabled,
                    IsOn = VxValue.Create<bool>(IsPushEnabled, v => IsPushEnabled = v)
                },

                new TextBlock
                {
                    Text = R.S("Settings_PushPage_Description.Text"),
                    Margin = new Thickness(0, 12, 0, 0)
                }
            );
        }
    }
}
