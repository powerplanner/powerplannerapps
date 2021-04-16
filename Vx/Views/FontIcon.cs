using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class FontIcon : View
    {
        public string Glyph { get; set; }

        public Color Color { get; set; } = Theme.Current.ForegroundColor;

        public double FontSize { get; set; }
    }
}
