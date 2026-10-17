using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidScrollView : DroidView<Vx.Views.ScrollView, FrameLayout>
    {
        private FrameLayout _container;
        private ScrollView _normalScrollView;
        private MyZoomAndPanView _zoomAndPanView;
        public DroidScrollView() : base(new ScrollView(VxDroidExtensions.ApplicationContext))
        {

        }

        protected override void ApplyProperties(Vx.Views.ScrollView oldView, Vx.Views.ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.MaximumZoomScale != newView.MinimumZoomScale || newView.HorizontalScrollEnabled)
            {
                // Dispose of normal scroll view
                if (_normalScrollView != null)
                {
                    View.RemoveView(_normalScrollView);
                    _normalScrollView = null;
                    _container = null;
                }

                // Initialize zoom and pan view if it doesn't exist
                if (_zoomAndPanView == null)
                {
                    // Need to introduce second view so that bottom margin of the child view work correctly inside the scroll view
                    _container = new FrameLayout(View.Context);

                    _zoomAndPanView = new MyZoomAndPanView(View.Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent)
                    };
                    _zoomAndPanView.AddView(_container);
                    View.AddView(_zoomAndPanView);
                }

                _zoomAndPanView.MinZoomFactor = newView.MinimumZoomScale;
                _zoomAndPanView.MaxZoomFactor = newView.MaximumZoomScale;
            }
            else
            {
                // Dispose of zoom and pan view
                if (_zoomAndPanView != null)
                {
                    View.RemoveView(_zoomAndPanView);
                    _zoomAndPanView = null;
                    _container = null;
                }

                // Initialize normal scroll view
                if (_normalScrollView == null)
                {
                    // Need to introduce second view so that bottom margin of the child view work correctly inside the scroll view
                    _container = new FrameLayout(View.Context);

                    _normalScrollView = new ScrollView(View.Context)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent)
                    };
                    _normalScrollView.AddView(_container);
                    View.AddView(_normalScrollView);
                }
            }

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                _container.RemoveAllViews();

                if (view != null)
                {
                    _container.AddView(view.CreateDroidView(VxView));
                }
            });
        }
    }
}