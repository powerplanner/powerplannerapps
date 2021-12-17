using System;
using System.Collections.Generic;
using System.Drawing;

namespace Vx.Views
{
    public class FrameLayout : View
    {
        public List<View> Children { get; } = new List<View>();

        public Color BackgroundColor { get; set; }
    }
}
