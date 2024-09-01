using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class WrapGrid : View
    {
        public List<View> Children { get; } = new List<View>();

        public Color BackgroundColor { get; set; }

        /// <summary>
        /// The width of each item. Must be specified.
        /// </summary>
        public float ItemWidth { get; set; } = 40;

        /// <summary>
        /// The height of each item. Must be specified.
        /// </summary>
        public float ItemHeight { get; set; } = 40;
    }
}
