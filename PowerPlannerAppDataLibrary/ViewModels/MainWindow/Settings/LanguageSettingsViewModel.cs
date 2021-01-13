using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class LanguageSettingsViewModel : BaseViewModel
    {
        public class LanguageOption
        {
            public string DisplayName { get; set; }

            public string LanguageCode { get; set; }
        }

        public LanguageSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            var overriddenLanguageCode = LanguageExtension.Current.GetLanguageOverrideCode();
            var matching = Options.FirstOrDefault(i => i.LanguageCode == overriddenLanguageCode);
            _selectedOption = matching;
        }

        public LanguageOption[] Options { get; private set; } = new LanguageOption[]
        {
            new LanguageOption()
            {
                DisplayName = LocalizationExtension.Current.GetString("Settings_LanguageSettings_AutomaticOptionDisplayName"),
                LanguageCode = "" // Empty string represents automatic/no override
            },
            new LanguageOption()
            {
                DisplayName = "English",
                LanguageCode = "en-US"
            },
            new LanguageOption()
            {
                DisplayName = "Español",
                LanguageCode = "es"
            },
            new LanguageOption()
            {
                DisplayName = "Français",
                LanguageCode = "fr"
            },
            new LanguageOption()
            {
                DisplayName = "Portugués",
                LanguageCode = "pt"
            },
            new LanguageOption()
            {
                DisplayName = "Deutsch",
                LanguageCode = "de"
            },
            new LanguageOption()
            {
                DisplayName = "العربية",
                LanguageCode = "ar"
            }
        };

        private LanguageOption _selectedOption;
        public LanguageOption SelectedOption
        {
            get => _selectedOption;
            set => SetProperty(ref _selectedOption, value, nameof(SelectedOption));
        }

        public async void SaveChanges()
        {
            if (SelectedOption == null)
            {
                return;
            }

            try
            {
                LanguageExtension.Current.SetLanguageOverrideCode(SelectedOption.LanguageCode);

                TelemetryExtension.Current?.TrackEvent("ChangedLanguage", new Dictionary<string, string>()
                {
                    { "Language", SelectedOption.LanguageCode }
                });

                // Reset the app so that changes are applied to all the views
                await FindAncestor<MainWindowViewModel>().HandleNormalLaunchActivation();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
