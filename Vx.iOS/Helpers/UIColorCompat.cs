﻿using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static UIColor LabelColor => IsSupported ? UIColor.Label : UIColor.FromRGBA(red: 0.0f, green: 0.0f, blue: 0.0f, alpha: 1.0f);

        public static UIColor SecondaryLabelColor => IsSupported ? UIColor.SecondaryLabel : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.6f);

        public static UIColor TertiaryLabelColor => IsSupported ? UIColor.TertiaryLabel : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.3f);

        public static UIColor QuaternaryLabelColor => IsSupported ? UIColor.QuaternaryLabel : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.18f);

        public static UIColor SystemFillColor => IsSupported ? UIColor.SystemFill : UIColor.FromRGBA(red: 0.47058823529411764f, green: 0.47058823529411764f, blue: 0.5019607843137255f, alpha: 0.2f);

        public static UIColor SecondarySystemFillColor => IsSupported ? UIColor.SecondarySystemFill : UIColor.FromRGBA(red: 0.47058823529411764f, green: 0.47058823529411764f, blue: 0.5019607843137255f, alpha: 0.16f);

        public static UIColor TertiarySystemFillColor => IsSupported ? UIColor.TertiarySystemFill : UIColor.FromRGBA(red: 0.4627450980392157f, green: 0.4627450980392157f, blue: 0.5019607843137255f, alpha: 0.12f);

        public static UIColor QuaternarySystemFillColor => IsSupported ? UIColor.QuaternarySystemFill : UIColor.FromRGBA(red: 0.4549019607843137f, green: 0.4549019607843137f, blue: 0.5019607843137255f, alpha: 0.08f);

        public static UIColor PlaceholderTextColor => IsSupported ? UIColor.PlaceholderText : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.3f);

        public static UIColor SystemBackgroundColor => IsSupported ? UIColor.SystemBackground : UIColor.FromRGBA(red: 1.0f, green: 1.0f, blue: 1.0f, alpha: 1.0f);

        public static UIColor SecondarySystemBackgroundColor => IsSupported ? UIColor.SecondarySystemBackground : UIColor.FromRGBA(red: 0.9490196078431372f, green: 0.9490196078431372f, blue: 0.9686274509803922f, alpha: 1.0f);

        public static UIColor TertiarySystemBackgroundColor => IsSupported ? UIColor.TertiarySystemBackground : UIColor.FromRGBA(red: 1.0f, green: 1.0f, blue: 1.0f, alpha: 1.0f);

        public static UIColor SystemGroupedBackgroundColor => IsSupported ? UIColor.SystemGroupedBackground : UIColor.FromRGBA(red: 0.9490196078431372f, green: 0.9490196078431372f, blue: 0.9686274509803922f, alpha: 1.0f);

        public static UIColor SecondarySystemGroupedBackgroundColor => IsSupported ? UIColor.SecondarySystemGroupedBackground : UIColor.FromRGBA(red: 1.0f, green: 1.0f, blue: 1.0f, alpha: 1.0f);

        public static UIColor TertiarySystemGroupedBackgroundColor => IsSupported ? UIColor.TertiarySystemGroupedBackground : UIColor.FromRGBA(red: 0.9490196078431372f, green: 0.9490196078431372f, blue: 0.9686274509803922f, alpha: 1.0f);

        public static UIColor SeparatorColor => IsSupported ? UIColor.Separator : UIColor.FromRGBA(red: 0.23529411764705882f, green: 0.23529411764705882f, blue: 0.2627450980392157f, alpha: 0.29f);

        public static UIColor OpaqueSeparatorColor => IsSupported ? UIColor.OpaqueSeparator : UIColor.FromRGBA(red: 0.7764705882352941f, green: 0.7764705882352941f, blue: 0.7843137254901961f, alpha: 1.0f);

        public static UIColor LinkColor => IsSupported ? UIColor.Link : UIColor.FromRGBA(red: 0.0f, green: 0.47843137254901963f, blue: 1.0f, alpha: 1.0f);

        public static UIColor SystemIndigoColor => IsSupported ? UIColor.SystemIndigo : UIColor.FromRGBA(red: 0.34509803921568627f, green: 0.33725490196078434f, blue: 0.8392156862745098f, alpha: 1.0f);

        public static UIColor SystemGray2Color => IsSupported ? UIColor.SystemGray2 : UIColor.FromRGBA(red: 0.6823529411764706f, green: 0.6823529411764706f, blue: 0.6980392156862745f, alpha: 1.0f);

        public static UIColor SystemGray3Color => IsSupported ? UIColor.SystemGray3 : UIColor.FromRGBA(red: 0.7803921568627451f, green: 0.7803921568627451f, blue: 0.8f, alpha: 1.0f);

        public static UIColor SystemGray4Color => IsSupported ? UIColor.SystemGray4 : UIColor.FromRGBA(red: 0.8196078431372549f, green: 0.8196078431372549f, blue: 0.8392156862745098f, alpha: 1.0f);

        public static UIColor SystemGray5Color => IsSupported ? UIColor.SystemGray5 : UIColor.FromRGBA(red: 0.8196078431372549f, green: 0.8196078431372549f, blue: 0.8392156862745098f, alpha: 1.0f);

        public static UIColor SystemGray6Color => IsSupported ? UIColor.SystemGray6 : UIColor.FromRGBA(red: 0.8196078431372549f, green: 0.8196078431372549f, blue: 0.8392156862745098f, alpha: 1.0f);
    }
}