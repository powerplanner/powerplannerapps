using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace InterfacesiOS.Helpers
{
    /// <summary>
    /// Provides semantic color names. All colors are available since iOS 13+, and the app's minimum version is iOS 14.
    /// </summary>
    public static class UIColorCompat
    {
        public static UIColor LabelColor => UIColor.Label;
        public static UIColor SecondaryLabelColor => UIColor.SecondaryLabel;
        public static UIColor TertiaryLabelColor => UIColor.TertiaryLabel;
        public static UIColor QuaternaryLabelColor => UIColor.QuaternaryLabel;
        public static UIColor SystemFillColor => UIColor.SystemFill;
        public static UIColor SecondarySystemFillColor => UIColor.SecondarySystemFill;
        public static UIColor TertiarySystemFillColor => UIColor.TertiarySystemFill;
        public static UIColor QuaternarySystemFillColor => UIColor.QuaternarySystemFill;
        public static UIColor PlaceholderTextColor => UIColor.PlaceholderText;
        public static UIColor SystemBackgroundColor => UIColor.SystemBackground;
        public static UIColor SecondarySystemBackgroundColor => UIColor.SecondarySystemBackground;
        public static UIColor TertiarySystemBackgroundColor => UIColor.TertiarySystemBackground;
        public static UIColor SystemGroupedBackgroundColor => UIColor.SystemGroupedBackground;
        public static UIColor SecondarySystemGroupedBackgroundColor => UIColor.SecondarySystemGroupedBackground;
        public static UIColor TertiarySystemGroupedBackgroundColor => UIColor.TertiarySystemGroupedBackground;
        public static UIColor SeparatorColor => UIColor.Separator;
        public static UIColor OpaqueSeparatorColor => UIColor.OpaqueSeparator;
        public static UIColor LinkColor => UIColor.Link;
        public static UIColor SystemIndigoColor => UIColor.SystemIndigo;
        public static UIColor SystemGray2Color => UIColor.SystemGray2;
        public static UIColor SystemGray3Color => UIColor.SystemGray3;
        public static UIColor SystemGray4Color => UIColor.SystemGray4;
        public static UIColor SystemGray5Color => UIColor.SystemGray5;
        public static UIColor SystemGray6Color => UIColor.SystemGray6;
    }
}