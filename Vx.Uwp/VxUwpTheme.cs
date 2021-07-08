using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Vx.Uwp
{
    public class VxUwpTheme : Theme
    {
        public override Color ForegroundColor => (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).ToVx();

        public override Color SubtleForegroundColor => (Application.Current.Resources["ApplicationSecondaryForegroundThemeBrush"] as SolidColorBrush).ToVx();

        public override Color PopupPageBackgroundColor => (Application.Current.Resources["PopupBackground"] as SolidColorBrush).ToVx();

        public override Color PopupPageBackgroundAltColor => (Application.Current.Resources["ToolTipBackgroundThemeBrush"] as SolidColorBrush).ToVx();
    }

    internal static class UwpThemeExtensions
    {
        public static Color ToVx(this SolidColorBrush brush)
        {
            return Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}
