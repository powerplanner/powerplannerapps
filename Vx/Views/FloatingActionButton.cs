using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    /// <summary>
    /// Only implemented on Android
    /// </summary>
    public class FloatingActionButton : View
    {
        public static readonly float DefaultSize = 56;

        public Action Click { get; set; }

        /// <summary>
        /// This is always Add icon for now
        /// </summary>
        public string Glyph { get; set; }
    }
}
