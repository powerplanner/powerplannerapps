using CoreGraphics;
using Foundation;
using GameController;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public abstract class iOSView<V, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] N> : NativeView<V, UIViewWrapper> where V : View where N : UIView
    {
        private UITapGestureRecognizer _tapGestureRecognizer;
        private ContextMenuInteractionHandler _cmInteractionHandler;
        private UIDropInteraction _dropInteraction;
        private iOSDropInteractionDelegate _dropDelegate;
        private UIDragInteraction _dragInteraction;
        private iOSDragInteractionDelegate _dragDelegate;

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
            ViewWrapper.MinWidth = newView.MinWidth;
            ViewWrapper.MaxWidth = newView.MaxWidth;
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
                    View.AddInteraction(_cmInteractionHandler.Interaction);
                }
            }
            else
            {
                if (_cmInteractionHandler != null)
                {
                    View.RemoveInteraction(_cmInteractionHandler.Interaction);
                    _cmInteractionHandler = null;
                }
            }

            // If drop is enabled
            if (newView.AllowDrop && newView.DragOver != null && newView.Drop != null)
            {
                // If we haven't set up the drop interaction yet
                if (_dropInteraction == null)
                {
                    _dropDelegate = new iOSDropInteractionDelegate(OnDragOver, OnDrop);
                    _dropInteraction = new UIDropInteraction(_dropDelegate);
                    View.AddInteraction(_dropInteraction);
                }
            }
            else
            {
                if (_dropInteraction != null)
                {
                    View.RemoveInteraction(_dropInteraction);
                    _dropInteraction = null;
                    _dropDelegate = null;
                }
            }

            // If drag is enabled
            if (newView.CanDrag && newView.DragStarting != null)
            {
                // If we haven't set up the drag interaction yet
                if (_dragInteraction == null)
                {
                    _dragDelegate = new iOSDragInteractionDelegate(OnDragStarting);
                    _dragInteraction = new UIDragInteraction(_dragDelegate)
                    {
                        // Defaults to false on iPhone (true only on iPad), so must enable explicitly
                        //Enabled = true
                    };
                    View.AddInteraction(_dragInteraction);
                    //View.UserInteractionEnabled = true;
                }
            }
            else
            {
                if (_dragInteraction != null)
                {
                    View.RemoveInteraction(_dragInteraction);
                    _dragInteraction = null;
                    _dragDelegate = null;
                }
            }

            // Any property change (text, image, size, alignment, etc.) may alter this view's
            // desired size, so invalidate its cached measurement and propagate up the tree.
            ViewWrapper.InvalidateMeasure();
        }

        private void OnDragStarting(Vx.Views.DragDrop.DragStartingEventArgs args)
        {
            VxView.DragStarting?.Invoke(args);
        }

        private class iOSDragInteractionDelegate : NSObject, IUIDragInteractionDelegate
        {
            private Action<Vx.Views.DragDrop.DragStartingEventArgs> _onDragStarting;

            public iOSDragInteractionDelegate(Action<Vx.Views.DragDrop.DragStartingEventArgs> onDragStarting)
            {
                _onDragStarting = onDragStarting;
            }

            // Required: Tell iOS what data is being dragged when the gesture starts
            [Export("dragInteraction:itemsForBeginningSession:")]
            public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
            {
                // Create a new drag starting event args
                var args = new Vx.Views.DragDrop.DragStartingEventArgs();

                // Call the onDragStarting action
                _onDragStarting.Invoke(args);

                var provider = new NSItemProvider(new NSString("internal_drop"));

                var dragItem = new UIDragItem(provider)
                {
                    LocalObject = new NSObjectDataPackage(args.Data)
                };

                return [dragItem];
            }
            
            // This restricts the drag context to your app only
            [Export("dragInteraction:sessionIsRestrictedToDraggingApplication:")]
            public bool SessionIsRestrictedToDraggingApplication(UIDragInteraction interaction, IUIDragSession session)
            {
                // Equivalent to "return YES;" in Objective-C
                return true; 
            }
        }

        private class NSObjectDataPackage : NSObject
        {
            public Vx.Views.DragDrop.DataPackage DataPackage { get; }

            public NSObjectDataPackage(Vx.Views.DragDrop.DataPackage dataPackage)
            {
                DataPackage = dataPackage;
            }
        }

        private class iOSDropInteractionDelegate : NSObject, IUIDropInteractionDelegate
        {
            private readonly Action<Vx.Views.DragDrop.DragEventArgs> _onDragOver;
            private readonly Action<Vx.Views.DragDrop.DragEventArgs> _onDrop;

            public iOSDropInteractionDelegate(Action<Vx.Views.DragDrop.DragEventArgs> onDragOver, Action<Vx.Views.DragDrop.DragEventArgs> onDrop)
            {
                _onDragOver = onDragOver;
                _onDrop = onDrop;
            }

            private static Vx.Views.DragDrop.DragEventArgs GetVxDragEventArgs(IUIDropSession session)
            {
                var vxDataPackage = GetVxDataPackage(session);

                return new Vx.Views.DragDrop.DragEventArgs(vxDataPackage, GetModifiers());
            }

            // Queries live keyboard state (works on iPad/Mac Catalyst, iOS 14+)
            private static Vx.Views.DragDrop.DragDropModifiers GetModifiers()
            {
                var modifiers = Vx.Views.DragDrop.DragDropModifiers.None;

                var keyboardInput = GCKeyboard.CoalescedKeyboard?.KeyboardInput;
                if (keyboardInput != null)
                {
                    bool controlPressed =
                        (keyboardInput.GetButton(GCKeyCode.LeftControl)?.IsPressed ?? false) ||
                        (keyboardInput.GetButton(GCKeyCode.RightControl)?.IsPressed ?? false);

                    if (controlPressed)
                    {
                        modifiers |= Vx.Views.DragDrop.DragDropModifiers.Control;
                    }
                }

                return modifiers;
            }

            private static Vx.Views.DragDrop.DataPackage GetVxDataPackage(IUIDropSession session)
            {
                if (session.Items.Length == 1)
                {
                    var pkg = session.Items[0].LocalObject as NSObjectDataPackage;
                    return pkg?.DataPackage ?? new Vx.Views.DragDrop.DataPackage();
                }
                return new Vx.Views.DragDrop.DataPackage();
            }

            // Tell iOS if the view can handle the current drag session data type
            [Export("dropInteraction:canHandleSession:")]
            public bool CanHandleSession(UIDropInteraction interaction, IUIDropSession session)
            {
                var vxArgs = GetVxDragEventArgs(session);

                _onDragOver.Invoke(vxArgs);

                return vxArgs.AcceptedOperation != Vx.Views.DragDrop.DataPackageOperation.None;
            }

            // Define the visual proposal feedback (Copy, Move, Cancel)
            [Export("dropInteraction:sessionDidUpdate:")]
            public UIDropProposal SessionDidUpdate(UIDropInteraction interaction, IUIDropSession session)
            {
                var vxArgs = GetVxDragEventArgs(session);

                _onDragOver.Invoke(vxArgs);

                switch (vxArgs.AcceptedOperation)
                {
                    case Vx.Views.DragDrop.DataPackageOperation.Copy:
                        return new UIDropProposal(UIDropOperation.Copy);
                    case Vx.Views.DragDrop.DataPackageOperation.Move:
                        return new UIDropProposal(UIDropOperation.Move);
                    default:
                        return new UIDropProposal(UIDropOperation.Forbidden);
                }
            }

            // Process the dropped item data
            [Export("dropInteraction:performDrop:")]
            public void PerformDrop(UIDropInteraction interaction, IUIDropSession session)
            {
                var vxArgs = GetVxDragEventArgs(session);

                _onDrop.Invoke(vxArgs);
            }
        }

        private void OnDragOver(Vx.Views.DragDrop.DragEventArgs e)
        {
            VxView.DragOver?.Invoke(e);
        }

        private void OnDrop(Vx.Views.DragDrop.DragEventArgs e)
        {
            VxView.Drop?.Invoke(e);
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