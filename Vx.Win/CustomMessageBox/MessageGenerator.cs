using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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
                Margin = new Microsoft.UI.Xaml.Thickness(0, 20, 0, 24),
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };
        }
    }
}
