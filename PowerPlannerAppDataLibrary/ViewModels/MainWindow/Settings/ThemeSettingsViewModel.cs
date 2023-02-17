using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    internal class ThemeSettingsViewModel : PopupComponentViewModel
    {
        private VxState<Themes> _theme = new VxState<Themes>(Helpers.Settings.ThemeOverride);
        private Themes[] Options = new Themes[]
        {
            Themes.Automatic,
            Themes.Light,
            Themes.Dark
        };

        public ThemeSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("String_Theme").ToUpper();
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("String_Theme"),
                    Items = Options,
                    ItemTemplate = v => new TextBlock
                    { 
                        Text = ((Themes)v).ToLocalizedString()
                    },
                    SelectedItem = VxValue.Create<object>(_theme.Value, v => _theme.Value = (Themes)v)
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

        private void SaveChanges()
        {
            if (_theme.Value != Helpers.Settings.ThemeOverride)
            {
                try
                {
                    Helpers.Settings.ThemeOverride = _theme.Value;

                    TelemetryExtension.Current?.TrackEvent("ChangedTheme", new Dictionary<string, string>()
                    {
                        { "Theme", _theme.Value.ToString() }
                    });

                    // Reset the app so that changes are applied to all the views
                    ThemeExtension.Current.Relaunch();
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }
    }
}
