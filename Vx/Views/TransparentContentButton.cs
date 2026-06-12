using System;

namespace Vx.Views
{
    public class TransparentContentButton : View
    {
        public View Content { get; set; }

        public Action Click { get; set; }

        public string AltText { get; set; }

        public string TooltipText { get; set; }
    }
}
