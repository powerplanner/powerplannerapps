using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class Theme
    {
        public static Theme CurrentTheme { get; set; } = Light;

        public static readonly Theme Light = new Theme()
        {
            ForegroundColor = Color.Black,
            BackgroundColor = Color.White,
            ButtonBackgroundColor = Color.Gray
        };

        public Color ForegroundColor { get; set; }

        public Color BackgroundColor { get; set; }

        public Color ButtonBackgroundColor { get; set; }
    }
}
