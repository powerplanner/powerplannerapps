using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class Border : View
    {
        public Color BackgroundColor { get; set; }

        public Thickness BorderThickness { get; set; }

        public Color BorderColor { get; set; }

        public Thickness Padding { get; set; }

        public View Content { get; set; }
    }
}
