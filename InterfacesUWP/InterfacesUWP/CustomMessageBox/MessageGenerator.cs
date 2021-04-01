using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    public class MessageGenerator
    {
        public static TextBlock Generate(string text)
        {
            return new TextBlock()
            {
                Text = text,
                FontSize = 16,
                Margin = new Windows.UI.Xaml.Thickness(0, 20, 0, 24),
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
            };
        }
    }
}
