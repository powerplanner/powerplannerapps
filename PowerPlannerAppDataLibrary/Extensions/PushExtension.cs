using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class PushExtension
    {
        public static PushExtension Current { get; set; }

        public abstract Task<string> GetPushChannelUri();
    }
}
