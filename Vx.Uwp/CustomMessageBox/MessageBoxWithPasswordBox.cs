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
    public class MessageBoxWithPasswordBox : CustomMessageBoxWithContent
    {
        public PasswordBox PasswordBox { get; private set; }

        public string Password
        {
            get { return PasswordBox.Password; }
        }

        protected override bool AutoFocusDefaultButton
        {
            get
            {
                return false;
            }
        }

        public MessageBoxWithPasswordBox(string message, string title, string password, params string[] buttons) : base(title, buttons)
        {
            StackPanel sp = new StackPanel();

            if (message != null)
                sp.Children.Add(MessageGenerator.Generate(message));

            PasswordBox = new PasswordBox()
            {
                Password = (password == null) ? "" : password,
                Margin = new Windows.UI.Xaml.Thickness(0, -18, 0, 24),
                Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204))
            };

            sp.Children.Add(PasswordBox);

            //focus on the text box
            PasswordBox.Loaded += delegate { PasswordBox.Focus(Windows.UI.Xaml.FocusState.Programmatic); };

            PasswordBox.KeyUp += TextBox_KeyUp;

            Content = sp;
        }

        void TextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                if (SpButtons.Children.Count > 0)
                    SendResponse(SpButtons.Children.Count - 1);
        }
    }
}
