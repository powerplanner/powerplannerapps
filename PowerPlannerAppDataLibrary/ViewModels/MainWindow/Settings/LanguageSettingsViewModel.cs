using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class LanguageSettingsViewModel : PopupComponentViewModel
    {
        public class LanguageOption
        {
            public string DisplayName { get; set; }

            public string LanguageCode { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public LanguageSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_LanguageSettings_Header.Text");

            string overriddenLanguageCode;
            if (LanguageExtension.Current != null)
            {
                overriddenLanguageCode = LanguageExtension.Current?.GetLanguageOverrideCode();
            }
            else
            {
                overriddenLanguageCode = Helpers.Settings.LanguageOverride ?? "";
            }

            var matching = Options.FirstOrDefault(i => i.LanguageCode == overriddenLanguageCode);
            _selectedOption = matching;
        }

        public LanguageOption[] Options { get; private set; } = new LanguageOption[]
        {
            new LanguageOption()
            {
                DisplayName = PowerPlannerResources.GetString("Settings_LanguageSettings_AutomaticOptionDisplayName"),
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

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_LanguageSettings_Description.Text")
                },

                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("Settings_LanguageSettings_ComboBoxLanguageOption.Header"),
                    Items = Options,
                    SelectedItem = VxValue.Create<object>(SelectedOption, i => SelectedOption = i as LanguageOption),
                    Margin = new Thickness(0, 18, 0, 0)
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ButtonSave.Content"),
                    Click = SaveChanges,
                    Margin = new Thickness(0, 18, 0, 0)
                },

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote.Text"),
                    Margin = new Thickness(0, 9, 0, 0)
                }.CaptionStyle()

            );
        }

        public async void SaveChanges()
        {
            if (SelectedOption == null)
            {
                return;
            }

            try
            {
                LanguageExtension.Current?.SetLanguageOverrideCode(SelectedOption.LanguageCode);
                Helpers.Settings.LanguageOverride = SelectedOption.LanguageCode == "" ? null : SelectedOption.LanguageCode;
                PowerPlannerResources.ResetCultureInfo();

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
