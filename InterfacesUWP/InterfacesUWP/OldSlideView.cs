using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    public interface SlideViewContentGenerator
    {
        FrameworkElement GetCurrent();

        void MoveNext();

        void MovePrevious();
    }

    public class SlideView : MyGrid
    {
        # region Instance Variables

        private TranslateTransform _prevTransform, _currTransform, _nextTransform;

        private FrameworkElement _prev, _current, _next;

        private Slide _slide;

        # endregion

        # region Slide

        private class Slide
        {
            public enum GridToShow { Left, Middle, Right };

            public event EventHandler Completed;

            private TranslateTransform _left, _middle, _right;
            private double _width;

            private DispatcherTimer timer;

            public Slide(TranslateTransform leftTransform, TranslateTransform middleTransform, TranslateTransform rightTransform, double width)
            {
                _left = leftTransform;
                _middle = middleTransform;
                _right = rightTransform;

                _width = width;

                timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 4) };

                timer.Tick += delegate
                {
                    if (Math.Abs(middleTransform.X) <= 1)
                    {
                        timer.Stop();

                        middleTransform.X = 0;
                        leftTransform.X = width * -1;
                        rightTransform.X = width;

                        completed();
                    }

                    else
                    {
                        middleTransform.X /= 1.5;
                        leftTransform.X = middleTransform.X - width;
                        rightTransform.X = middleTransform.X + width;
                    }
                };

                timer.Start();
            }

            public void Finish()
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();

                    _middle.X = 0;
                    _left.X = _width * -1;
                    _right.X = _width;
                }
            }

            public void Cancel()
            {
                timer.Stop();
            }

            private void completed()
            {
                if (Completed != null)
                    Completed(this, new EventArgs());
            }
        }

        # endregion

        //# region Content

        ///// <summary>
        ///// Do not use this.
        ///// </summary>
        //public new UIElement Content
        //{
        //    get { return null; }
        //}

        //# endregion

        # region SlideView

        public SlideView()
        {
            //initialize the translate transforms
            _prevTransform = new TranslateTransform();
            _currTransform = new TranslateTransform();
            _nextTransform = new TranslateTransform();


            //watch the gestures
            base.ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.All;
            base.ManipulationDelta += MyFlipView_ManipulationDelta;
            base.ManipulationCompleted += MyFlipView_ManipulationCompleted;
            base.ManipulationInertiaStarting += MyFlipView_ManipulationInertiaStarting;

            base.PointerWheelChanged += SlideView_PointerWheelChanged;

            base.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            base.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;

            base.SizeChanged += MyFlipView_SizeChanged;

        }

        void SlideView_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            int delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta;

            //horizontal
            if (e.GetCurrentPoint(null).Properties.IsHorizontalMouseWheel)
            {
                //positive is tilted to the right
                if (delta >= 120) //120 is one move
                    Next();
                else if (delta <= -120)
                    Previous();
            }

            //normal vertical scrolling
            else
            {
                //positive is pushing forward
                if (delta >= 120)
                    Previous();
                else if (delta <= -120)
                    Next();
            }
        }

        private bool usingInertia;
        void MyFlipView_ManipulationInertiaStarting(object sender, Windows.UI.Xaml.Input.ManipulationInertiaStartingRoutedEventArgs e)
        {
            if (e.Velocities.Linear.X < -0.3)
            {
                usingInertia = true;
                Next();
            }

            else if (e.Velocities.Linear.X > 0.3)
            {
                usingInertia = true;
                Previous();
            }
        }

        void MyFlipView_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            if (usingInertia)
            {
                usingInertia = false;
                return;
            }

            if (_currTransform.X < _current.ActualWidth / -2.0)
            {
                Next();
            }

            else if (_currTransform.X > _current.ActualWidth / 2.0)
            {
                Previous();
            }

            else
            {
                //slide back to the center
                _slide = new Slide(_prevTransform, _currTransform, _nextTransform, _current.ActualWidth);
            }
        }

        void MyFlipView_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (usingInertia)
                return;

            if (_slide != null)
                _slide.Cancel();

            adjustTransforms(e.Delta.Translation.X);
        }

        private void adjustTransforms(double x)
        {
            //if we're dragging too far to the right (moving to previous)
            if (_currTransform.X + x > _current.ActualWidth)
                x = _current.ActualWidth - _currTransform.X;

            //else if we're dragging too far to the left (moving to next)
            if (_currTransform.X + x < _current.ActualWidth * -1)
                x = _current.ActualWidth * -1 - _currTransform.X;

            _currTransform.X += x;
            _prevTransform.X += x;
            _nextTransform.X += x;
        }

        void MyFlipView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_prevTransform.X -= e.NewSize.Width - e.PreviousSize.Width;

            //_nextTransform.X += e.NewSize.Width - e.PreviousSize.Width;

            Clip = new RectangleGeometry() { Rect = new Rect(new Point(0, 0), e.NewSize) };
        }

        # endregion

        # region ContentGenerator

        public SlideViewContentGenerator ContentGenerator
        {
            get { return GetValue(ContentGeneratorProperty) as SlideViewContentGenerator; }
            set { SetValue(ContentGeneratorProperty, value); }
        }

        public static readonly DependencyProperty ContentGeneratorProperty = DependencyProperty.Register("ContentGenerator", typeof(SlideViewContentGenerator), typeof(SlideView), new PropertyMetadata(null, contentGeneratorChanged));

        private static void contentGeneratorChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as SlideView).contentGeneratorChanged(args);
        }

        private void contentGeneratorChanged(DependencyPropertyChangedEventArgs args)
        {
            render();
        }

        # endregion

        # region render

        private void render()
        {
            if (ContentGenerator == null)
                return;

            base.Children.Clear();

            var generator = ContentGenerator;

            //stop watching old current
            stopWatching(_current);
            stopWatching(_prev);

            if (_prev != null)
                OnRemoving(_prev);

            if (_current != null)
                OnRemoving(_current);

            if (_next != null)
                OnRemoving(_next);

            //set the UIElements
            generator.MovePrevious();
            _prev = generator.GetCurrent();

            if (_prev != null)
                OnCreating(_prev);

            generator.MoveNext();
            FrameworkElement oldCurr = _current;
            _current = generator.GetCurrent();

            if (_current != null)
                OnCreating(_current);

            OnChangedCurrent(oldCurr, _current);

            generator.MoveNext();
            _next = generator.GetCurrent();

            if (_next != null)
                OnCreating(_next);

            generator.MovePrevious();

            //watch new current
            watchCurrent();
            watchPrevious();

            
            //set the transforms
            _prev.RenderTransform = _prevTransform;
            _current.RenderTransform = _currTransform;
            _next.RenderTransform = _nextTransform;

            //add the items
            base.Children.Add(_prev);
            base.Children.Add(_current);
            base.Children.Add(_next);
        }

        # endregion render

        private void stopWatching(FrameworkElement el)
        {
            if (el != null)
                el.SizeChanged -= _current_SizeChanged;
        }

        private void watchPrevious()
        {
            if (_prev != null)
                _prev.SizeChanged += _prev_SizeChanged;
        }

        void _prev_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _prevTransform.X -= e.NewSize.Width - e.PreviousSize.Width;
        }

        private void watchCurrent()
        {
            if (_current != null)
                _current.SizeChanged += _current_SizeChanged;
        }

        void _current_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double change = e.NewSize.Width - e.PreviousSize.Width;

            _nextTransform.X += change;

            //base.Width = e.NewSize.Width;
            //base.Height = e.NewSize.Height;
        }

        # region Next

        /// <summary>
        /// Slides to display the next item
        /// </summary>
        public void Next()
        {
            if (ContentGenerator == null)
                return;

            ContentGenerator.MoveNext();
            ContentGenerator.MoveNext();
            FrameworkElement newEl = ContentGenerator.GetCurrent();

            if (newEl != null)
                OnCreating(newEl);

            ContentGenerator.MovePrevious();

            if (newEl == null)
                return;

            //complete the last transition if it was still going on
            if (_slide != null)
                _slide.Finish();

            //remove previous item
            if (_prev != null)
                base.Children.Remove(_prev);

            //stop watching previous current's size
            stopWatching(_current);
            stopWatching(_prev);

            //move the UIElements over
            FrameworkElement removedEl = _prev;

            if (removedEl != null)
                OnRemoving(removedEl);

            _prev = _current;
            _current = _next;

            OnChangedCurrent(_prev, _current);

            //move the transforms over
            _prevTransform = _currTransform;
            _currTransform = _nextTransform;

            //start watching new current's size
            watchCurrent();
            watchPrevious();

            //get the new next
            _next = newEl;
            _nextTransform = new TranslateTransform() { X = _currTransform.X + _current.ActualWidth };
            _next.RenderTransform = _nextTransform;

            //add the item to the grid
            base.Children.Add(_next);

            //assign the translates correctly
            //if (_prev != null)
            //    _prev.RenderTransform = _prevTransform;

            //if (_current != null)
            //    _current.RenderTransform = _currTransform;

            //if (_next != null)
            //    _next.RenderTransform = _nextTransform;

            //do the slide animation
            _slide = new Slide(_prevTransform, _currTransform, _nextTransform, _prev.ActualWidth);
            
            //call method so extending class can do something
            OnSlidingNext(removedEl, _next);
        }

        protected virtual void OnSlidingNext(FrameworkElement removedEl, FrameworkElement newEl)
        {
            //nothing
        }

        # endregion

        # region Previous

        /// <summary>
        /// Slides to display the previous item
        /// </summary>
        public void Previous()
        {
            if (ContentGenerator == null)
                return;

            ContentGenerator.MovePrevious();
            ContentGenerator.MovePrevious();
            FrameworkElement newEl = ContentGenerator.GetCurrent();

            if (newEl != null)
                OnCreating(newEl);

            ContentGenerator.MoveNext();

            if (newEl == null)
                return;

            //complete the last transition if it was still going on
            if (_slide != null)
                _slide.Finish();

            //remove previous item
            if (_next != null)
                base.Children.Remove(_next);

            //stop watching previous current's size
            stopWatching(_current);
            stopWatching(_prev);

            //move the UIElements over
            FrameworkElement removedEl = _next;

            if (removedEl != null)
                OnRemoving(removedEl);

            _next = _current;
            _current = _prev;

            OnChangedCurrent(_next, _current);

            //move the transforms
            _nextTransform = _currTransform;
            _currTransform = _prevTransform;

            //get the new previous
            _prev = newEl;
            _prevTransform = new TranslateTransform() { X = _currTransform.X };

            //start watching new current's size
            watchCurrent();
            watchPrevious();

            //add the item to the grid
            base.Children.Add(_prev);

            //assign the translates correctly
            if (_prev != null)
                _prev.RenderTransform = _prevTransform;

            if (_current != null)
                _current.RenderTransform = _currTransform;

            if (_next != null)
                _next.RenderTransform = _nextTransform;

            //do the slide animation
            _slide = new Slide(_prevTransform, _currTransform, _nextTransform, _next.ActualWidth);

            //call the method allowing extending class to do something
            OnSlidingPrevious(removedEl, _prev);
        }

        protected virtual void OnSlidingPrevious(FrameworkElement removedEl, FrameworkElement newEl)
        {
            //nothing
        }

        # endregion

        # region Current

        /// <summary>
        /// Gets the currently displayed UIElement
        /// </summary>
        public UIElement Current
        {
            get { return _current; }
        }

        # endregion

        # region OnRemoving

        protected virtual void OnRemoving(FrameworkElement el)
        {
            //nothing
        }

        # endregion

        # region OnCreating

        protected virtual void OnCreating(FrameworkElement el)
        {
            //nothing
        }

        # endregion

        # region OnChangedCurrent

        protected virtual void OnChangedCurrent(FrameworkElement oldEl, FrameworkElement newEl)
        {
            //nothing
        }

        # endregion
    }
}
