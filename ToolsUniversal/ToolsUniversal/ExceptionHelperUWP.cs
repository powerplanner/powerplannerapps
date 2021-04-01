using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsUniversal
{
    public static class ExceptionHelperUWP
    {
        public static bool IsRpcServerUnavailable(Exception ex)
        {
            return unchecked((uint)ex.HResult == 0x800706BA);
        }

        public static bool IsRpcIssue(Exception ex)
        {
            return IsRpcServerUnavailable(ex);
        }
    }
}
