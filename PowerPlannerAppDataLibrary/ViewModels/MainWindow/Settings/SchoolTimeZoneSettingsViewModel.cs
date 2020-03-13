using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SchoolTimeZoneSettingsViewModel : BaseSettingsViewModelWithAccount
    {
        public SchoolTimeZoneSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            _selectedSchoolTimeZone = Account.SchoolTimeZone;
        }

        private TimeZoneInfo _selectedSchoolTimeZone;
        public TimeZoneInfo SelectedSchoolTimeZone
        {
            get => _selectedSchoolTimeZone;
            set => SetProperty(ref _selectedSchoolTimeZone, value, nameof(SelectedSchoolTimeZone));
        }

        public async void Save()
        {
            try
            {
                IsEnabled = false;
                await Account.SetSchoolTimeZone(SelectedSchoolTimeZone);

                // Reset the app so that changes are applied to all the views
                await base.FindAncestor<MainWindowViewModel>().HandleNormalLaunchActivation();
            }
            catch { IsEnabled = true; }
        }

        public List<TimeZoneInfo> AvailableTimeZones { get; private set; } = GetAvailableTimeZones();

        private static List<TimeZoneInfo> GetAvailableTimeZones()
        {
            List<TimeZoneInfo> answer = new List<TimeZoneInfo>();
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                if (TZConvert.TryWindowsToIana(tz.Id, out string iana))
                {
                    answer.Add(tz);
                }
            }

            return answer;
        }
    }
}
