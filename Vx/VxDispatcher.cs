using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vx
{
    public class VxDispatcher
    {
        public static IVxDispatcher Current { get; set; }

        public static Task RunAsync(Action action)
        {
#if DEBUG
            if (Current == null)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
            return Current.RunAsync(action);
        }
    }

    public interface IVxDispatcher
    {
        Task RunAsync(Action action);
    }
}
