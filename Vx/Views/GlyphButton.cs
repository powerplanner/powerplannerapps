using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class GlyphButton : View
    {
        public string Glyph { get; set; }

        public Action Click { get; set; }
    }
}
