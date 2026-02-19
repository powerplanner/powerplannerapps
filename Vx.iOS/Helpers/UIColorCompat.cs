using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using UIKit;

namespace InterfacesiOS.Helpers
{
    /// <summary>
    /// Code converted from Swift code here: https://github.com/noahsark769/ColorCompatibility/blob/master/Sources/ColorCompatibility.swift
    /// </summary>
    public static class UIColorCompat
    {
        private static bool IsSupported = UIDevice.CurrentDevice.CheckSystemVersion(13, 0);

        public static UIColor LabelColor => IsSupported ? GetLabel() : UIColor.FromRGBA(red: 0.0f, green: 0.0f, blue: 0.0f, alpha: 1.0f);

        public static UIColor SecondaryLabelColor => IsSupported ? GetSecondaryLabel() : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.6f);

        public static UIColor TertiaryLabelColor => IsSupported ? GetTertiaryLabel() : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.3f);

        public static UIColor QuaternaryLabelColor => IsSupported ? GetQuaternaryLabel() : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.18f);

        public static UIColor SystemFillColor => IsSupported ? GetSystemFill() : UIColor.FromRGBA(red: 0.47058823529411764f, green: 0.47058823529411764f, blue: 0.5019607843137255f, alpha: 0.2f);

        public static UIColor SecondarySystemFillColor => IsSupported ? GetSecondarySystemFill() : UIColor.FromRGBA(red: 0.47058823529411764f, green: 0.47058823529411764f, blue: 0.5019607843137255f, alpha: 0.16f);

        public static UIColor TertiarySystemFillColor => IsSupported ? GetTertiarySystemFill() : UIColor.FromRGBA(red: 0.4627450980392157f, green: 0.4627450980392157f, blue: 0.5019607843137255f, alpha: 0.12f);

        public static UIColor QuaternarySystemFillColor => IsSupported ? GetQuaternarySystemFill() : UIColor.FromRGBA(red: 0.4549019607843137f, green: 0.4549019607843137f, blue: 0.5019607843137255f, alpha: 0.08f);

        public static UIColor PlaceholderTextColor => IsSupported ? GetPlaceholderText() : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.3f);

        public static UIColor SystemBackgroundColor => IsSupported ? GetSystemBackground() : UIColor.FromRGBA(red: 1.0f, green: 1.0f, blue: 1.0f, alpha: 1.0f);

        public static UIColor SecondarySystemBackgroundColor => IsSupported ? GetSecondarySystemBackground() : UIColor.FromRGBA(red: 0.9490196078431372f, green: 0.9490196078431372f, blue: 0.9686274509803922f, alpha: 1.0f);

        public static UIColor TertiarySystemBackgroundColor => IsSupported ? GetTertiarySystemBackground() : UIColor.FromRGBA(red: 1.0f, green: 1.0f, blue: 1.0f, alpha: 1.0f);

        public static UIColor SystemGroupedBackgroundColor => IsSupported ? GetSystemGroupedBackground() : UIColor.FromRGBA(red: 0.9490196078431372f, green: 0.9490196078431372f, blue: 0.9686274509803922f, alpha: 1.0f);

        public static UIColor SecondarySystemGroupedBackgroundColor => IsSupported ? GetSecondarySystemGroupedBackground() : UIColor.FromRGBA(red: 1.0f, green: 1.0f, blue: 1.0f, alpha: 1.0f);

        public static UIColor TertiarySystemGroupedBackgroundColor => IsSupported ? GetTertiarySystemGroupedBackground() : UIColor.FromRGBA(red: 0.9490196078431372f, green: 0.9490196078431372f, blue: 0.9686274509803922f, alpha: 1.0f);

        public static UIColor SeparatorColor => IsSupported ? GetSeparator() : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.29f);

        public static UIColor OpaqueSeparatorColor => IsSupported ? GetOpaqueSeparator() : UIColor.FromRGBA(red: 0.7764705882352941f, green: 0.7764705882352941f, blue: 0.7843137254901961f, alpha: 1.0f);

        public static UIColor LinkColor => IsSupported ? GetLink() : UIColor.FromRGBA(red: 0.0f, green: 0.47843137254901963f, blue: 1.0f, alpha: 1.0f);

        public static UIColor SystemIndigoColor => IsSupported ? GetSystemIndigo() : UIColor.FromRGBA(red: 0.34509803921568627f, green: 0.33725490196078434f, blue: 0.8392156862745098f, alpha: 1.0f);

        public static UIColor SystemGray2Color => IsSupported ? GetSystemGray2() : UIColor.FromRGBA(red: 0.6823529411764706f, green: 0.6823529411764706f, blue: 0.6980392156862745f, alpha: 1.0f);

        public static UIColor SystemGray3Color => IsSupported ? GetSystemGray3() : UIColor.FromRGBA(red: 0.7803921568627451f, green: 0.7803921568627451f, blue: 0.8f, alpha: 1.0f);

        public static UIColor SystemGray4Color => IsSupported ? GetSystemGray4() : UIColor.FromRGBA(red: 0.8196078431372549f, green: 0.8196078431372549f, blue: 0.8392156862745098f, alpha: 1.0f);

        public static UIColor SystemGray5Color => IsSupported ? GetSystemGray5() : UIColor.FromRGBA(red: 0.8196078431372549f, green: 0.8196078431372549f, blue: 0.8392156862745098f, alpha: 1.0f);

        public static UIColor SystemGray6Color => IsSupported ? GetSystemGray6() : UIColor.FromRGBA(red: 0.8196078431372549f, green: 0.8196078431372549f, blue: 0.8392156862745098f, alpha: 1.0f);

        // Helper methods to access iOS 13+ APIs
        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetLabel() => UIColor.Label;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSecondaryLabel() => UIColor.SecondaryLabel;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetTertiaryLabel() => UIColor.TertiaryLabel;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetQuaternaryLabel() => UIColor.QuaternaryLabel;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemFill() => UIColor.SystemFill;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSecondarySystemFill() => UIColor.SecondarySystemFill;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetTertiarySystemFill() => UIColor.TertiarySystemFill;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetQuaternarySystemFill() => UIColor.QuaternarySystemFill;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetPlaceholderText() => UIColor.PlaceholderText;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemBackground() => UIColor.SystemBackground;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSecondarySystemBackground() => UIColor.SecondarySystemBackground;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetTertiarySystemBackground() => UIColor.TertiarySystemBackground;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemGroupedBackground() => UIColor.SystemGroupedBackground;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSecondarySystemGroupedBackground() => UIColor.SecondarySystemGroupedBackground;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetTertiarySystemGroupedBackground() => UIColor.TertiarySystemGroupedBackground;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSeparator() => UIColor.Separator;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetOpaqueSeparator() => UIColor.OpaqueSeparator;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetLink() => UIColor.Link;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemIndigo() => UIColor.SystemIndigo;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemGray2() => UIColor.SystemGray2;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemGray3() => UIColor.SystemGray3;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemGray4() => UIColor.SystemGray4;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemGray5() => UIColor.SystemGray5;

        [SupportedOSPlatform("ios13.0")]
        private static UIColor GetSystemGray6() => UIColor.SystemGray6;
    }
}