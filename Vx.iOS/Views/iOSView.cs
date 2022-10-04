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

            UILinearLayout.SetWeight(ViewWrapper, LinearLayout.GetWeight(newView));

            if (newView.Tapped != null && _tapGestureRecognizer == null)
            {
                _tapGestureRecognizer = new UITapGestureRecognizer();

                _tapGestureRecognizer.AddTarget(() => VxView.Tapped?.Invoke());

                View.AddGestureRecognizer(_tapGestureRecognizer);
            }

            if (newView.ContextMenu != null && UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
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

                UIMenu menu = CreateMenu(cm.Items);
                return UIContextMenuConfiguration.Create(null, null, a => menu);
            }

            private static UIMenu CreateMenu(IEnumerable<IContextMenuItem> contextMenuItems)
            {
                UIMenuElement[] actions = CreateMenuElements(contextMenuItems);
                return UIMenu.Create(actions);
            }

            private static UIMenuElement[] CreateMenuElements(IEnumerable<IContextMenuItem> contextMenuItems)
            {
                if (contextMenuItems.OfType<ContextMenuSeparator>().Any())
                {
                    List<UIMenuElement> groupedActions = new List<UIMenuElement>();
                    foreach (var g in SeparatedIntoGroups(contextMenuItems))
                    {
                        groupedActions.Add(UIMenu.Create("", null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, CreateNonSeparatedMenuElements(g)));
                    }
                    return groupedActions.ToArray();
                }
                else
                {
                    return CreateNonSeparatedMenuElements(contextMenuItems);
                }
            }

            private static IEnumerable<IContextMenuItem[]> SeparatedIntoGroups(IEnumerable<IContextMenuItem> flatList)
            {
                List<IContextMenuItem> currGroup = new List<IContextMenuItem>();
                foreach (var item in flatList)
                {
                    if (item is ContextMenuSeparator)
                    {
                        if (currGroup.Count > 0)
                        {
                            yield return currGroup.ToArray();
                        }
                        currGroup = new List<IContextMenuItem>();
                    }
                    else
                    {
                        currGroup.Add(item);
                    }
                }
                if (currGroup.Count > 0)
                {
                    yield return currGroup.ToArray();
                }
            }

            private static UIMenuElement[] CreateNonSeparatedMenuElements(IEnumerable<IContextMenuItem> contextMenuItems)
            {
                var actions = new List<UIMenuElement>();

                foreach (var item in contextMenuItems)
                {
                    var el = CreateMenuElement(item);
                    if (el != null)
                    {
                        actions.Add(el);
                    }
                }

                return actions.ToArray();
            }

            private static UIMenuElement CreateMenuElement(IContextMenuItem item)
            {
                if (item is ContextMenuItem cmItem)
                {
                    var a = UIAction.Create(cmItem.Text, cmItem.Glyph.GlyphToUIImage(), null, _ => cmItem.Click?.Invoke());
                    if (cmItem.Style == ContextMenuItemStyle.Destructive)
                    {
                        a.Attributes = UIMenuElementAttributes.Destructive;
                    }
                    return a;
                }
                else if (item is ContextMenuRadioItem rItem)
                {
                    return UIAction.Create(rItem.Text, rItem.IsChecked ? UIImage.GetSystemImage("checkmark") : null, null, _ => rItem.Click?.Invoke());
                }
                else if (item is ContextMenuSubItem cmItemGroup)
                {
                    return UIMenu.Create(cmItemGroup.Text, cmItemGroup.Glyph.GlyphToUIImage(), UIMenuIdentifier.None, default(UIMenuOptions), CreateMenuElements(cmItemGroup.Items));
                }

                return null;
            }

            private static UIImage GetSystemImageHelper(string imgName)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    return UIImage.GetSystemImage(imgName);
                }
                return null;
            }
        }
    }
}