using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace PowerPlannerUWP.Extensions
{
    public class UWPBrowserExtension : BrowserExtension
    {
        public override async Task OpenUrlAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
