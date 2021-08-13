using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class EmailExtension
    {
        public static EmailExtension Current { get; set; }

        public abstract Task ComposeNewMailAsync(string to, string subject);
    }
}
