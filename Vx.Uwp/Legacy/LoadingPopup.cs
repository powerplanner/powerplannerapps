using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace InterfacesUWP
{
    /// <summary>
    /// Displays a progress indicator over the entire page
    /// </summary>
    public class LoadingPopup
    {
        public event EventHandler Canceled;
        public bool IsCanceled { get; private set; }

        private Popup popup;

        private Grid background;
        public Grid Background { get { return background; } }

        private Grid center;
        public Grid Center { get { return center; } }

        private string originalMessage = "";
        private TextBlock textBlockMessage;
        public string Text
        {
            get { return originalMessage; }
            set { originalMessage = value; textBlockMessage.Text = originalMessage; }
        }

        private Button cancel;
        public bool IsCancelable
        {
            get { return cancel.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                    cancel.Visibility = Visibility.Visible;
                else
                    cancel.Visibility = Visibility.Collapsed;
            }
        }

        private ProgressRing progressRing;
        public ProgressRing ProgressRing { get { return progressRing; } }

        private DispatcherTimer timer;

        public LoadingPopup()
        {
            popup = new Popup();

            Window currWindow = Window.Current;
            if (currWindow == null)
                throw new NullReferenceException("Window.Current was null");

            background = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black) { Opacity = 0.7 },
                Width = currWindow.Bounds.Width,
                Height = currWindow.Bounds.Height
            };

            StackPanel vertical = new StackPanel() { Margin = new Thickness(100, 48, 0, 48) };

            currWindow.SizeChanged += (s, e) =>
            {
                background.Width = e.Size.Width;
                background.Height = e.Size.Height;
                center.Width = e.Size.Width;

                if (e.Size.Width < 600)
                {
                    vertical.Margin = new Thickness(20, 48, 0, 48);
                    textBlockMessage.FontSize = 24;
                }

                else
                {
                    vertical.Margin = new Thickness(100, 48, 0, 48);
                    textBlockMessage.FontSize = 30;
                }
            };
            
            center = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                Width = currWindow.Bounds.Width
            };
            center.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            StackPanel sp = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0,0,0,12)
            };

            progressRing = new Windows.UI.Xaml.Controls.ProgressRing()
            {
                IsActive = true,
                Foreground = new SolidColorBrush(Colors.White)
            };

            sp.Children.Add(progressRing);

            textBlockMessage = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 30,
                Margin = new Thickness(24,0,0,0)
            };

            sp.Children.Add(textBlockMessage);

            vertical.Children.Add(sp);

            cancel = new Button()
            {
                Content = "Cancel",
                HorizontalAlignment = HorizontalAlignment.Right,
                RequestedTheme = ElementTheme.Dark,
                Visibility = Visibility.Collapsed
            };
            cancel.Click += cancel_Click;
            vertical.Children.Add(cancel);

            center.Children.Add(vertical);

            background.Children.Add(center);

            popup.Child = background;


            timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            timer.Tick += timer_Tick;
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;

            Close();

            if (Canceled != null)
                Canceled(this, new EventArgs());
        }

        private int tick = 1;
        void timer_Tick(object sender, object e)
        {
            string message = originalMessage;

            for (int i = 0; i < tick; i++)
                message += '.';

            textBlockMessage.Text = message;

            tick = (tick + 1);
            if (tick == 4)
                tick = 1;
        }

        private bool isShowing;
        private bool pendingShow;
        public void Show()
        {
            if (isShowing)
                return;

            if (isClosing)
            {
                pendingShow = true;
                return;
            }

            if (popup.IsOpen)
                return;

            isShowing = true;
            pendingShow = false;
            pendingClose = false;
            isClosing = false;

            popup.IsOpen = true;
            timer.Start();

            DoubleAnimation a = new DoubleAnimation()
            {
                From = 0, To = 1, Duration = new Duration(new TimeSpan(0,0,0,0,300))
            };

            Storyboard.SetTarget(a, background);
            Storyboard.SetTargetProperty(a, "Opacity");

            Storyboard s = new Storyboard();
            s.Children.Add(a);
            s.Completed += delegate
            {
                isShowing = false;
                pendingShow = false;

                if (pendingClose)
                    Close();
            };
            s.Begin();
        }

        private bool isClosing;
        private bool pendingClose;
        public void Close()
        {
            if (isClosing)
                return;

            if (isShowing)
            {
                pendingClose = true;
                return;
            }

            if (!popup.IsOpen)
            {
                pendingClose = false;
                return;
            }

            isClosing = true;
            isShowing = false;
            pendingShow = false;
            pendingClose = false;

            DoubleAnimation a = new DoubleAnimation()
            {
                From = 1, To = 0, Duration = new Duration(new TimeSpan(0,0,0,0,300))
            };

            Storyboard.SetTarget(a, background);
            Storyboard.SetTargetProperty(a, "Opacity");

            Storyboard s = new Storyboard();
            s.Children.Add(a);
            s.Completed += delegate
            {
                isClosing = false;
                pendingClose = false;
                popup.IsOpen = false;

                timer.Stop();

                if (pendingShow)
                    Show();
            };
            s.Begin();
        }
    }
}
