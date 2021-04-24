using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;
using System.Threading.Tasks;

namespace InterfacesiOS.App
{
    internal class IOSDispatcher : PortableDispatcher
    {
        public override Task RunAsync(Action codeToExecute)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            Action actionWithAwait = delegate
            {
                try
                {
                    codeToExecute();
                    completionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    completionSource.SetException(ex);
                }
            };

            // Asynchronously run it, and we'll flag when our code completed
            NativeiOSApplication.Current.BeginInvokeOnMainThread(actionWithAwait);

            return completionSource.Task;
        }
    }
}