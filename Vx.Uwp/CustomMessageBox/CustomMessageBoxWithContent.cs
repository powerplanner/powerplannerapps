using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    public class CustomMessageBoxWithContent : CustomMessageBoxBase
    {
        public CustomMessageBoxWithContent(string title, params string[] buttons) : this(null, title, buttons) { }

        /// <summary>
        /// Set to true by default. Automatically focuses default button such that if the user hits the space or enter key, the default button is pressed.
        /// </summary>
        protected virtual bool AutoFocusDefaultButton { get { return true; } }

        protected StackPanel SpButtons;
        public CustomMessageBoxWithContent(FrameworkElement content, string title, params string[] buttons)
        {
            if (content != null)
                Content = content;

            if (title != null)
                Title = new TextBlock()
                {
                    Text = title,
                    FontSize = 30,
                    Foreground = new SolidColorBrush(Colors.Black),
                    TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
                };

            if (buttons != null && buttons.Length > 0)
            {
                SpButtons = new StackPanel() { Orientation = Orientation.Horizontal };

                ButtonAdvanced[] array = ButtonGenerator.Generate(90, 12, buttons);

                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Click += button_Click;
                    SpButtons.Children.Add(array[i]);
                }

                //auto focus first (default) button
                if (AutoFocusDefaultButton && array.Length > 0)
                    array[0].Loaded += (s, e) =>
                        {
                            (s as ButtonAdvanced).Focus(FocusState.Keyboard);
                        };

                base.Buttons = SpButtons;
            }
        }

        void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SendResponse(SpButtons.Children.IndexOf(sender as Button));
        }
    }
}
