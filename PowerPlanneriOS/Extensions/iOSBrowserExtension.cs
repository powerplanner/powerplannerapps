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
            return UIApplication.SharedApplication.OpenUrlAsync(new NSUrl(uri.ToString()), new UIApplicationOpenUrlOptions());
        }
    }
}
