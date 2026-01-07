using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    internal class ThemeSettingsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;
        private VxState<Themes> _theme = new VxState<Themes>(Helpers.Settings.ThemeOverride);
        private VxState<bool> _saving = new VxState<bool>();
        private Themes[] Options = new Themes[]
        {
            Themes.Automatic,
            Themes.Light,
            Themes.Dark
        };

        // Default light blue color used when no custom color is set
        private static readonly byte[] DefaultNoClassColor = new byte[] { 84, 107, 199 };

        public ThemeSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("String_Theme").ToUpper();
            _account = MainScreenViewModel.CurrentAccount;
        }

        private Color GetCurrentNoClassColor()
        {
            var color = _account?.NoClassColor ?? DefaultNoClassColor;
            return Color.FromArgb(255, color[0], color[1], color[2]);
        }

        protected override View Render()
        {
            bool hasAccount = _account != null;

            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_AppTheme_Title"),
                    FontSize = Theme.Current.SubtitleFontSize,
                    FontWeight = FontWeights.SemiBold
                },

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_AppTheme_Description"),
                    Margin = new Thickness(0, 6, 0, 0)
                },

                new ComboBox
                {
                    Header = PowerPlannerResources.GetString("String_Theme"),
                    Items = Options,
                    ItemTemplate = v => new TextBlock
                    { 
                        Text = ((Themes)v).ToLocalizedString()
                    },
                    SelectedItem = VxValue.Create<object>(_theme.Value, v => _theme.Value = (Themes)v),
                    Margin = new Thickness(0, 18, 0, 0)
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ButtonSave.Content"),
                    Click = SaveThemeChanges,
                    Margin = new Thickness(0, 18, 0, 0),
                    IsEnabled = _theme.Value != Helpers.Settings.ThemeOverride
                },

                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote.Text"),
                    Margin = new Thickness(0, 9, 0, 0)
                }.CaptionStyle(),

                hasAccount ? new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_NoClassColor_Title"),
                    FontSize = Theme.Current.SubtitleFontSize,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 24, 0, 0)
                } : null,

                hasAccount ? new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_NoClassColor_Description"),
                    Margin = new Thickness(0, 6, 0, 0)
                } : null,

                hasAccount ? new ColorPicker
                {
                    Header = PowerPlannerResources.GetString("AddClassPage_ColorPickerEditClassColor.Header"),
                    Color = VxValue.Create(GetCurrentNoClassColor(), OnColorChanged),
                    IsEnabled = !_saving.Value,
                    Margin = new Thickness(0, 18, 0, 0)
                } : null,

                hasAccount ? new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_NoClassColor_ResetToDefault"),
                    Click = ResetNoClassColorToDefault,
                    IsEnabled = !_saving.Value && _account.NoClassColor != null,
                    Margin = new Thickness(0, 12, 0, 0)
                } : null
            );
        }

        private void SaveThemeChanges()
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

        private async void OnColorChanged(Color color)
        {
            try
            {
                var newColorBytes = new byte[] { color.R, color.G, color.B };

                // Don't save if it's the same color
                if (_account.NoClassColor != null && _account.NoClassColor.Length == 3 &&
                    _account.NoClassColor[0] == newColorBytes[0] &&
                    _account.NoClassColor[1] == newColorBytes[1] &&
                    _account.NoClassColor[2] == newColorBytes[2])
                {
                    return;
                }

                await SaveNoClassColor(newColorBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to process color change: {ex.Message}");
            }
        }

        private async void ResetNoClassColorToDefault()
        {
            try
            {
                await SaveNoClassColor(DefaultNoClassColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset to default color: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task SaveNoClassColor(byte[] colorBytes)
        {
            _saving.Value = true;

            try
            {
                await _account.SaveNoClassColor(colorBytes);

                // Invalidate all semesters' NoClassClass to pick up the new color
                InvalidateNoClassColorInSemesters();

                // Sync the setting to the server
                _ = Sync.SyncSettings(_account, Sync.ChangedSetting.NoClassColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save no class color: {ex.Message}");
            }
            finally
            {
                _saving.Value = false;
            }
        }

        private void InvalidateNoClassColorInSemesters()
        {
            try
            {
                var mainScreen = MainScreenViewModel;
                if (mainScreen?.CurrentSemester != null)
                {
                    mainScreen.CurrentSemester.InvalidateNoClassClass();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to invalidate no class color in semesters: {ex.Message}");
            }
        }
    }
}
