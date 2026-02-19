using Foundation;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Extensions
{
    public class iOSEmailExtension : EmailExtension
    {
        public override async Task ComposeNewMailAsync(string to, string subject)
        {
            NSUrl url = new NSUrl($"mailto:?to={Uri.EscapeDataString(to)}&subject={Uri.EscapeDataString(subject)}");

            if (!await UIApplication.SharedApplication.OpenUrlAsync(url, new UIApplicationOpenUrlOptions()))
            {
                _ = new PortableMessageDialog("Looks like you don't have email configured on your phone. You'll need to set up email before you can send an email.", "Email app not configured").ShowAsync();
            }
        }
    }
}