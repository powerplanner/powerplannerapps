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
        public Action OnBack { get; set; }
        public string BackText { get; set; }
        public Color BackgroundColor { get; set; } = Theme.Current.ChromeColor;
        public Color ForegroundColor { get; set; } = Color.White;
        public List<ToolbarCommand> PrimaryCommands { get; } = new List<ToolbarCommand>();
        public List<ToolbarCommand> SecondaryCommands { get; } = new List<ToolbarCommand>();

        /// <summary>
        /// iOS reverses the primary commands, so the first one is displayed on the right. If you have relational-specific commands,
        /// like Previous and Next, use this to determine if you should reveres those command orders.
        /// </summary>
        public static readonly bool DisplaysPrimaryCommandsRightToLeft = VxPlatform.Current == Platform.iOS;
    }

    public enum ToolbarCommandStyle
    {
        Default,
        Destructive
    }

    public class ToolbarCommand
    {
        public string Text { get; set; }
        public Action Action { get; set; }
        public string Glyph { get; set; }
        public ToolbarCommandStyle Style { get; set; }

        public ToolbarCommand[] SubCommands { get; set; }

        /// <summary>
        /// Does not compare Action, but compares all visual attributes
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool AreSame(IEnumerable<ToolbarCommand> first, IEnumerable<ToolbarCommand> second)
        {
            // If they're both null or both same reference
            if (first == second)
            {
                return true;
            }

            // If only one is null
            if (first == null || second == null)
            {
                return false;
            }

            return first.SequenceEqual(second, Comparer.Instance);
        }

        private class Comparer : IEqualityComparer<ToolbarCommand>
        {
            public static Comparer Instance { get; } = new Comparer();

            public bool Equals(ToolbarCommand x, ToolbarCommand y)
            {
                return x.Text == y.Text
                    && x.Glyph == y.Glyph
                    && x.Style == y.Style;
            }

            public int GetHashCode(ToolbarCommand obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
