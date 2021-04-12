using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Xamarin.Essentials;

namespace BareMvvm.Core.App
{
    public class FormsDispatcher : PortableDispatcher
    {
        public override Task RunAsync(Action codeToExecute)
        {
            return MainThread.InvokeOnMainThreadAsync(codeToExecute);
        }
    }
}
