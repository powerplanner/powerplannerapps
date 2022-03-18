using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;
using TimeZoneNames;
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
                var matching = AvailableTimeZones.FirstOrDefault(i => i.TimeZone.Equals(Account.SchoolTimeZone));
                if (matching != null)
                {
                    _selectedSchoolTimeZone = matching;
                }
            }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_Description.Text")
                },

                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ComboBoxTimeZone.Header"),
                    Items = AvailableTimeZones,
                    SelectedItem = VxValue.Create<object>(SelectedSchoolTimeZone, v => SelectedSchoolTimeZone = v as FriendlyTimeZone),
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
            );
        }
        
        public class FriendlyTimeZone
        {
            public TimeZoneInfo TimeZone { get; private set; }

            public string FriendlyName { get; private set; }

            public FriendlyTimeZone(TimeZoneInfo tz)
            {
                TimeZone = tz;
                FriendlyName = TZNames.GetDisplayNameForTimeZone(tz.Id, CultureInfo.CurrentUICulture.Name);
            }

            public override string ToString()
            {
                return FriendlyName;
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, nameof(IsEnabled));
        }

        private FriendlyTimeZone _selectedSchoolTimeZone;
        public FriendlyTimeZone SelectedSchoolTimeZone
        {
            get => _selectedSchoolTimeZone;
            set => SetProperty(ref _selectedSchoolTimeZone, value, nameof(SelectedSchoolTimeZone));
        }

        public async void Save()
        {
            try
            {
                IsEnabled = false;
                await Account.SetSchoolTimeZone(SelectedSchoolTimeZone.TimeZone);

                // Reset the app so that changes are applied to all the views
                await base.FindAncestor<MainWindowViewModel>().HandleNormalLaunchActivation();
            }
            catch { IsEnabled = true; }
        }

        public List<FriendlyTimeZone> AvailableTimeZones { get; private set; } = GetAvailableTimeZones();

        private static List<FriendlyTimeZone> GetAvailableTimeZones()
        {
            List<FriendlyTimeZone> answer = new List<FriendlyTimeZone>();

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
                                    answer.Add(new FriendlyTimeZone(TimeZoneInfo.FindSystemTimeZoneById(iana)));
                                }
                                catch { }
                            }
                        }
                    }
                }
                answer = answer.OrderBy(i => i.TimeZone.BaseUtcOffset).ToList();
            }
            else
            {
                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (TZConvert.TryWindowsToIana(tz.Id, out string iana))
                    {
                        answer.Add(new FriendlyTimeZone(tz));
                    }
                }
            }

            return answer;
        }
    }
}
