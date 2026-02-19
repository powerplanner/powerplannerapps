using CoreGraphics;
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
        private ContextMenuInteractionHandler _cmInteractionHandler;

        public iOSView()
        {
            base.View = new UIViewWrapper(Activator.CreateInstance<N>());
        }

        public UIViewWrapper ViewWrapper => base.View;
        public new N View => ViewWrapper.View as N;

        public iOSView(N view)
        {
            base.View = new UIViewWrapper(view);
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            View.Alpha = newView.Opacity;

            ViewWrapper.Margin = newView.Margin;
            ViewWrapper.Height = newView.Height;
            ViewWrapper.Width = newView.Width;
            ViewWrapper.HorizontalAlignment = newView.HorizontalAlignment;
            ViewWrapper.VerticalAlignment = newView.VerticalAlignment;

            UILinearLayout.SetWeight(ViewWrapper, LinearLayout.GetWeight(newView));

            if (newView.Tapped != null && _tapGestureRecognizer == null)
            {
                _tapGestureRecognizer = new UITapGestureRecognizer();

                _tapGestureRecognizer.AddTarget(() => VxView.Tapped?.Invoke());

                View.AddGestureRecognizer(_tapGestureRecognizer);
            }

            if (newView.ContextMenu != null)
            {
                if (_cmInteractionHandler == null)
                {
                    _cmInteractionHandler = new ContextMenuInteractionHandler(this);
                }
                if (!View.Interactions.Contains(_cmInteractionHandler.Interaction))
                {
                    View.AddInteraction(_cmInteractionHandler.Interaction);
                }
            }
            else
            {
                if (_cmInteractionHandler != null && View.Interactions.Length > 0)
                {
                    View.RemoveInteraction(View.Interactions[0]);
                }
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
                    afterTransfer(view.NativeUIView());
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


        private class ContextMenuInteractionHandler : NSObject, IUIContextMenuInteractionDelegate
        {
            private iOSView<V, N> _iosView;
            public UIContextMenuInteraction Interaction { get; private set; }

            public ContextMenuInteractionHandler(iOSView<V, N> iosView)
            {
                _iosView = iosView;
                Interaction = new UIContextMenuInteraction(this);
            }

            public UIContextMenuConfiguration GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
            {
                List<UIMenuElement> elementList = new List<UIMenuElement>();

                var cm = _iosView.VxView.ContextMenu?.Invoke();
                if (cm == null)
                {
                    return null;
                }

                return UIContextMenuConfiguration.Create(null, null, a => VxiOSContextMenu.CreateMenu(cm));
            }
        }
    }
}