using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public abstract class iOSView<V, N> : NativeView<V, UIViewWrapper> where V : View where N : UIView
    {
        private UITapGestureRecognizer _tapGestureRecognizer;

        public iOSView()
        {
            base.View = new UIViewWrapper(Activator.CreateInstance<N>());
            //View.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        public UIViewWrapper ViewWrapper => base.View;
        public new N View => ViewWrapper.View as N;

        public iOSView(N view)
        {
            base.View = new UIViewWrapper(view);
            //view.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            View.Alpha = newView.Opacity;

            ViewWrapper.Margin = newView.Margin;
            ViewWrapper.Height = newView.Height;
            ViewWrapper.Width = newView.Width;
            ViewWrapper.HorizontalAlignment = newView.HorizontalAlignment;
            ViewWrapper.VerticalAlignment = newView.VerticalAlignment;

            // Clearing heights on buttons does some funky things, so keeping this scoped to border for now
            //if (newView is Border)
            //{
            //    if (oldView == null || newView.Width != oldView.Width)
            //    {
            //        View.ClearWidth();

            //        if (!float.IsNaN(newView.Width))
            //        {
            //            View.SetWidth(newView.Width);
            //        }
            //    }

            //    if (oldView == null || newView.Height != oldView.Height)
            //    {
            //        View.ClearHeight();

            //        if (!float.IsNaN(newView.Height))
            //        {
            //            View.SetHeight(newView.Height);
            //        }
            //    }
            //}

            if (newView.Tapped != null && _tapGestureRecognizer == null)
            {
                _tapGestureRecognizer = new UITapGestureRecognizer();

                _tapGestureRecognizer.AddTarget(() => VxView.Tapped?.Invoke());

                View.AddGestureRecognizer(_tapGestureRecognizer);
            }
        }

        protected void ReconcileContent(View oldContent, View newContent, Action<UIView> afterSubviewAddedAction, Action<UIView> afterTransfer = null)
        {
            VxReconciler.Reconcile(oldContent, newContent, view =>
            {
                View.RemoveAllConstraints();
                View.ClearAllSubviews();

                if (view != null)
                {
                    var child = view.CreateUIView(VxView).View;
                    child.TranslatesAutoresizingMaskIntoConstraints = false;
                    View.AddSubview(child);
                    afterSubviewAddedAction(child);
                }
            }, transferView: view =>
            {
                if (afterTransfer != null)
                {
                    afterTransfer(view.NativeView.View as UIView);
                }
            });
        }

        protected void ReconcileContent(View oldContent, View newContent, Thickness? overriddenChildMargin = null)
        {
            ReconcileContent(oldContent, newContent, subview =>
            {
                var modifiedMargin = (overriddenChildMargin ?? newContent.Margin).AsModified();
                subview.StretchWidthAndHeight(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);

                // Prevent this from stretching and filling horizontal width
                subview.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Horizontal);
            }, afterTransfer: subview =>
            {
                // If we need to change the margins
                if (overriddenChildMargin != null || oldContent.Margin != newContent.Margin)
                {
                    View.RemoveAllConstraintsAffectingSubview(subview);
                    var modifiedMargin = (overriddenChildMargin ?? newContent.Margin).AsModified();
                    subview.StretchWidthAndHeight(View, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);
                }
            });
        }

        protected void ReconcileContentNew(View oldContent, View newContent, Action<View> changeView)
        {
            VxReconciler.Reconcile(oldContent, newContent, changeView);
        }

        protected void ReconcileContentNew(View oldContent, View newContent)
        {
            ReconcileContentNew(oldContent, newContent, view =>
            {
                var contentView = View as UIContentView;

                if (view != null)
                {
                    var child = view.CreateUIView(VxView);
                    contentView.Content = child;
                }
                else
                {
                    contentView.Content = null;
                }
            });
        }
    }
}