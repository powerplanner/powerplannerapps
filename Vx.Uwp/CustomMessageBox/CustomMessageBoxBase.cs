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
    public class MessageBoxResponse : EventArgs
    {
        /// <summary>
        /// -1 represents canceled. 0-x represents various response types, determined by extending classes.
        /// </summary>
        public int Response { get; private set; }

        public MessageBoxResponse(int response) { Response = response; }
    }

    public abstract class CustomMessageBoxBase
    {
        public event EventHandler<MessageBoxResponse> Response;
        public event EventHandler Showing;

        private Popup popup;

        /// <summary>
        /// The background overlay, black, covering the entire screen below Middle
        /// </summary>
        public Grid Background { get; private set; }

        /// <summary>
        /// The middle of the screen, filled with white.
        /// </summary>
        public Grid Middle { get; private set; }

        /// <summary>
        /// Where the items are displayed. Automatically centers and adds margins.
        /// </summary>
        public StackPanel Center { get; private set; }

        /// <summary>
        /// Where the title and content is contained. Has right margin.
        /// </summary>
        public StackPanel TitleAndContent { get; private set; }

        public ScrollViewer ScrollViewer { get; private set; }

        public UIElement Title
        {
            get { return TitleAndContent.Children[0]; }
            set
            {
                TitleAndContent.Children[0] = value;
            }
        }

        public UIElement Content
        {
            get { return TitleAndContent.Children[1]; }
            set { TitleAndContent.Children[1] = value; }
        }

        /// <summary>
        /// Automatically aligns to right
        /// </summary>
        public FrameworkElement Buttons
        {
            get { return Center.Children[1] as FrameworkElement; }
            set { value.HorizontalAlignment = HorizontalAlignment.Right; Center.Children[1] = value; }
        }

        private Window _currWindow;
        public CustomMessageBoxBase()
        {
            popup = new Popup();
            

            _currWindow = Window.Current;
            if (_currWindow == null)
                throw new NullReferenceException("Window.Current was null");

            Background = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black) { Opacity = 0.7 },
                Width = _currWindow.Bounds.Width,
                Height = _currWindow.Bounds.Height
            };

            _currWindow.SizeChanged += (s, e) =>
            {
                Background.Width = e.Size.Width;
                Background.Height = e.Size.Height;

                double newSideMargin = getSideMargin();
                Center.Margin = new Thickness(newSideMargin, 24, newSideMargin, 24);
            };

            Middle = new Grid()
            {
                Background = new SolidColorBrush(Colors.White),
                MaxHeight = 700,
                VerticalAlignment = VerticalAlignment.Center
            };

            ScrollViewer = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };

            double sideMargin = getSideMargin();
            Center = new StackPanel()
            {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                Margin = new Thickness(sideMargin,24,sideMargin,24)
            };

            TitleAndContent = new StackPanel() { Margin = new Thickness(0, 0, 24, 0) };

            TitleAndContent.Children.Add(new Grid()); //empty spot for title
            TitleAndContent.Children.Add(new Grid()); //empty spot for message

            Center.Children.Add(TitleAndContent);
            Center.Children.Add(new Grid()); //empty spot for buttons

            ScrollViewer.Content = Center;

            Middle.Children.Add(ScrollViewer);

            Background.Children.Add(Middle);

            popup.Child = Background;
        }

        private double getSideMargin()
        {
            if (_currWindow.Bounds.Width < 600)
                return 20;

            return _currWindow.Bounds.Width / 5;
        }

        private bool isShowing;
        private bool pendingShow;

        /// <summary>
        /// Sends the respone event out. Then closes the message box.
        /// </summary>
        /// <param name="response"></param>
        protected void SendResponse(int response)
        {
            if (Response != null)
                Response(this, new MessageBoxResponse(response));

            Close();
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <returns>Returns a reference to the message box, in order to link commands together</returns>
        public CustomMessageBoxBase Show()
        {
            if (isShowing)
                return this;

            if (isClosing)
            {
                pendingShow = true;
                return this;
            }

            if (popup.IsOpen)
                return this;

            isShowing = true;
            pendingShow = false;
            pendingClose = false;
            isClosing = false;

            popup.IsOpen = true;

            DoubleAnimation a = new DoubleAnimation()
            {
                From = 0, To = 1, Duration = new Duration(new TimeSpan(0,0,0,0,300))
            };

            Storyboard.SetTarget(a, Background);
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

            ScrollViewer.Focus(FocusState.Programmatic);

            if (Showing != null)
                Showing(this, new EventArgs());

            return this;
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

            Storyboard.SetTarget(a, Background);
            Storyboard.SetTargetProperty(a, "Opacity");

            Storyboard s = new Storyboard();
            s.Children.Add(a);
            s.Completed += delegate
            {
                isClosing = false;
                pendingClose = false;
                popup.IsOpen = false;

                if (pendingShow)
                    Show();
            };
            s.Begin();
        }
    }
}
