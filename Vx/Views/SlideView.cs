﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class SlideView : View
    {
        public VxValue<int> Position { get; set; }

        public Func<int, View> ItemTemplate { get; set; }

        public int? MinPosition { get; set; }
        public int? MaxPosition { get; set; }

        public Color BackgroundColor { get; set; }

        public bool ShowMouseArrowIndicatorsOnHover { get; set; }
    }
}
