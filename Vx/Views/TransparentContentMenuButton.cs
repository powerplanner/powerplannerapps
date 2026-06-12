using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    /// <summary>
    /// A button that displays transparent content and has a menu.
    /// Currently only implemented on iOS. Implemented as a separate control since if the inner view is complicated, it doesn't lay out correctly. Works well for Glyphs.
    /// </summary>
    public class TransparentContentMenuButton : View
    {
        public View Content { get; set; }

        public string AltText { get; set; }

        public string TooltipText { get; set; }

        public List<IMenuItem> Menu { get; set; }
    }
}
