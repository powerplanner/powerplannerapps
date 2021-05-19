using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Helpers
{
    public static class SdkSupportHelper
    {
        private static bool CheckSystemVersion(int major, int minor)
        {
            return UIDevice.CurrentDevice.CheckSystemVersion(major, minor);
        }

        public static bool IsNotificationsSupported => CheckSystemVersion(10, 0);
    }
}