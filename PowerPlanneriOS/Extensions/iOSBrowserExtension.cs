using System;
using System.Threading.Tasks;
using Foundation;
using PowerPlannerAppDataLibrary.Extensions;
using UIKit;

namespace PowerPlanneriOS.Extensions
{
    public class iOSBrowserExtension : BrowserExtension
    {
        public override Task OpenUrlAsync(Uri uri)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(uri.ToString()));
            return Task.CompletedTask;
        }
    }
}
