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
        public override Task RunAsync(Action codeToExecute)
        {
            NativeUwpAppWindow window = PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow as NativeUwpAppWindow;

            if (window != null)
            {
                var taskCompletionSource = new TaskCompletionSource();
                window.DispatcherQueue.TryEnqueue(delegate
                {
                    try
                    {
                        codeToExecute();
                        taskCompletionSource.SetResult();
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                });
                return taskCompletionSource.Task;
            }
            else
            {
                codeToExecute();
                return Task.CompletedTask;
            }
        }
    }
}
