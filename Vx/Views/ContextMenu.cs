using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class ContextMenu
    {
        public List<ContextMenuItem> Items { get; } = new List<ContextMenuItem>();

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

    public class ContextMenuItem
    {
        public string Text { get; set; }
        public Action Click { get; set; }
    }
}
