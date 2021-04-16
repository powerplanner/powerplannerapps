using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class TextBlock : View
    {
        public string Text { get; set; } = "";

        public FontWeights FontWeight { get; set; }

        public Color TextColor { get; set; } = Theme.Current.ForegroundColor;
    }
}
