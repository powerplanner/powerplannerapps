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

namespace InterfacesUWP
{
    class DotsAnimation
    {
        /// <summary>
        /// The duration in milliseconds of the animation
        /// </summary>
        public int Duration = 0;

        /// <summary>
        /// The minimum speed of the movement
        /// </summary>
        public double MinimumSpeed = 0;

        /// <summary>
        /// Returns a value from 0 to 1, representing the percent complete through the animation
        /// </summary>
        /// <param name="currTime"></param>
        /// <returns></returns>
        public double GetPosition(int currTime)
        {
            double x = (double)currTime / Duration;

            if (x < 0)
                x = 0;
            else if (x > 1)
                x = 1;

            return formula(x); //boost the speed
        }

        private double formula(double x)
        {
            if (x >= 0.25 && x <= 0.75)
                return formulaLinear(x);
            return formulaCubed(x);
        }

        private double formulaLinear(double x)
        {
            return 0.25 * x + 0.375;
        }

        private double formulaCubed(double x)
        {
            return 4 * Math.Pow((x - 0.5), 3) + 0.5;
        }
    }

    public class MyProgressIndicator : Canvas
    {
        private interface Next
        {
            void Show();
            void Stop();
        }

        private bool isRunning;
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        private ProgressSquare first;
        private Color color;

        public MyProgressIndicator(Color color)
        {
            this.color = color;
            first = new ProgressSquare(this, color);

            ProgressSquare temp = first;
            for (int i = 0; i < 4; i++)
            {
                temp.Next = new ProgressSquare(this, color);
                temp = (ProgressSquare)temp.Next;
            }

            temp.Next = new WaitSquare(first);
        }

        public void Start()
        {
            if (!isRunning)
            {
                base.Opacity = 1;
                first.Show();
                isRunning = true;
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                base.Opacity = 0;
                first.Stop();
                isRunning = false;
            }
        }

        private class WaitSquare : Next
        {
            private ProgressSquare first;

            public WaitSquare(ProgressSquare theFirstSquare)
            {
                first = theFirstSquare;
            }

            private DispatcherTimer t;
            public void Show()
            {
                t = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 3400) };
                t.Tick += delegate { t.Stop(); first.Show(); };
                t.Start();
            }


            public void Stop()
            {
                if (t != null)
                    t.Stop();
            }
        }

        private class ProgressSquare : Grid, Next
        {
            public Next Next;
            private TranslateTransform transform;
            private static int width = 5;

            private static int totalTime = 500;
            private static int interval = 3;
            private int currTime = 0;
            private static int timeTillNext = 40;

            private Canvas container;

            public ProgressSquare(Canvas container, Color color)
            {
                this.container = container;

                base.Background = new SolidColorBrush(color);
                base.Width = width;
                base.Height = width;
                base.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                base.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;

                transform = new TranslateTransform() { X = width * -1 };
                base.RenderTransform = transform;

                container.Children.Add(this);
            }

            private DispatcherTimer timer;
            public void Show()
            {
                transform.X = width * -1;
                currTime = 0;

                DotsAnimation a = new DotsAnimation() { Duration = totalTime };
                bool hasntShownNext = true;

                timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, interval) };

                timer.Tick += delegate
                {
                    if (currTime >= totalTime)
                    {
                        timer.Stop();
                        transform.X = container.Width;
                        return;
                    }

                    transform.X = (container.Width + width) * a.GetPosition(currTime) - width;

                    if (currTime >= timeTillNext && hasntShownNext)
                    {
                        if (Next != null)
                            Next.Show();

                        hasntShownNext = false;
                    }

                    currTime += interval;
                };

                timer.Start();
            }


            public void Stop()
            {
                if (timer != null)
                    timer.Stop();

                transform.X = width * -1;

                if (Next != null)
                    Next.Stop();
            }
        }
    }

    public class TopProgressIndicator
    {
        private Popup popup;
        private MyProgressIndicator progressBar;
        private TextBlock textBlockMessage;
        private TextBlock textBlockTime;

        private Slide slideMessage;
        private Slide slideTime;

        private static int negY = -30;

        public Brush Foreground
        {
            get { return textBlockTime.Foreground; }
            set
            {
                textBlockTime.Foreground = value;
                textBlockMessage.Foreground = value;
            }
        }

        private string loadingText = "";
        public string LoadingText
        {
            get { return loadingText; }
            set
            {
                if (value == null)
                {
                    progressBar.Stop();
                    loadingText = "";
                    slideMessage.SlideUp(loadingText, false);
                    slideTime.SlideDown("", false);
                    return;
                }

                progressBar.Start();

                loadingText = value;
                slideMessage.SlideUp(loadingText, true);
                slideTime.SlideUp("", false);
                timer.Stop();
            }
        }

        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) }; //5 seconds
        private string tempText = "";
        public string TempText
        {
            get { return tempText; }
            set
            {
                tempText = value;
                slideMessage.SlideUp(tempText, true);
                slideTime.SlideUp("", false);

                timer.Stop();
                timer.Start();
            }
        }

        void timer_Tick(object sender, object e)
        {
            if (loadingText.Length == 0)
            {
                slideMessage.SlideUp("", false);
                slideTime.SlideDown("", false);
                timer.Stop();
            }

            else
            {
                slideMessage.SlideUp(loadingText, true);
                timer.Stop();
            }
        }

        public TopProgressIndicator(Color dotsColor)
        {
            popup = new Popup();

            timer.Tick += timer_Tick;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            progressBar = new MyProgressIndicator(dotsColor);
            Grid.SetRow(progressBar, 0);
            grid.Children.Add(progressBar);

            textBlockTime = new TextBlock()
            {
                Text = DateTime.Now.ToString("t"),
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 16,
                Margin = new Thickness(0, 6, 24, 0)
            };

            //update the time
            DispatcherTimer t = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
            t.Tick += delegate { textBlockTime.Text = DateTime.Now.ToString("t"); };
            t.Start();

            TranslateTransform transformTime = new TranslateTransform();
            textBlockTime.RenderTransform = transformTime;
            slideTime = new Slide(transformTime);

            Grid.SetRow(textBlockTime, 1);
            grid.Children.Add(textBlockTime);


            textBlockMessage = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 16,
                Margin = new Thickness(24, 6, 0, 0)
            };

            TranslateTransform transformMessage = new TranslateTransform();
            textBlockMessage.RenderTransform = transformMessage;
            transformMessage.Y = negY;
            slideMessage = new Slide(transformMessage);
            slideMessage.Completed += (s, e) =>
            {
                textBlockMessage.Text = e.NewText;

                if (e.FinishedSlideUp)
                    slideTime.SlideDown("", false);
            };

            Grid.SetRow(textBlockMessage, 1);
            grid.Children.Add(textBlockMessage);


            popup.Child = grid;
            popup.IsOpen = true;
        }

        private class SlideEventArgs : EventArgs
        {
            private string newText;
            private bool finishedSlideUp;
            public SlideEventArgs(string newText, bool finishedSlideUp)
            {
                this.newText = newText;
                this.finishedSlideUp = finishedSlideUp;
            }

            public string NewText { get { return newText; } }
            public bool FinishedSlideUp { get { return finishedSlideUp; } }
        }

        private class Slide
        {
            public event EventHandler<SlideEventArgs> Completed;

            private TranslateTransform transform;
            private DispatcherTimer timer;

            public Slide(TranslateTransform transform)
            {
                this.transform = transform;
                timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 5) };
                timer.Tick += timer_Tick;
            }

            private bool slideDown;
            private bool back;
            private string newText = "";

            void timer_Tick(object sender, object e)
            {
                if (slideDown)
                {
                    if (transform.Y == 0)
                    {
                        if (back)
                            slideDown = false;
                        else
                            timer.Stop();

                        if (Completed != null)
                            Completed(this, new SlideEventArgs(newText, false));

                        back = false;

                        return;
                    }

                    if (transform.Y + 5 > 0)
                        transform.Y = 0;
                    else
                        transform.Y += 5;
                }

                else
                {
                    if (transform.Y == negY)
                    {
                        if (back)
                            slideDown = true;
                        else
                            timer.Stop();

                        if (Completed != null)
                            Completed(this, new SlideEventArgs(newText, !back));

                        back = false;

                        return;
                    }

                    if (transform.Y - 5 < negY)
                        transform.Y = negY;
                    else
                        transform.Y -= 5;
                }
            }

            public void SlideDown(string text, bool backUp)
            {
                newText = text;
                back = backUp;
                slideDown = true;
                timer.Start();
            }

            public void SlideUp(string text, bool backUp)
            {
                newText = text;
                back = backUp;
                slideDown = false;
                timer.Start();
            }


        }

        private Page currPage;
        public Page CurrentPage
        {
            set
            {
                if (currPage != null)
                    currPage.SizeChanged -= currPage_SizeChanged;

                currPage = value;

                if (currPage != null)
                {
                    currPage.SizeChanged += currPage_SizeChanged;

                    progressBar.Width = value.ActualWidth;
                }
            }
        }

        void currPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            progressBar.Width = currPage.ActualWidth;
        }
    }
}
