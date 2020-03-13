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
            if (Account.SchoolTimeZone != null)
            {
                var matching = AvailableTimeZones.FirstOrDefault(i => i.Equals(Account.SchoolTimeZone));
                if (matching != null)
                {
                    _selectedSchoolTimeZone = matching;
                }
            }
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

            if (App.PowerPlannerApp.UsesIanaTimeZoneIds)
            {
                HashSet<string> visitedWindowsIds = new HashSet<string>();
                // In Android, the system time zones are already in IANA format
                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (TZConvert.TryIanaToWindows(tz.Id, out string windows))
                    {
                        if (visitedWindowsIds.Add(windows))
                        {
                            if (TZConvert.TryWindowsToIana(windows, out string iana))
                            {
                                try
                                {
                                    answer.Add(TimeZoneInfo.FindSystemTimeZoneById(iana));
                                }
                                catch { }
                            }
                        }
                    }
                }
                answer = answer.OrderBy(i => i.BaseUtcOffset).ToList();
            }
            else
            {
                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (TZConvert.TryWindowsToIana(tz.Id, out string iana))
                    {
                        answer.Add(tz);
                    }
                }
            }

            return answer;
        }

        /// <summary>
        /// Some platforms like Android don't format time zones by default very well.
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public static string Format(TimeZoneInfo timeZone)
        {
            if (!App.PowerPlannerApp.UsesIanaTimeZoneIds)
            {
                throw new InvalidOperationException("This should only be called if UsesIanaTimeZoneIds is set to true");
            }

            return $"(UTC{(timeZone.BaseUtcOffset.TotalMinutes < 0 ? "-" : "+")}{timeZone.BaseUtcOffset.ToString("hh\\:mm")}) {TZConvert.IanaToWindows(timeZone.Id)}";
        }
    }
}
