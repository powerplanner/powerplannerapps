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
    public class MessageBoxWithTextBox : CustomMessageBoxWithContent
    {
        public TextBox TextBox { get; private set; }

        protected override bool AutoFocusDefaultButton
        {
            get
            {
                return false;
            }
        }

        public MessageBoxWithTextBox(string message, string title, string text, params string[] buttons) : base(title, buttons)
        {
            StackPanel sp = new StackPanel();

            if (message != null)
                sp.Children.Add(MessageGenerator.Generate(message));

            TextBox = new TextBox()
            {
                Text = (text == null) ? "" : text,
                Margin = new Microsoft.UI.Xaml.Thickness(0, -18, 0, 24),
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 204, 204, 204))
            };

            sp.Children.Add(TextBox);

            //focus on the text box
            TextBox.Loaded += delegate { TextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic); };

            TextBox.KeyUp += TextBox_KeyUp;

            Content = sp;
        }

        void TextBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                if (SpButtons.Children.Count > 0)
                    SendResponse(SpButtons.Children.Count - 1);
        }
    }
}
