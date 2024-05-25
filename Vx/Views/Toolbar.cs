using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Vx.Views
{
    public class Toolbar : View
    {
        public string Title { get; set; }
        /// <summary>
        /// CustomTitle is only supported on UWP right now
        /// </summary>
        public View CustomTitle { get; set; }
        public Action OnBack { get; set; }
        public string BackText { get; set; }
        public Color BackgroundColor { get; set; } = Theme.Current.ChromeColor;
        public Color ForegroundColor { get; set; } = Color.White;
        public List<MenuItem> PrimaryCommands { get; } = new List<MenuItem>();
        public List<MenuItem> SecondaryCommands { get; } = new List<MenuItem>();

        /// <summary>
        /// Setting this displays a Close button on the far right (useful for popup toolbars). Currently only implemented on UWP.
        /// </summary>
        public Action OnClose { get; set; }

        /// <summary>
        /// iOS reverses the primary commands, so the first one is displayed on the right. If you have relational-specific commands,
        /// like Previous and Next, use this to determine if you should reveres those command orders.
        /// </summary>
        public static readonly bool DisplaysPrimaryCommandsRightToLeft = VxPlatform.Current == Platform.iOS;
    }
}
