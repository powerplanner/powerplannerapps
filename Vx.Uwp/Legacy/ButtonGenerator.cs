using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    public class ButtonAdvanced : Button
    {
        public new event RoutedEventHandler Click;

        # region BackgroundHover

        public Brush BackgroundHover
        {
            get { return GetValue(BackgroundHoverProperty) as Brush; }
            set { SetValue(BackgroundHoverProperty, value); }
        }

        public static readonly DependencyProperty BackgroundHoverProperty = DependencyProperty.Register("BackgroundHover", typeof(Brush), typeof(ButtonAdvanced), null);

        private Brush storedBackground;
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            if (BackgroundHover != null)
            {
                storedBackground = Background;
                Background = BackgroundHover;
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            if (storedBackground != null)
            {
                Background = storedBackground;
                storedBackground = null;
            }
        }

        # endregion

        # region PointerPressed

        # region BackgroundPressed

        public Brush BackgroundPressed
        {
            get { return GetValue(BackgroundPressedProperty) as Brush; }
            set { SetValue(BackgroundPressedProperty, value); }
        }

        public static readonly DependencyProperty BackgroundPressedProperty = DependencyProperty.Register("BackgroundPressed", typeof(Brush), typeof(ButtonAdvanced), null);

        # endregion

        # region ForegroundPressed

        public Brush ForegroundPressed
        {
            get { return GetValue(ForegroundPressedProperty) as Brush; }
            set { SetValue(ForegroundPressedProperty, value); }
        }

        public static readonly DependencyProperty ForegroundPressedProperty = DependencyProperty.Register("ForegroundPressed", typeof(Brush), typeof(ButtonAdvanced), null);

        # endregion

        private Brush storedBackgroundPress, storedForegroundPress;
        private bool pointerPressed;
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (BackgroundPressed != null)
            {
                storedBackgroundPress = Background;
                Background = BackgroundPressed;
            }

            if (ForegroundPressed != null)
            {
                storedForegroundPress = Foreground;
                Foreground = ForegroundPressed;
            }

            pointerPressed = true;
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            if (storedBackgroundPress != null)
            {
                Background = storedBackgroundPress;
                storedBackgroundPress = null;
            }

            if (storedForegroundPress != null)
            {
                Foreground = storedForegroundPress;
                storedForegroundPress = null;
            }

            if (pointerPressed)
            {
                pointerPressed = false;

                if (Click != null)
                    Click(this, new RoutedEventArgs());
            }
        }

        # endregion

        # region KeyPressed

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Windows.System.VirtualKey.Space || e.Key == Windows.System.VirtualKey.Enter)
            {
                OnPointerPressed(null);
                OnPointerReleased(null);
            }
        }

        # endregion
    }

    public class ButtonDefault : ButtonAdvanced
    {
        public ButtonDefault()
        {
            Foreground = new SolidColorBrush(Colors.White);
            Background = new SolidColorBrush(Color.FromArgb(255, 70, 70, 70));
            BackgroundHover = new SolidColorBrush(Color.FromArgb(255, 114, 114, 114));

            BackgroundPressed = new SolidColorBrush(Colors.Black);

            BorderBrush = Background;
        }
    }

    public class ButtonSecondary : ButtonAdvanced
    {
        public ButtonSecondary()
        {
            Foreground = new SolidColorBrush(Colors.Black);
            Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
            BackgroundHover = new SolidColorBrush(Color.FromArgb(255, 216, 216, 216));

            BackgroundPressed = Foreground;
            ForegroundPressed = new SolidColorBrush(Colors.White);

            SetBinding(BorderBrushProperty, new Binding() { Path = new PropertyPath("Background"), Source = this });
        }
    }

    public class ButtonGenerator
    {
        /// <summary>
        /// Generates multiple buttons for each text item with the specified width
        /// </summary>
        /// <param name="width"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static ButtonAdvanced[] Generate(int width, int marginBetween, params string[] texts)
        {
            ButtonAdvanced[] answer = new ButtonAdvanced[texts.Length];

            for (int i = 0; i < texts.Length; i++)
            {
                if (i == 0)
                {
                    answer[i] = new ButtonDefault()
                    {
                        Content = texts[i],
                        Width = width,
                        TabIndex = texts.Length - i - 1
                    };
                }

                else
                    answer[i] = new ButtonSecondary()
                    {
                        Content = texts[i],
                        Width = width,
                        TabIndex = texts.Length - i - 1
                    };

                if (i != texts.Length - 1)
                    answer[i].Margin = new Thickness(0, 0, marginBetween, 0);
            }

            return answer;
        }
    }
}
