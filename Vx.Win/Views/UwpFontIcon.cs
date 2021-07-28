using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Vx.Uwp.Views
{
    public class UwpFontIcon : UwpView<Vx.Views.FontIcon, FontIcon>
    {
        //private static FontFamily MaterialIconFontFamily = new FontFamily("ms-appx:///Vx.Uwp/Resources/Fonts/MaterialIcons-Regular.ttf#Material Icons");
        private static FontFamily MaterialIconFontFamily = new FontFamily("ms-appx:///Vx.Uwp/Resources/Fonts/MaterialIconsOutlined-Regular.otf#Material Icons Outlined");

        public UwpFontIcon()
        {
            View.FontFamily = MaterialIconFontFamily;
        }

        protected override void ApplyProperties(Vx.Views.FontIcon oldView, Vx.Views.FontIcon newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Glyph = newView.Glyph;
            View.Foreground = newView.Color.ToUwpBrush();
            View.FontSize = newView.FontSize;
        }
    }
}
