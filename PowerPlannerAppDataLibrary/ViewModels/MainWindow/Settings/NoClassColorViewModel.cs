using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Drawing;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class NoClassColorViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;
        private VxState<bool> _saving = new VxState<bool>();

        // Default light blue color used when no custom color is set
        private static readonly byte[] DefaultNoClassColor = new byte[] { 84, 107, 199 };

        public NoClassColorViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_NoClassColor_Title");
            _account = MainScreenViewModel.CurrentAccount;
        }

        private Color GetCurrentColor()
        {
            var color = _account.NoClassColor ?? DefaultNoClassColor;
            return Color.FromArgb(255, color[0], color[1], color[2]);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_NoClassColor_Description"),
                    Margin = new Thickness(0, 0, 0, 24)
                },

                new ColorPicker
                {
                    Header = PowerPlannerResources.GetString("Settings_NoClassColor_ColorPicker_Header"),
                    Color = VxValue.Create(GetCurrentColor(), OnColorChanged),
                    IsEnabled = !_saving.Value
                },

                new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_NoClassColor_ResetToDefault"),
                    Click = ResetToDefault,
                    IsEnabled = !_saving.Value && _account.NoClassColor != null,
                    Margin = new Thickness(0, 18, 0, 0)
                }
            );
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

                await SaveColor(newColorBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to process color change: {ex.Message}");
            }
        }

        private async void ResetToDefault()
        {
            try
            {
                await SaveColor(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset to default color: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task SaveColor(byte[] colorBytes)
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
                // Show error or handle as appropriate
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
                if (mainScreen?.CurrentYear?.Semesters != null)
                {
                    foreach (var semester in mainScreen.CurrentYear.Semesters)
                    {
                        semester.InvalidateNoClassClass();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to invalidate no class color in semesters: {ex.Message}");
            }
        }
    }
}