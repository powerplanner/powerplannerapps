using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    internal class ThemeSettingsViewModel : PopupComponentViewModel
    {
        public class ThemeColorOption
        {
            public string Name { get; }
            public Color Color { get; }

            public ThemeColorOption(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }

        public static readonly ThemeColorOption[] ColorOptions = new ThemeColorOption[]
        {
            new ThemeColorOption("Default Blue", Color.FromArgb(46, 54, 109)),    // #2E366D
            new ThemeColorOption("Ocean Blue", Color.FromArgb(21, 101, 192)),     // #1565C0
            new ThemeColorOption("Teal", Color.FromArgb(0, 121, 107)),            // #00796B
            new ThemeColorOption("Green", Color.FromArgb(46, 125, 50)),           // #2E7D32
            new ThemeColorOption("Purple", Color.FromArgb(106, 27, 154)),         // #6A1B9A
            new ThemeColorOption("Deep Purple", Color.FromArgb(69, 39, 160)),     // #4527A0
            new ThemeColorOption("Indigo", Color.FromArgb(40, 53, 147)),          // #283593
            new ThemeColorOption("Red", Color.FromArgb(198, 40, 40)),             // #C62828
            new ThemeColorOption("Pink", Color.FromArgb(173, 20, 87)),            // #AD1457
            new ThemeColorOption("Orange", Color.FromArgb(230, 81, 0)),           // #E65100
            new ThemeColorOption("Brown", Color.FromArgb(78, 52, 46)),            // #4E342E
            new ThemeColorOption("Blue Grey", Color.FromArgb(55, 71, 79)),        // #37474F
            new ThemeColorOption("Dark Grey", Color.FromArgb(66, 66, 66)),        // #424242
        };

        private VxState<Themes> _theme = new VxState<Themes>(Helpers.Settings.ThemeOverride);
        private Themes[] Options = new Themes[]
        {
            Themes.Automatic,
            Themes.Light,
            Themes.Dark
        };

        private VxState<Color> _selectedColor;
        private AccountDataItem _account;

        public ThemeSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("String_Theme").ToUpper();

            _account = FindAncestor<MainScreenViewModel>()?.CurrentAccount;

            var currentColor = _account?.PrimaryThemeColor?.ToColor() ?? ThemeColorGenerator.DefaultPrimary;
            if (currentColor.A == 0 && currentColor.R == 0 && currentColor.G == 0 && currentColor.B == 0)
            {
                currentColor = ThemeColorGenerator.DefaultPrimary;
            }

            _selectedColor = new VxState<Color>(currentColor);
        }

        private Color OriginalColor => _account?.PrimaryThemeColor?.ToColor() is Color c && c.A != 0
            ? c
            : ThemeColorGenerator.DefaultPrimary;

        private bool IsThemeDirty => _theme.Value != Helpers.Settings.ThemeOverride;
        private bool IsColorDirty => _selectedColor.Value != OriginalColor;
        private bool IsDirty => IsThemeDirty || IsColorDirty;

        protected override View Render()
        {
            var content = new List<View>();

            // Light/dark/auto theme selector
            content.Add(new ComboBox
            {
                Header = PowerPlannerResources.GetString("String_Theme"),
                Items = Options,
                ItemTemplate = v => new TextBlock
                {
                    Text = ((Themes)v).ToLocalizedString()
                },
                SelectedItem = VxValue.Create<object>(_theme.Value, v => _theme.Value = (Themes)v)
            });

            // Theme color section (only when logged in)
            if (_account != null)
            {
                var previewColors = ThemeColorGenerator.Generate(_selectedColor.Value);

                content.Add(new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_ThemeColor_Title"),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 24, 0, 6)
                });

                // Color swatch grid
                var swatchGrid = new WrapGrid
                {
                    ItemWidth = 48,
                    ItemHeight = 48
                };

                foreach (var option in ColorOptions)
                {
                    bool isSelected = option.Color == _selectedColor.Value;

                    swatchGrid.Children.Add(new Border
                    {
                        BackgroundColor = option.Color,
                        CornerRadius = 24,
                        Width = 40,
                        Height = 40,
                        Margin = new Thickness(4),
                        Content = isSelected ? new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.Check,
                            FontSize = 20,
                            Color = Color.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        } : null
                    }.WrapWithClick(() => _selectedColor.Value = option.Color));
                }

                content.Add(swatchGrid);

                // Preview strip
                content.Add(new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_ThemeColor_Preview"),
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 18, 0, 6)
                });

                content.Add(new Border
                {
                    BackgroundColor = previewColors.Primary,
                    CornerRadius = 8,
                    Height = 48,
                    Content = new TextBlock
                    {
                        Text = GetSelectedColorName(),
                        TextColor = Color.White,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                });
            }

            // Save button
            content.Add(new AccentButton
            {
                Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ButtonSave.Content"),
                Click = SaveChanges,
                Margin = new Thickness(0, 18, 0, 0),
                IsEnabled = IsDirty
            });

            if (IsThemeDirty)
            {
                content.Add(new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote.Text"),
                    Margin = new Thickness(0, 9, 0, 0)
                }.CaptionStyle());
            }

            return RenderGenericPopupContent(content);
        }

        private string GetSelectedColorName()
        {
            var match = ColorOptions.FirstOrDefault(o => o.Color == _selectedColor.Value);
            return match?.Name ?? "Custom";
        }

        private async void SaveChanges()
        {
            try
            {
                // Save theme color if changed
                if (IsColorDirty && _account != null)
                {
                    var color = _selectedColor.Value;
                    byte[] colorBytes;

                    if (color == ThemeColorGenerator.DefaultPrimary)
                    {
                        colorBytes = null;
                    }
                    else
                    {
                        colorBytes = new byte[] { color.R, color.G, color.B };
                    }

                    await _account.SetPrimaryThemeColorAsync(colorBytes);
                    ThemeColorApplier.Apply(_account);

                    TelemetryExtension.Current?.TrackEvent("ChangedThemeColor", new Dictionary<string, string>()
                    {
                        { "Color", GetSelectedColorName() }
                    });
                }

                // Save light/dark theme if changed (triggers relaunch)
                if (IsThemeDirty)
                {
                    Helpers.Settings.ThemeOverride = _theme.Value;

                    TelemetryExtension.Current?.TrackEvent("ChangedTheme", new Dictionary<string, string>()
                    {
                        { "Theme", _theme.Value.ToString() }
                    });

                    ThemeExtension.Current.Relaunch();
                    return; // Relaunch will close everything
                }

                RemoveViewModel();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }

    internal static class ThemeViewExtensions
    {
        public static View WrapWithClick(this View view, Action click)
        {
            return new ListItemButton
            {
                Content = view,
                Click = click
            };
        }
    }
}
