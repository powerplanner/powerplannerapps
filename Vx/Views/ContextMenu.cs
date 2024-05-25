using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Views
{
    public class ContextMenu
    {
        public List<IMenuItem> Items { get; } = new List<IMenuItem>();

        public void Show(View view)
        {
#if DEBUG
            if (NativeView.ShowContextMenu == null)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
            NativeView.ShowContextMenu(this, view);
        }
    }

    public class MenuItem : IMenuItem
    {
        public string Text { get; set; }
        public Action Click { get; set; }
        public string Glyph { get; set; }
        public MenuItemStyle Style { get; set; }
        public List<IMenuItem> SubItems { get; } = new List<IMenuItem>();

        /// <summary>
        /// Does not compare Action, but compares all visual attributes
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool AreSame(IEnumerable<MenuItem> first, IEnumerable<MenuItem> second)
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

        private class Comparer : IEqualityComparer<MenuItem>
        {
            public static Comparer Instance { get; } = new Comparer();

            public bool Equals(MenuItem x, MenuItem y)
            {
                return x.Text == y.Text
                    && x.Glyph == y.Glyph
                    && x.Style == y.Style;
            }

            public int GetHashCode(MenuItem obj)
            {
                return obj.GetHashCode();
            }
        }
    }

    public class MenuSeparator : IMenuItem
    {

    }

    public class MenuRadioItem : IMenuItem
    {
        public string Text { get; set; }
        public string GroupName { get; set; }
        public bool IsChecked { get; set; }
        public Action Click { get; set; }
    }

    public interface IMenuItem
    {

    }

    public enum MenuItemStyle
    {
        Default,
        Destructive
    }
}
