using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ToolsPortable;

namespace Vx.Views
{
    public abstract class Theme
    {
        public static event EventHandler ThemeChanged;

        private static Theme _currentTheme;
        public static Theme Current
        {
            get
            {
#if DEBUG
                if (_currentTheme == null)
                {
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException("Native platform needs to initialize Theme");
                }
#endif

                return _currentTheme;
            }
            set => _currentTheme = value;
        }

        protected abstract bool IsPlatformDarkTheme { get; }

        public bool IsDarkTheme => IsCustomTheme ? _isCustomThemeDark : IsPlatformDarkTheme;

        public bool IsCustomTheme { get; private set; }
        private bool _isCustomThemeDark;
        private Color _customForegroundColor;
        private Color _customBackgroundColor;
        private Color _customChromeColor;

        public void ApplyColors(Color primaryColor, Color defaultAccentColor, Color defaultDarkAccentColor, bool isCustomTheme, Color customForegroundColor, Color customBackgroundColor, Color customChromeColor)
        {
            PrimaryColor = primaryColor;
            DefaultAccentColor = defaultAccentColor;
            DefaultDarkAccentColor = defaultDarkAccentColor;

            IsCustomTheme = isCustomTheme;
            _isCustomThemeDark = customForegroundColor.GetBrightness() > 0.5f;
            _customForegroundColor = customForegroundColor;
            _customBackgroundColor = customBackgroundColor;
            _customChromeColor = customChromeColor;

            ForegroundColor = IsCustomTheme ? _customForegroundColor : IsDarkTheme ? Color.White : Color.Black;
            SubtleForegroundColor = Color.FromArgb(179, ForegroundColor.R, ForegroundColor.G, ForegroundColor.B);
            ChromeColor = IsCustomTheme ? _customChromeColor : IsDarkTheme ? DarkThemeChromeColor : LightThemeChromeColor;
            ChromeLightColor = ColorHelper.AdjustLightnessBlend(ChromeColor, IsDarkTheme ? 15 : -15);
            BackgroundColor = IsCustomTheme ? _customBackgroundColor : IsDarkTheme ? Color.FromArgb(30, 30, 30) : Color.White;
            BackgroundAlt1Color = ColorHelper.AdjustLightnessBlend(BackgroundColor, IsDarkTheme ? 13 : -10);
            BackgroundAlt2Color = ColorHelper.AdjustLightnessBlend(BackgroundColor, IsDarkTheme ? 3 : -3);

            // PROBLEM: The BackroundAlt1Color doesn't look as good on light theme as it used to in the prev version... I want it to be bluer, closer to the chrome color...

            NotifyThemeChanged();
        }

        public Color PrimaryColor { get; private set; } = Color.Blue;

        public static Color DefaultAccentColor { get; private set; } = Color.Blue;

        /// <summary>
        /// Accent color for light theme so shows up on dark themes
        /// </summary>
        public static Color DefaultDarkAccentColor { get; private set; } = Color.Blue;

        /// <summary>
        /// Background color of buttons, titlebars, etc
        /// </summary>
        public Color ChromeColor { get; private set; }

        public Color DarkThemeChromeColor { get; private set; } = Color.FromArgb(15, 15, 15);

        public Color LightThemeChromeColor { get; private set; } = Color.FromArgb(239, 244, 249);

        /// <summary>
        /// Lighter variant of <see cref="ChromeColor"/>, used for ripple/pressed states.
        /// </summary>
        public Color ChromeLightColor { get; private set; }

        public Color ForegroundColor { get; private set; }

        public Color SubtleForegroundColor { get; private set; }

        public Color BackgroundColor { get; private set; }

        public Color PopupPageBackgroundColor => BackgroundColor;

        public Color PopupPageBackgroundAltColor => ChromeColor;

        public Color AccentColor => IsDarkTheme ? DefaultDarkAccentColor : DefaultAccentColor;

        public Color BackgroundAlt1Color { get; private set; }

        public Color BackgroundAlt2Color { get; private set; }

        private static Lazy<float> _pageMargin = new Lazy<float>(() =>
        {
            if (VxPlatform.Current == Platform.Android)
            {
                return 16;
            }
            else if (VxPlatform.Current == Platform.iOS)
            {
                return 20;
            }
            else
            {
                return 24;
            }
        });

        /// <summary>
        /// Windows uses 24px for typical page margins, and then sometimes 16px margins between content? And sometimes 12.
        /// iOS seems to use 20px for typical page margins and seems to work in units of 10.
        /// Android typically works in units of 8dp, only 16dp for page margins, but otherwise straightforward with Windows?
        /// </summary>
        public float PageMargin => _pageMargin.Value;

        public float SmallPageMargin => 6;

        public float CornerRadius => 9;

        public virtual float CaptionFontSize => 12;

        public virtual float BodyFontSize => 14;

        public virtual float SubtitleFontSize => 20;

        public virtual float TitleFontSize => 24;

        public virtual float HeaderFontSize => 38;

        public static void NotifyThemeChanged()
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
