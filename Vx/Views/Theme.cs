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

        /// <summary>
        /// Windows uses 24px for typical page margins, and then sometimes 16px margins between content? And sometimes 12.
        /// </summary>
        public static float DefaultPageMargin { get; set; } = 24;

        public abstract Color ForegroundColor { get; }

        public abstract Color SubtleForegroundColor { get; }

        public Color AccentColor => DefaultAccentColor;

        public float PageMargin => DefaultPageMargin;

        public float BodyFontSize => 14;

        public float TitleFontSize => 24;
    }
}
