using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace InterfacesUWP
{
    public interface SlideViewContentGenerator
    {
        FrameworkElement GetCurrent();

        void MoveNext();

        void MovePrevious();
    }

    public class SlideView : MyContentControl
    {
        # region Instance Variables

        private MyEqualColumnsPanel _stackpanel;
        private ScrollViewer _scrollViewer;

        # endregion

        #region Slides
        
        private void setWithoutSnap(double x)
        {
            if (_scrollViewer.HorizontalOffset == x)
                return;

            _scrollViewer.HorizontalSnapPointsType = SnapPointsType.None;
            setX(x);
            _scrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
        }

        private void setX(double x)
        {
            _scrollViewer.ChangeView(x, null, null, true);
        }

        private void setToCenter()
        {
            setX(TotalColumnWidth());
        }
        
        private void slideX(double to)
        {
            _scrollViewer.ChangeView(to, null, null, false);
        }

        private void slideToLeft()
        {
            slideX(0);
        }

        private void slideToCenter()
        {
            slideX(TotalColumnWidth());
        }

        private void slideToRight()
        {
            slideX(double.MaxValue);
        }

        #endregion

        private double _minimumColumnWidth = double.PositiveInfinity;
        /// <summary>
        /// If this value is set, the number of columns will be ignored. Unset value is double.PostiviteInfinity.
        /// </summary>
        public double MinimumColumnWidth
        {
            get { return _minimumColumnWidth; }
            set
            {
                if (_minimumColumnWidth == value)
                    return;

                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Minimum column width must be positive or positive infinity.");

                _minimumColumnWidth = value;
                calculateNumberOfColumns();
            }
        }

        private void calculateNumberOfColumns()
        {
            if (_arrangedWidth == 0 || MinimumColumnWidth == double.PositiveInfinity)
                return;

            Columns = Math.Max((int)(_arrangedWidth / MinimumColumnWidth), 1);
        }

        #region Columns

        private int _columns = 1;
        public int Columns
        {
            get { return _columns; }
            set
            {
                if (value == _columns)
                    return;

                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Columns must be 1 or greater.");

                _columns = value;
                render();
            }
        }

        #endregion

        private int countOfVisuals()
        {
            return Columns + 2;
        }

        #region ColumnWidth

        public double ColumnWidth
        {
            get
            {
                return (_arrangedWidth - (Columns + 1) * ColumnSpacing) / Columns;
            }
        }

        #endregion

        #region ColumnSpacing

        public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register("ColumnSpacing", typeof(double), typeof(SlideView), new PropertyMetadata(0.0, OnColumnSpacingChanged));

        /// <summary>
        /// Spacing that is applied to left and right of each column (adjacent columns merge their spacing so it isn't doubled)
        /// </summary>
        public double ColumnSpacing
        {
            get { return (double)GetValue(ColumnSpacingProperty); }
            set { SetValue(ColumnSpacingProperty, value); }
        }

        private static void OnColumnSpacingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SlideView).OnColumnSpacingChanged(e);
        }

        private void OnColumnSpacingChanged(DependencyPropertyChangedEventArgs e)
        {
            _stackpanel.ColumnSpacing = ColumnSpacing;
        }

        #endregion

        //# region Content

        ///// <summary>
        ///// Do not use this.
        ///// </summary>
        //public new UIElement Content
        //{
        //    get { return null; }
        //}

        //# endregion

        #region SlideView

        public SlideView()
        {
            _stackpanel = new MyEqualColumnsPanel()
            {
                //Orientation = Orientation.Horizontal
            };

            _scrollViewer = new ScrollViewer()
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollMode = ScrollMode.Disabled,
                HorizontalSnapPointsType = SnapPointsType.MandatorySingle,
                ZoomMode = ZoomMode.Disabled,
                Opacity = 0
            };
            _scrollViewer.Content = _stackpanel;
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
            _scrollViewer.Loaded += _scrollViewer_Loaded;
            base.Content = _scrollViewer;
            

            //watch the gestures
            //base.ManipulationMode = ManipulationModes.TranslateRailsX | ManipulationModes.TranslateX | ManipulationModes.TranslateInertia;
            //base.ManipulationDelta += MyFlipView_ManipulationDelta;
            //base.ManipulationCompleted += MyFlipView_ManipulationCompleted;
            //base.ManipulationInertiaStarting += MyFlipView_ManipulationInertiaStarting;

            //base.PointerWheelChanged += SlideView_PointerWheelChanged;

            base.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            base.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            
        }

        void _scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            setToCenter();

            var dontWait = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
            {
                try
                {
                    _scrollViewer.Opacity = 1;
                }
                catch { }
            });
        }

        /// <summary>
        /// Returns column width combined with half of each side's spacing (thus the effective column width including padding)
        /// </summary>
        /// <returns></returns>
        private double TotalColumnWidth()
        {
            return ColumnWidth + ColumnSpacing;
        }

        void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //as we scroll, we'll dynamically add upcoming/previous items

            //moving next
            // Scrollable - Offset = 0 when scrolled all the way to the right
            if ((_scrollViewer.ScrollableWidth - _scrollViewer.HorizontalOffset) < TotalColumnWidth() * 0.5)
            {
                showNextVisual();
            }

            //moving previous
            else if (_scrollViewer.HorizontalOffset < TotalColumnWidth() * 0.5)
            {
                if (showPreviousVisual())
                {
                    setWithoutSnap(_scrollViewer.HorizontalOffset + TotalColumnWidth());
                }
            }

            // We only remove once we've stopped moving
            if (!e.IsIntermediate)
            {
                // Only update current item when we stopped moving
                int actualCurrentIndex = (int)((_scrollViewer.HorizontalOffset + TotalColumnWidth() * 0.5) / TotalColumnWidth());

                if (actualCurrentIndex != CurrentItemIndex)
                    UpdateCurrentItemIndex(actualCurrentIndex);

                int countRemovedFromLeft = 0;

                // Remove on the left
                while (CurrentItemIndex > 1)
                {
                    if (!removePrevVisual())
                        break;

                    countRemovedFromLeft++;
                }

                // If we removed from the left, we need to adjust scroll offset
                if (countRemovedFromLeft > 0)
                    setWithoutSnap(_scrollViewer.HorizontalOffset - TotalColumnWidth() * countRemovedFromLeft);

                // Remove on the right
                int desiredCountOfVisuals = countOfVisuals();
                while (_stackpanel.Children.Count > desiredCountOfVisuals)
                {
                    if (!removeNextVisual())
                        break;
                }
            }
        }

        private double _arrangedWidth;

        protected override Size ArrangeOverride(Size finalSize)
        {
            _arrangedWidth = finalSize.Width;

            base.ArrangeOverride(finalSize);

            calculateNumberOfColumns();

            setColumnWidth();

            setWithoutSnap(TotalColumnWidth());

            return finalSize;
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

        private FrameworkElement generateAndPrepareCurrentVisual()
        {
            FrameworkElement visual = ContentGenerator.GetCurrent();
            
            OnCreating(visual);

            return visual;
        }

        # region render

        private void render()
        {
            if (ContentGenerator == null)
                return;

            var generator = ContentGenerator;

            //deal with old items
            foreach (FrameworkElement el in _stackpanel.Children.OfType<FrameworkElement>())
                OnRemoving(el);

            //grab a reference of the old current item
            FrameworkElement oldCurrent = _stackpanel.Children.ElementAtOrDefault(1) as FrameworkElement;

            //clear everything
            _stackpanel.Children.Clear();


            //rewind to the beginning element needed
            generator.MovePrevious();


            //now generate all the UIElements, while adding them
            for (int i = 0; ; )
            {
                FrameworkElement visual = generateAndPrepareCurrentVisual();

                //when we're changing the current item, we call the method to say we've changed it
                if (i == 1)
                {
                    CurrentItemIndex = 1;
                    OnChangedCurrent(oldCurrent, visual);
                }

                _stackpanel.Children.Add(visual);


                i++;

                //doing the check here makes sure we don't ever move past the last desired item
                if (i == countOfVisuals())
                    break;

                generator.MoveNext();
            }


            //and then rewind the generator to the current item (it's sitting on the last item right now)
            for (int i = 2; i < countOfVisuals(); i++)
                generator.MovePrevious();

            //and make sure the translate is at the original position
            //setToCenter();
            setWithoutSnap(TotalColumnWidth());
        }


        private void setColumnWidth()
        {
            _stackpanel.ColumnWidth = ColumnWidth;
        }


        # endregion render



        # region Next

        private bool showNextVisual()
        {
            if (ContentGenerator == null)
                return false;

            int timesToMoveForward = _stackpanel.Children.Count - CurrentItemIndex;

            for (int i = 0; i < timesToMoveForward; i++)
                ContentGenerator.MoveNext();

            FrameworkElement newEl = generateAndPrepareCurrentVisual();
            
            for (int i = 0; i < timesToMoveForward; i++)
                ContentGenerator.MovePrevious();

            //add the item
            _stackpanel.Children.Add(newEl);

            //call method so extending class can do something
            OnSlidingNext(null, newEl);

            return true;
        }

        private bool removePrevVisual()
        {
            if (ContentGenerator == null)
                return false;

            //remove previous item
            FrameworkElement removedEl = _stackpanel.Children.FirstOrDefault() as FrameworkElement;
            if (removedEl != null)
            {
                _stackpanel.Children.Remove(removedEl);
                CurrentItemIndex--;
                OnRemoving(removedEl);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Does not adjust the HorizontalOffset
        /// </summary>
        /// <returns></returns>
        private bool changeVisualsToNext()
        {
            if (showNextVisual())
            {
                UpdateCurrentItemIndex(CurrentItemIndex + 1);
                removePrevVisual();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Slides to display the next item
        /// </summary>
        public void Next()
        {
            if (!changeVisualsToNext())
                return;

            //and make it slide from the new correct starting point, to the final ending point
            setToCenter();
        }

        protected virtual void OnSlidingNext(FrameworkElement removedEl, FrameworkElement newEl)
        {
            //nothing
        }

        # endregion

        # region Previous

        private bool showPreviousVisual()
        {
            if (ContentGenerator == null)
                return false;

            int timesToMove = CurrentItemIndex + 1;

            for (int i = 0; i < timesToMove; i++)
                ContentGenerator.MovePrevious();

            FrameworkElement newEl = generateAndPrepareCurrentVisual();
            
            for (int i = 0; i < timesToMove; i++)
                ContentGenerator.MoveNext();

            


            //insert the new previous
            _stackpanel.Children.Insert(0, newEl);

            CurrentItemIndex++;

            //call the method allowing extending class to do something
            OnSlidingPrevious(null, newEl);

            return true;
        }

        private bool removeNextVisual()
        {
            if (ContentGenerator == null)
                return false;

            //remove last item on the right
            FrameworkElement onRight = _stackpanel.Children.LastOrDefault() as FrameworkElement;
            if (onRight != null)
            {
                _stackpanel.Children.Remove(onRight);
                OnRemoving(onRight);
                return true;
            }

            return false;
        }

        private bool changeVisualsToPrevious()
        {
            if (showPreviousVisual())
            {
                UpdateCurrentItemIndex(CurrentItemIndex - 1);
                removeNextVisual();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Slides to display the previous item
        /// </summary>
        public void Previous()
        {
            if (!changeVisualsToPrevious())
                return;

            slideToCenter();
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
            get { return _stackpanel.Children.ElementAtOrDefault(1); }
        }

        # endregion

        public IEnumerable<UIElement> Children
        {
            get { return _stackpanel.Children; }
        }

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

        private int CurrentItemIndex { get; set; } = int.MinValue;
        private void UpdateCurrentItemIndex(int newIndex)
        {
            FrameworkElement prevCurrEl;

            if (CurrentItemIndex == int.MinValue)
                prevCurrEl = null;
            else
            {
                prevCurrEl = _stackpanel.Children.ElementAtOrDefault(CurrentItemIndex) as FrameworkElement;

                if (ContentGenerator != null)
                {
                    if (CurrentItemIndex < newIndex)
                        for (int i = CurrentItemIndex; i < newIndex; i++)
                            ContentGenerator.MoveNext();

                    else if (CurrentItemIndex > newIndex)
                        for (int i = CurrentItemIndex; i > newIndex; i--)
                            ContentGenerator.MovePrevious();

                }
            }

            CurrentItemIndex = newIndex;

            OnChangedCurrent(prevCurrEl, _stackpanel.Children.ElementAtOrDefault(newIndex) as FrameworkElement);
        }

        protected virtual void OnChangedCurrent(FrameworkElement oldEl, FrameworkElement newEl)
        {
            //nothing
        }

        # endregion
    }
}
