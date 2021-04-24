using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public static class MyScrollViewerExtensions
    {
        /// <summary>
        /// Scrolls the scroll viewer to show the desired rectangle region.
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="desiredRegion"></param>
        public static void ShowRegion(this ScrollViewer scrollViewer, Rect desiredRegion)
        {
            Rect currentRegionAfterZoomChange = scrollViewer.GetVisibleRegion();

            if (currentRegionAfterZoomChange.Height == 0 || currentRegionAfterZoomChange.Width == 0)
                return;

            float zoomFactor = (float)Math.Min(scrollViewer.ViewportHeight / desiredRegion.Height, scrollViewer.ViewportWidth / desiredRegion.Width);
            if (zoomFactor > 1)
                zoomFactor = 1;

            currentRegionAfterZoomChange.Height = scrollViewer.ViewportHeight / zoomFactor;
            currentRegionAfterZoomChange.Width = scrollViewer.ViewportWidth / zoomFactor;

            // Find out what we need to make the bottom visible
            if (desiredRegion.Bottom > currentRegionAfterZoomChange.Bottom)
                currentRegionAfterZoomChange.Y += desiredRegion.Bottom - currentRegionAfterZoomChange.Bottom;

            // And then find out what we need to make the top visible
            // (the top always wins if both can't be visible)
            if (desiredRegion.Top < currentRegionAfterZoomChange.Top)
                currentRegionAfterZoomChange.Y += desiredRegion.Top - currentRegionAfterZoomChange.Top;

            // And do the same for X
            if (desiredRegion.Right > currentRegionAfterZoomChange.Right)
                currentRegionAfterZoomChange.X += desiredRegion.Right - currentRegionAfterZoomChange.Right;
            if (desiredRegion.Left < currentRegionAfterZoomChange.Left)
                currentRegionAfterZoomChange.X += desiredRegion.Left - currentRegionAfterZoomChange.Left;
            
            scrollViewer.ChangeView(currentRegionAfterZoomChange.X * zoomFactor, currentRegionAfterZoomChange.Y * zoomFactor, zoomFactor);
        }

        public static Rect GetVisibleRegion(this ScrollViewer scrollViewer)
        {
            float zoomFactor = scrollViewer.ZoomFactor;

            return new Rect(
                x: scrollViewer.HorizontalOffset / zoomFactor, // The offsets are relative to the zoom factor
                y: scrollViewer.VerticalOffset / zoomFactor,
                width: scrollViewer.ViewportWidth / zoomFactor, // The viewport is never affected by zoom, but how much space is actually visible is affected by zoom
                height: scrollViewer.ViewportHeight / zoomFactor);
        }

        /// <summary>
        /// Call this method once and it will continuously ensure that when the user is typing into the text box, the scroll viewer will automatically scroll
        /// to keep the text box in view. Currently only works for text box at the bottom of the scroll viewer, and very hacky since platform doesn't have
        /// much support for this scenario till Creators Update
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="tb"></param>
        public static void EnsureExpandableTextBoxRemainsVisibleWhileTyping(this ScrollViewer scrollViewer, TextBox tb)
        {
            // KeyTipVerticalOffset was added in Creators Update 15063, so can't use it
            // StartBringIntoView is also Creators Update
            // Instead we'll watch the size change events and then scroll down based on increases there
            bool atBottom = false;
            int manualSetViewCounts = 0;
            SizeChangedEventHandler sizeChanged = (s, e) =>
            {
                if (atBottom)
                {
                    manualSetViewCounts++;
                    scrollViewer.ChangeView(null, double.MaxValue, null, false);
                    return;
                }
            };

            tb.GotFocus += delegate
            {
                tb.SizeChanged += sizeChanged;
            };
            tb.LostFocus += delegate
            {
                tb.SizeChanged -= sizeChanged;
            };
            scrollViewer.ViewChanged += (s, e) =>
            {
                if (manualSetViewCounts > 0)
                {
                    manualSetViewCounts--;
                    return;
                }

                atBottom = scrollViewer.ExtentHeight - scrollViewer.VerticalOffset == scrollViewer.ViewportHeight;
            };

            // Selection changed will notify about key tip and selection changes
            //tb.SelectionChanged += delegate
            //{
            //    Point tbStartPoint = tb.TransformToVisual(scrollViewer).TransformPoint(new Point());

            //    double keyTipPoint = tbStartPoint.Y + tb.KeyTipVerticalOffset;

            //    scrollViewer.ChangeView(null, keyTipPoint, null);
            //};
        }
    }
}
