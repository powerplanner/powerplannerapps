using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibraryUnitTests
{
    public class DummyDispatcher : PortableDispatcher
    {
        public override Task RunAsync(Action codeToExecute)
        {
            codeToExecute();
            return Task.CompletedTask;
        }
    }
}
