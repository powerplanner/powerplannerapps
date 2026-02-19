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
        // All properties return true since the minimum supported iOS version is 14.0
        public static bool IsNotificationsSupported => true;
        public static bool IsUIDatePickerInlineStyleSupported => true;
        public static bool IsUIDatePickerCompactStyleSupported => true;
        public static bool IsUIDatePickerWheelsStyleSupported => true;
        public static bool IsVerticalScrollIndicatorInsetsSupported => true;
        public static bool IsUserInterfaceStyleSupported => true;
        public static bool IsSafeAreaInsetsSupported => true;
    }
}