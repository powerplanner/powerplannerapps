using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public abstract class Theme
    {
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

        public static Color DefaultAccentColor { get; set; } = Color.Blue;

        public abstract Color ForegroundColor { get; }

        public abstract Color SubtleForegroundColor { get; }

        public abstract Color PopupPageBackgroundColor { get; }

        public abstract Color PopupPageBackgroundAltColor { get; }

        public Color AccentColor => DefaultAccentColor;

        private static Lazy<float> _pageMargin = new Lazy<float>(() =>
        {
            if (VxPlatform.Current == Platform.Android)
            {
                return 16;
            }
            else
            {
                return 24; // iOS will get down-converted to 20 automatically
            }
        });

        /// <summary>
        /// Windows uses 24px for typical page margins, and then sometimes 16px margins between content? And sometimes 12.
        /// iOS seems to use 20px for typical page margins and seems to work in units of 10.
        /// Android typically works in units of 8dp, only 16dp for page margins, but otherwise straightforward with Windows?
        /// </summary>
        public float PageMargin => _pageMargin.Value;

        public float CaptionFontSize => 12;

        public float BodyFontSize => 14;

        public float TitleFontSize => 24;

        private static Lazy<float> _marginModifier = new Lazy<float>(() =>
        {
            switch (VxPlatform.Current)
            {
                case Platform.Uwp:
                    return 1;

                case Platform.iOS:
                    return 20f / 24f; // iOS margins should be 20px, everything gets downgraded. Realistically even sizes like buttons should be downgraded (Android usually recommends 48dp, iOS recommends 44, so similar downsize factor. If I only do it for margins, other UI would be inconsistent.

                case Platform.Android:
                    return 1;

                default:
                    throw new NotImplementedException();
            }
        });
        public float MarginModifier => _marginModifier.Value;
    }
}
