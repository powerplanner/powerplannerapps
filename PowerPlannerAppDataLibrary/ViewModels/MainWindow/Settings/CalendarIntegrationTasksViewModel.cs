using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Diagnostics;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class CalendarIntegrationTasksViewModel : BaseSettingsViewModelWithAccount
    {
        public CalendarIntegrationTasksViewModel(BaseViewModel parent) : base(parent)
        {
            _isIntegrationEnabled = !Account.IsTasksCalendarIntegrationDisabled;
        }

        private bool _isIntegrationEnabled;

        public bool IsIntegrationEnabled
        {
            get { return _isIntegrationEnabled; }
            set
            {
                if (_isIntegrationEnabled != value)
                {
                    _isIntegrationEnabled = value;
                    OnPropertyChanged(nameof(IsIntegrationEnabled));
                }

                if (Account.IsTasksCalendarIntegrationDisabled != !value)
                {
                    Account.IsTasksCalendarIntegrationDisabled = !value;
                    Account.IsAppointmentsUpToDate = false;
                    SaveAndUpdate();
                }
            }
        }

        private async void SaveAndUpdate()
        {
            try
            {
                base.IsEnabled = false;

                Debug.WriteLine("Tasks calendar integration settings changed, saving...");
                await AccountsManager.Save(Account);
                Debug.WriteLine("Tasks calendar integration settings changed, saved.");

                if (AppointmentsExtension.Current != null)
                {
                    AppointmentsExtension.Current.ResetAll(Account, await AccountDataStore.Get(Account.LocalAccountId));

                    // Wait for the calendar integration to complete
                    await AppointmentsExtension.Current.GetTaskForAllCompleted();
                }
            }

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
