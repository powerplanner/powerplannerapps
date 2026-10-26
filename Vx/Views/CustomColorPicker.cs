using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class CustomColorPicker : View
    {
        public string Header { get; set; }

        public bool IsEnabled { get; set; } = true;

        public VxValue<Color> Color { get; set; }
    }
}
