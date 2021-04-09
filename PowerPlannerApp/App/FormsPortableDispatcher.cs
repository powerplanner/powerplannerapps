using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Xamarin.Essentials;

namespace PowerPlannerApp.App
{
    public class FormsPortableDispatcher : PortableDispatcher
    {
        public static readonly FormsPortableDispatcher Current = new FormsPortableDispatcher();

        public override Task RunAsync(Action codeToExecute)
        {
            return MainThread.InvokeOnMainThreadAsync(codeToExecute);
        }
    }
}
