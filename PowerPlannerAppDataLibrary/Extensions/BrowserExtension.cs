using System;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class BrowserExtension
    {
        public static BrowserExtension Current { get; set; }

        public abstract Task OpenUrlAsync(Uri uri);
    }
}
