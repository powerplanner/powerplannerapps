using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Core;

namespace ToolsUniversal
{
    public class CoreDispatcherPortableWrapper : PortableDispatcher
    {
        private CoreDispatcher _dispatcher;

        public CoreDispatcherPortableWrapper(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override Task RunAsync(Action codeToExecute)
        {
            return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                codeToExecute.Invoke();
            }).AsTask();
        }
    }
}
