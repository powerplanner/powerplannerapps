﻿using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
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
            AvailableTimeZones = GetAvailableTimeZones();
            Account = FindAncestor<MainWindowViewModel>().CurrentAccount;
            IsEnabled = Account != null && AvailableTimeZones.Count > 0;

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

                Error != null ? new TextBlock
                {
                    Text = Error,
                    FontWeight = FontWeights.Bold,
                    TextColor = System.Drawing.Color.Red,
                    Margin = new Thickness(0, 6, 0, 0)
                } : null,

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

            public FriendlyTimeZone(TimeZoneInfo tz, string friendlyName)
            {
                TimeZone = tz;
                FriendlyName = friendlyName;
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

        public string Error { get; private set; }

        public async void Save()
        {
            try
            {
                IsEnabled = false;
                await Account.SetSchoolTimeZone(SelectedSchoolTimeZone.TimeZone);

                // Reset the app so that changes are applied to all the views
                await base.FindAncestor<MainWindowViewModel>().HandleNormalLaunchActivation(sync: false);
            }
            catch { IsEnabled = true; }
        }

        public List<FriendlyTimeZone> AvailableTimeZones { get; private set; }

        private List<FriendlyTimeZone> GetAvailableTimeZones()
        {
            try
            {
                return GetAvailableTimeZones(CultureInfo.CurrentUICulture.Name);
            }
            catch
            {
                try
                {
                    return GetAvailableTimeZones(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                }
                catch
                {
                    try
                    {
                        return GetAvailableTimeZones("en-US");
                    }
                    catch (Exception usEx)
                    {
                        TelemetryExtension.Current?.TrackException(usEx);
                        Error = "Error: Could not load the time zones.";
                        return new List<FriendlyTimeZone>();
                    }
                }
            }
        }

        private static List<FriendlyTimeZone> GetAvailableTimeZones(string languageCode)
        {
            List<FriendlyTimeZone> answer = new List<FriendlyTimeZone>();
            Exception latestEx = null;

            foreach (var tz in TZNames.GetDisplayNames(languageCode, useIanaZoneIds: true))
            {
                // In Android, the system time zones are already in IANA format
                if (App.PowerPlannerApp.UsesIanaTimeZoneIds)
                {
                    try
                    {
                        answer.Add(new FriendlyTimeZone(TimeZoneInfo.FindSystemTimeZoneById(tz.Key), tz.Value));
                    }
                    catch (Exception ex)
                    {
                        latestEx = ex;
                    }
                }
                else if (TZConvert.TryIanaToWindows(tz.Key, out string windowsId))
                {
                    answer.Add(new FriendlyTimeZone(TimeZoneInfo.FindSystemTimeZoneById(windowsId), tz.Value));
                }
            }

            if (answer.Count <= 5 && latestEx != null)
            {
                throw latestEx;
            }

            return answer;
        }
    }
}
