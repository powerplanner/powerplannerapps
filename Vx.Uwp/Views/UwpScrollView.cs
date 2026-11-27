using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpScrollView : UwpView<Vx.Views.ScrollView, ScrollViewer>
    {
        public UwpScrollView()
        {
            View.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        protected override void ApplyProperties(ScrollView oldView, ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.HorizontalScrollBarVisibility = newView.HorizontalScrollEnabled ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            View.ZoomMode = (newView.MaximumZoomScale != newView.MinimumZoomScale) ? ZoomMode.Enabled : ZoomMode.Disabled;
            View.MaxZoomFactor = newView.MaximumZoomScale;
            View.MinZoomFactor = newView.MinimumZoomScale;

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                if (View.Content is FrameworkElement oldContent)
                {
                    oldContent.SizeChanged -= ContentFwEl_SizeChanged;
                }

                var contentFwEl = view?.CreateFrameworkElement();
                View.Content = contentFwEl;

                if (VxView.ScrollToBottomWhenContentHeightChanges && contentFwEl != null)
                {
                    contentFwEl.SizeChanged += ContentFwEl_SizeChanged;
                }
            });

            if (newView.ScrollToBottomWhenContentHeightChanges)
            {
                View.ScrollToVerticalOffset(double.MaxValue);
            }
        }

        private double _prevHeight;
        private void ContentFwEl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                var prevHeight = _prevHeight;
                _prevHeight = e.NewSize.Height;

                if (e.NewSize.Height > prevHeight)
                {
                    View.ScrollToVerticalOffset(double.MaxValue);
                }
            }
            catch { }
        }
    }
}
