using System;
using System.Drawing;

namespace Vx.Views
{
    public class TransparentContentButton : View
    {
        public View Content { get; set; }

        public Action Click { get; set; }

        public string AltText { get; set; }

        public string TooltipText { get; set; }

        public float CornerRadius { get; set; }

        public Color BackgroundColor { get; set; }
    }
}
