using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP
{
    public class CustomScrollViewer : ContentControl
    {
        private TranslateTransform _transform = new TranslateTransform();
        private ScrollBar _verticalScrollBar;
        private ContentPresenter _contentPresenter;

        public CustomScrollViewer()
        {
            DefaultStyleKey = typeof(CustomScrollViewer);
            base.ManipulationMode = ManipulationModes.TranslateInertia | ManipulationModes.TranslateRailsY | ManipulationModes.TranslateY;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _contentPresenter = this.GetTemplateChild("contentPresenter") as ContentPresenter;
            _contentPresenter.RenderTransform = _transform;

            _verticalScrollBar = this.GetTemplateChild("verticalScrollBar") as ScrollBar;
            _verticalScrollBar.ValueChanged += _verticalScrollBar_ValueChanged;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, finalSize.Width, finalSize.Height) };
            _verticalScrollBar.ViewportSize = ViewportHeight / ExtentHeight;
            _contentPresenter.Width = finalSize.Width;
            return base.ArrangeOverride(finalSize);
        }

        void _verticalScrollBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //if (e.OriginalSource != _verticalScrollBar)
            //    return;

            double newY = e.NewValue * -1 * ScrollableHeight;

            _transform.Y = newY;
        }

        //protected override void OnContentChanged(object oldContent, object newContent)
        //{
        //    base.OnContentChanged(oldContent, newContent);

        //    applyTranslateTransform();
        //}

        //protected override void OnContentTemplateChanged(Windows.UI.Xaml.DataTemplate oldContentTemplate, Windows.UI.Xaml.DataTemplate newContentTemplate)
        //{
        //    base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);

        //    applyTranslateTransform();
        //}

        //private void applyTranslateTransform()
        //{
        //    if (base.ContentTemplateRoot != null)
        //        base.ContentTemplateRoot.RenderTransform = _transform;

        //    if (base.Content is UIElement)
        //        (base.Content as UIElement).RenderTransform = _transform;
        //}

        /// <summary>
        /// Gets the vertical size of all the scrollable content in the CustomScrollViewer.
        /// </summary>
        public double ExtentHeight
        {
            get
            {
                if (_contentPresenter == null)
                    return 0;

                return _contentPresenter.ActualHeight;
            }
        }

        /// <summary>
        /// Gets the vertical size of the viewable content. This is simply ActualHeight minus any horizontal scroll bar height.
        /// </summary>
        public double ViewportHeight
        {
            get
            {
                return ActualHeight;
            }
        }

        /// <summary>
        /// Gets a value that represents the vertical size of the area that can be scrolled; the difference between the height of the extent and the height of the viewport.
        /// </summary>
        public double ScrollableHeight
        {
            get
            {
                return ExtentHeight - ViewportHeight;
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);

            _verticalScrollBar.IndicatorMode = ScrollingIndicatorMode.MouseIndicator;
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            _verticalScrollBar.IndicatorMode = ScrollingIndicatorMode.None;
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                _verticalScrollBar.IndicatorMode = ScrollingIndicatorMode.MouseIndicator;
                return;
            }

            _verticalScrollBar.IndicatorMode = ScrollingIndicatorMode.TouchIndicator;

            double newY = _transform.Y + e.Delta.Translation.Y;

            //too far to the bottom
            //if (newY * -1 > ScrollableHeight)
            //{
            //    newY = ScrollableHeight * -1;
            //}

            ////too far to the top
            //if (newY > 0)
            //{
            //    newY = 0;
            //}

            //_transform.Y = newY;

            //_verticalScrollBar.IndicatorMode = ScrollingIndicatorMode.TouchIndicator;

            if (ScrollableHeight <= 0)
                _verticalScrollBar.Value = 0;
            else
                _verticalScrollBar.Value = (newY * -1) / ScrollableHeight; //this automatically handles out of range values

            e.Handled = true;
        }

        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
        {
            base.OnManipulationCompleted(e);

            _verticalScrollBar.IndicatorMode = ScrollingIndicatorMode.None;
        }



        private void blah()
        {
            new ScrollViewer();
        }
    }
}
