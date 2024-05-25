using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class ProgressBar : View
    {
        public bool IsIndeterminate { get; set; } = false;
        public double Value { get; set; } = 0;
        public double MaxValue { get; set; } = 1;
        public Color Color { get; set; } = Theme.Current.AccentColor;
    }
}
