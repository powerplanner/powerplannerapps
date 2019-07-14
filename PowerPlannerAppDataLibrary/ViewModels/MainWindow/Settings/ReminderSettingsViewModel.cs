using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ReminderSettingsViewModel : BaseViewModel
    {
        private AccountDataItem _account;

        public ReminderSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            _account = base.FindAncestor<MainWindowViewModel>().CurrentAccount;
            if (_account != null)
            {
                _remindersDayBefore = _account.RemindersDayBefore;
                _remindersDayOf = _account.RemindersDayOf;
                IsEnabled = true;
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, nameof(IsEnabled)); }
        }

        private bool _remindersDayOf;
        public bool RemindersDayOf
        {
            get { return _remindersDayOf; }
            set
            {
                if (_remindersDayOf != value)
                {
                    _remindersDayOf = value;
                    OnPropertyChanged(nameof(RemindersDayOf));
                }

                if (_account.RemindersDayOf != value)
                {
                    _account.RemindersDayOf = value;
                    SaveAndUpdate();
                }
            }
        }

        private bool _remindersDayBefore;
        public bool RemindersDayBefore
        {
            get { return _remindersDayBefore; }
            set
            {
                if (_remindersDayBefore != value)
                {
                    _remindersDayBefore = value;
                    OnPropertyChanged(nameof(RemindersDayBefore));
                }

                if (_account.RemindersDayBefore != value)
                {
                    _account.RemindersDayBefore = value;
                    SaveAndUpdate();
                }
            }
        }

        private async void SaveAndUpdate()
        {
            try
            {
                IsEnabled = false;
                
                Debug.WriteLine("Reminder settings changed, saving...");
                await AccountsManager.Save(_account);
                Debug.WriteLine("Reminder settings changed, saved.");

                var dontWait = RemindersExtension.Current?.ResetReminders(_account, await AccountDataStore.Get(_account.LocalAccountId));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                IsEnabled = true;
            }
        }
    }
}
