using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    /// <summary>
    /// Adaptive grid panel that automatically arranges children into columns based on available width.
    /// Supported on UWP, Android, and iOS.
    /// </summary>
    public class AdaptiveGridPanel : View
    {
        public List<View> Children { get; } = new List<View>();

        public float MinColumnWidth { get; set; } = 250;

        public float ColumnSpacing { get; set; } = 24;
    }
}
