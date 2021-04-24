using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Vx.iOS.Helpers;
using Vx.Views;

namespace Vx.iOS
{
    public class VxiOSTheme : Theme
    {
        public override Color ForegroundColor => UIColorCompat.LabelColor.ToVx();

        public override Color SubtleForegroundColor => UIColorCompat.SecondaryLabelColor.ToVx();
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