using BareMvvm.Core.App;
using InterfacesUWP.AppWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Core;

namespace InterfacesUWP.App
{
    public class UwpDispatcher : PortableDispatcher
    {
        public override async Task RunAsync(Action codeToExecute)
        {
            NativeUwpAppWindow window = PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow as NativeUwpAppWindow;

            if (window != null)
            {
                await window.Window.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { codeToExecute(); });
            }
            else
            {
                codeToExecute();
            }
        }
    }
}
