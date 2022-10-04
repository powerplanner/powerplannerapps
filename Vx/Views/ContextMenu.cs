using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class ContextMenu
    {
        public List<IContextMenuItem> Items { get; } = new List<IContextMenuItem>();

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

    public class ContextMenuItem : IContextMenuItem
    {
        public string Text { get; set; }
        public Action Click { get; set; }
        public string Glyph { get; set; }
        public ContextMenuItemStyle Style { get; set; }
    }

    public class ContextMenuSeparator : IContextMenuItem
    {

    }

    public class ContextMenuSubItem : IContextMenuItem
    {
        public string Text { get; set; }
        public string Glyph { get; set; }
        public List<IContextMenuItem> Items { get; } = new List<IContextMenuItem>();
    }

    public class ContextMenuRadioItem : IContextMenuItem
    {
        public string Text { get; set; }
        public string GroupName { get; set; }
        public bool IsChecked { get; set; }
        public Action Click { get; set; }
    }

    public interface IContextMenuItem
    {

    }

    public enum ContextMenuItemStyle
    {
        Default,
        Destructive
    }
}
