using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpTextBlock : UwpView<Vx.Views.TextBlock, TextBlock>
    {
        protected override void ApplyProperties(Vx.Views.TextBlock oldView, Vx.Views.TextBlock newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = newView.Text;
            View.FontWeight = newView.FontWeight.ToUwp();
            View.Foreground = newView.TextColor.ToUwpBrush();
            View.TextWrapping = newView.WrapText ? Windows.UI.Xaml.TextWrapping.Wrap : Windows.UI.Xaml.TextWrapping.NoWrap;
            View.FontSize = newView.FontSize;
        }
    }
}
