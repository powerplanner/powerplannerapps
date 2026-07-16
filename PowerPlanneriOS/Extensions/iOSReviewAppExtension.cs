using Foundation;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Extensions
{
    internal class iOSReviewAppExtension : ReviewAppExtension
    {
        public override async Task ReviewAppAsync()
        {
            var url = $"itms-apps://itunes.apple.com/app/id1278178608?action=write-review";
            bool opened = false;
            try
            {
                opened = await UIApplication.SharedApplication.OpenUrlAsync(new NSUrl(url), new UIApplicationOpenUrlOptions());
            }
            catch { }

            if (!opened)
            {
                var dontWait = new PortableMessageDialog("The Store failed to open. Try again later.", "Unable to open the Store").ShowAsync();
            }
        }
    }
}
