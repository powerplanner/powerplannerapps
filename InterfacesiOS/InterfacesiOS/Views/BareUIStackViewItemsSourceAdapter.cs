using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Collections;
using System.Collections.Specialized;
using ToolsPortable;

namespace InterfacesiOS.Views
{
    public abstract class BaseBareUIViewItemsSourceAdapter
    {
        private Func<object, UIView> _createViewFunction;

        public BaseBareUIViewItemsSourceAdapter(Func<object, UIView> createViewFuction)
        {
            _createViewFunction = createViewFuction;
        }

        private NotifyCollectionChangedEventHandler _itemsSourceCollectionChangedHandler;
        private IEnumerable _itemsSource;
        public IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource is INotifyCollectionChanged && _itemsSourceCollectionChangedHandler != null)
                {
                    (_itemsSource as INotifyCollectionChanged).CollectionChanged -= _itemsSourceCollectionChangedHandler;
                }

                _itemsSource = value;
                _itemsSourceCollectionChangedHandler = null;

                if (value is INotifyCollectionChanged)
                {
                    _itemsSourceCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(BareUIStackViewItemsSourceAdapter_CollectionChanged).Handler;
                    (value as INotifyCollectionChanged).CollectionChanged += _itemsSourceCollectionChangedHandler;
                }

                Reset();
            }
        }

        private void BareUIStackViewItemsSourceAdapter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    HandleAdd(e.NewStartingIndex, e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    HandleRemove(e.OldStartingIndex, e.OldItems.Count);
                    break;

                case NotifyCollectionChangedAction.Replace:

                    // First remove the old items
                    HandleRemove(e.OldStartingIndex, e.OldItems.Count);

                    // Then add the new items
                    HandleAdd(e.NewStartingIndex, e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Move:

                    UIView[] viewsToMove = new UIView[e.NewItems.Count];

                    for (int i = 0; i < viewsToMove.Length; i++)
                    {
                        // Grab reference to the view we're going to move
                        viewsToMove[i] = Views[i + e.OldStartingIndex];

                        // And then remove the view
                        RemoveView(viewsToMove[i]);
                    }

                    for (int i = 0; i < viewsToMove.Length; i++)
                    {
                        // Add the view in its new location
                        InsertView(viewsToMove[i], i + e.NewStartingIndex);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
            }
        }

        protected abstract void RemoveView(UIView view);
        protected abstract void InsertView(UIView view, int index);
        protected abstract UIView[] Views { get; }

        private void HandleAdd(int indexToAddAt, IList addedItems)
        {
            for (int i = 0; i < addedItems.Count; i++)
            {
                InsertItem(addedItems[i], i + indexToAddAt);
            }
        }

        private void HandleRemove(int indexToRemoveFrom, int countToRemove)
        {
            for (int i = 0; i < countToRemove; i++)
            {
                RemoveView(Views[indexToRemoveFrom]);
            }
        }

        protected void InsertItem(object item, int index)
        {
            InsertView(CreateView(item), index);
        }

        private void Reset()
        {
            foreach (var subview in Views)
            {
                RemoveView(subview);
            }

            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    var view = CreateView(item);
                    view.TranslatesAutoresizingMaskIntoConstraints = false;
                    InsertView(view, Views.Length);
                }
            }
        }

        protected virtual UIView CreateView(object item)
        {
            return _createViewFunction(item);
        }
    }

    public class BareUIStackViewItemsSourceAdapter : BaseBareUIViewItemsSourceAdapter
    {
        public UIStackView StackView { get; private set; }

        public BareUIStackViewItemsSourceAdapter(UIStackView stackView, Func<object, UIView> createViewFuction)
            : base(createViewFuction)
        {
            StackView = stackView;
        }

        protected override UIView[] Views => StackView.ArrangedSubviews;

        protected override void RemoveView(UIView view)
        {
            StackView.ActuallyRemoveArrangedSubview(view);
        }

        protected override void InsertView(UIView view, int index)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            StackView.InsertArrangedSubview(view, (uint)index);

            if (view.Constraints.Length == 0)
            {
                if (StackView.Axis == UILayoutConstraintAxis.Vertical)
                {
                    view.StretchWidth(StackView);
                }
                else
                {
                    view.StretchHeight(StackView);
                }
            }
        }
    }

    public class BareUIStackViewItemsSourceAdapter<V> : BareUIStackViewItemsSourceAdapter where V : UIView
    {
        public event EventHandler<V> OnViewCreated;

        public BareUIStackViewItemsSourceAdapter(UIStackView stackView) : base(stackView, null)
        {

        }

        protected override UIView CreateView(object item)
        {
            V view = Activator.CreateInstance<V>();

            var prop = view.GetType().GetProperty("DataContext");
            if (prop != null)
            {
                prop.SetValue(view, item);
            }

            OnViewCreated?.Invoke(this, view);

            return view;
        }
    }

    public class BareUIViewItemsSourceAdapter : BaseBareUIViewItemsSourceAdapter
    {
        public UIView View { get; private set; }

        public BareUIViewItemsSourceAdapter(UIView view, Func<object, UIView> createViewFuction) : base(createViewFuction)
        {
            View = view;
        }

        protected override UIView[] Views => View.Subviews;

        protected override void InsertView(UIView view, int index)
        {
            View.InsertSubview(view, index);
        }

        protected override void RemoveView(UIView view)
        {
            view.RemoveFromSuperview();
        }
    }

    public class BareUIViewItemsSourceAdapterAsStackPanel : BareUIViewItemsSourceAdapter
    {
        public BareUIViewItemsSourceAdapterAsStackPanel(UIView view, Func<object, UIView> createViewFuction) : base(view, createViewFuction)
        {
        }

        protected override void InsertView(UIView view, int index)
        {
            UIView above = null;
            if (index > 0)
            {
                above = Views[index - 1];
            }
            UIView below = null;
            if (index < Views.Length)
            {
                below = Views[index];
            }

            view.TranslatesAutoresizingMaskIntoConstraints = false;
            base.InsertView(view, index);
            view.StretchWidth(View);

            UpdateConstraints(above, view, below);
        }

        protected override void RemoveView(UIView view)
        {
            int index = Views.FindIndex(i => i == view);
            UIView above = null;
            if (index > 0)
            {
                above = Views[index - 1];
            }
            UIView below = null;
            if (index + 1 < Views.Length)
            {
                below = Views[index + 1];
            }

            base.RemoveView(view);

            UpdateConstraintsForRemoval(above, view, below);
        }

        private void UpdateConstraints(UIView above, UIView newView, UIView below)
        {
            // Remove the existing constraint that gets changed
            if (above != null)
            {
                // Why should these be FirstOrDefault though? Doesn't that mean I have a logical error if this ever returns null?
                // When there's exactly 1 view, it should have a constraint for the Top, and it should have a constraint for the Bottom
                // For two views...
                // - View 0 - Top = Top of Parent
                // - View 1 - Top = Bottom of View 1, Bottom = Bottom of Parent
                // So if I was inserting at index 1, the above view is View 0, which does NOT have any bottom constraint

                NSLayoutConstraint existing;

                if (below == null)
                {
                    // For when the "above" view is at the bottom, that view will have a constraint that the bottom should be pinned to bottom of parent view... we need to remove that constraint
                    existing = View.Constraints.FirstOrDefault(i => i.FirstItem == above && i.FirstAttribute == NSLayoutAttribute.Bottom && i.SecondItem == View && i.SecondAttribute == NSLayoutAttribute.Bottom);
                    if (existing == null)
                    {
                        throw new InvalidOperationException("The very bottom constraint was missing, logical error somewhere.");
                    }
                }

                else
                {
                    // Otherwise, above view wasn't at the bottom, so it has a constraint (added by the item below it) that the item below's top should be pinned to the bottom of the "above" view
                    // This is also removing the below item's constraint in one, so no need to do anything to below
                    existing = View.Constraints.FirstOrDefault(i => i.FirstItem == below && i.FirstAttribute == NSLayoutAttribute.Top && i.SecondItem == above && i.SecondAttribute == NSLayoutAttribute.Bottom);
                    if (existing == null)
                    {
                        throw new InvalidOperationException("Constraint between item was missing, logical error somewhere.");
                    }
                }

                View.RemoveConstraint(existing);
            }
            else if (below != null)
            {
                // If above was null (since we're inserting into the top), we need to remove the below constraint which is pinned to the top of the parent view
                var existing = View.Constraints.FirstOrDefault(i => i.FirstItem == below && i.FirstAttribute == NSLayoutAttribute.Top && i.SecondItem == View && i.SecondAttribute == NSLayoutAttribute.Top);
                if (existing != null)
                {
                    View.RemoveConstraint(existing);
                }
                else
                {
                    throw new InvalidOperationException("The very top constraint was missing, logical error somewhere.");
                }
            }

            if (above == null)
            {
                // Pin to top of view
                View.AddConstraint(NSLayoutConstraint.Create(
                    newView,
                    NSLayoutAttribute.Top,
                    NSLayoutRelation.Equal,
                    View,
                    NSLayoutAttribute.Top,
                    1,
                    0));
            }
            else
            {
                // Set this new view below the above view
                View.AddConstraint(NSLayoutConstraint.Create(
                    newView,
                    NSLayoutAttribute.Top,
                    NSLayoutRelation.Equal,
                    above,
                    NSLayoutAttribute.Bottom,
                    1,
                    0));
            }

            if (below == null)
            {
                // If this is the last view we need to pin it to bottom
                View.AddConstraint(NSLayoutConstraint.Create(
                    newView,
                    NSLayoutAttribute.Bottom,
                    NSLayoutRelation.Equal,
                    View,
                    NSLayoutAttribute.Bottom,
                    1,
                    0));
            }
        }

        private void UpdateConstraintsForRemoval(UIView above, UIView viewBeingRemoved, UIView below)
        {
            // Remove the existing constraints that gets changed
            // The one connecting to the top gets auto-removed since the viewBeingRemoved was removed
            // Same as the one connecting to below

            if (above != null && below != null)
            {
                View.AddConstraint(NSLayoutConstraint.Create(
                    below,
                    NSLayoutAttribute.Top,
                    NSLayoutRelation.Equal,
                    above,
                    NSLayoutAttribute.Bottom,
                    1,
                    0));
            }
            else if (above != null)
            {
                // If this is the last view we need to pin it to bottom
                View.AddConstraint(NSLayoutConstraint.Create(
                    above,
                    NSLayoutAttribute.Bottom,
                    NSLayoutRelation.Equal,
                    View,
                    NSLayoutAttribute.Bottom,
                    1,
                    0));
            }
            else if (below != null)
            {
                // Pin to top of view
                View.AddConstraint(NSLayoutConstraint.Create(
                    below,
                    NSLayoutAttribute.Top,
                    NSLayoutRelation.Equal,
                    View,
                    NSLayoutAttribute.Top,
                    1,
                    0));
            }
        }
    }
}