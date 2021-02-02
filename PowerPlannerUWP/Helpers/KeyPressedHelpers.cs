using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace PowerPlannerUWP.Helpers
{
    static class KeyPressedHelpers
    {
        // From: https://docs.microsoft.com/en-us/windows/uwp/design/input/keyboard-events#shortcut-keys-example
        /// <summary>
        /// Is the "Control" key pressed (held down)?
        /// </summary>
        /// <returns>True if Ctrl key is held down</returns>
        public static bool IsCtrlKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
    }
}
