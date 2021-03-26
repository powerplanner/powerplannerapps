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
            return Current.RunAsync(action);
        }
    }

    public interface IVxDispatcher
    {
        Task RunAsync(Action action);
    }
}
