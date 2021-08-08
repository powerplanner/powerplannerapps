using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SchoolTimeZoneSettingsViewModel : PopupComponentViewModel
    {
        public AccountDataItem Account { get; private set; }

        public SchoolTimeZoneSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Account = FindAncestor<MainWindowViewModel>().CurrentAccount;
            IsEnabled = Account != null;

            Title = PowerPlannerResources.GetString("Settings_SchoolTimeZone_Header.Text");

            if (Account.SchoolTimeZone != null)
            {
                var matching = AvailableTimeZones.FirstOrDefault(i => i.Equals(Account.SchoolTimeZone));
                if (matching != null)
                {
                    _selectedSchoolTimeZone = matching;
                }
            }
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_Description.Text")
                        },

                        new ComboBox
                        {
                            Header = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ComboBoxTimeZone.Header"),
                            Items = AvailableTimeZones,
                            SelectedItem = VxValue.Create<object>(SelectedSchoolTimeZone, v => SelectedSchoolTimeZone = v as TimeZoneInfo),
                            IsEnabled = IsEnabled,
                            Margin = new Thickness(0, 18, 0, 0)
                        },

                        new AccentButton
                        {
                            Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ButtonSave.Content"),
                            IsEnabled = IsEnabled,
                            Click = Save,
                            Margin = new Thickness(0, 18, 0, 0)
                        },

                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote.Text"),
                            FontSize = Theme.Current.CaptionFontSize,
                            TextColor = Theme.Current.SubtleForegroundColor,
                            Margin = new Thickness(0, 6, 0, 0)
                        }
                    }
                }
            };
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, nameof(IsEnabled));
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

            return Format(timeZone.BaseUtcOffset, TZConvert.IanaToWindows(timeZone.Id));
        }

        public static string FormatWindows(TimeZoneInfo timeZone)
        {
            return Format(timeZone.BaseUtcOffset, timeZone.DisplayName);
        }

        private static string Format(TimeSpan offset, string displayName)
        {
            return $"(UTC{(offset.TotalMinutes < 0 ? "-" : "+")}{offset.ToString("hh\\:mm")}) {displayName}";
        }
    }
}
