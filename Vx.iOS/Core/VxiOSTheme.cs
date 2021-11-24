using Foundation;
using InterfacesiOS.App;
using InterfacesiOS.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS
{
    public class VxiOSTheme : Theme
    {
        public override Color ForegroundColor => UIColorCompat.LabelColor.ToVx();

        public override Color SubtleForegroundColor => UIColorCompat.SecondaryLabelColor.ToVx();

        public override Color PopupPageBackgroundColor => UIColorCompat.SystemBackgroundColor.ToVx();

        public override Color PopupPageBackgroundAltColor => UIColorCompat.SystemGroupedBackgroundColor.ToVx();

        public override bool IsDarkTheme => SdkSupportHelper.IsUserInterfaceStyleSupported ? NativeiOSApplication.Current.Window.TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark : false;

        public override float BodyFontSize => (float)UIFont.PreferredBody.PointSize;

        public override float CaptionFontSize => (float)UIFont.PreferredCaption1.PointSize;

        public override float TitleFontSize => (float)UIFont.PreferredTitle1.PointSize;
    }

    internal static class iOSThemeExtensions
    {
        public static Color ToVx(this UIColor color)
        {
            color.GetRGBA(out nfloat r, out nfloat g, out nfloat b, out nfloat a);

            r *= 255;
            g *= 255;
            b *= 255;
            a *= 255;

            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }
    }
}