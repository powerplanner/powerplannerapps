using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Collections;
using System.Collections.Specialized;
using InterfacesDroid.DataTemplates;
using ToolsPortable;

namespace InterfacesDroid.Views
{
    public class ItemsControlWrapper
    {
        public ViewGroup ViewGroup { get; private set; }

        /// <summary>
        /// Anyone using this must hold a strong reference to the wrapper, otherwise list changes will
        /// stop occurring since the wrapper will be disposed.
        /// </summary>
        /// <param name="viewGroup"></param>
        public ItemsControlWrapper(ViewGroup viewGroup)
        {
            if (viewGroup == null)
                throw new ArgumentNullException(nameof(viewGroup));

            ViewGroup = viewGroup;

            // We really shouldn't give the ItemsSource a strong reference... that would mean this could be kept around
            // forever. Although it doesn't really add any impact.
            // But if we don't give the ItemsSource a strong reference, we need to make sure that our view is holding onto every
            // ItemsControlWrapper that it creates, otherwise it will be disposed and the event handler dropped.
        }
        
        private IEnumerable _itemsSource;
        private NotifyCollectionChangedEventHandler _collectionChangedHandler;
        public IEnumerable ItemsSource
        {
            get { return _itemsSource; }

            set
            {
                if (GetViewGroup() == null)
                    return;

                if (_itemsSource is INotifyCollectionChanged && _collectionChangedHandler != null)
                {
                    (_itemsSource as INotifyCollectionChanged).CollectionChanged -= _collectionChangedHandler;
                }

                _itemsSource = value;

                if (_itemsSource is INotifyCollectionChanged)
                {
                    _collectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(ItemsSource_CollectionChanged).Handler;
                    (_itemsSource as INotifyCollectionChanged).CollectionChanged += _collectionChangedHandler;
                }

                ResetFromItemsSource();
            }
        }

        private IDataTemplate _itemTemplate;
        public IDataTemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set
            {
                _itemTemplate = value;
                ResetFromItemsSource();
            }
        }

        /// <summary>
        /// Gets the view group if it's still active, otherwise returns null and de-registers itself.
        /// </summary>
        /// <returns></returns>
        private ViewGroup GetViewGroup()
        {
            return ViewGroup;
        }

        private void ResetFromItemsSource()
        {
            ViewGroup viewGroup = GetViewGroup();

            // If view group reference has been lost, do nothing
            if (viewGroup == null)
            {
                return;
            }

            // Remove all children
            try
            {
                viewGroup.RemoveAllViews();

                if (ItemTemplate == null || ItemsSource == null)
                {
                    return;
                }

                // And add the views
                foreach (var item in ItemsSource)
                {
                    viewGroup.AddView(GetViewFromItem(item, viewGroup));
                }
            }
            catch (ObjectDisposedException)
            {
                DisposeViewGroup();
            }
        }

        private void DisposeViewGroup()
        {
            // This means that the view object has been disposed, so we should stop sending events to it

            // Set the view to null
            ViewGroup = null;

            // And de-register the collection changed event
            if (_collectionChangedHandler != null && ItemsSource is INotifyCollectionChanged)
            {
                (ItemsSource as INotifyCollectionChanged).CollectionChanged -= _collectionChangedHandler;
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != ItemsSource)
            {
                return;
            }

            ViewGroup viewGroup = GetViewGroup();
            if (viewGroup == null || ItemTemplate == null)
            {
                return;
            }

            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        ResetFromItemsSource();
                        break;

                    case NotifyCollectionChangedAction.Add:
                        HandleAdd(viewGroup, e.NewStartingIndex, e.NewItems);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        HandleRemove(viewGroup, e.OldStartingIndex, e.OldItems.Count);
                        break;

                    case NotifyCollectionChangedAction.Replace:

                        // First remove the old items
                        HandleRemove(viewGroup, e.OldStartingIndex, e.OldItems.Count);

                        // Then add the new items
                        HandleAdd(viewGroup, e.NewStartingIndex, e.NewItems);
                        break;

                    case NotifyCollectionChangedAction.Move:

                        View[] viewsToMove = new View[e.NewItems.Count];

                        for (int i = 0; i < viewsToMove.Length; i++)
                        {
                            // Grab reference to the view we're going to move
                            viewsToMove[i] = viewGroup.GetChildAt(i + e.OldStartingIndex);

                            // And then remove the view
                            viewGroup.RemoveViewAt(i + e.OldStartingIndex);
                        }

                        for (int i = 0; i < viewsToMove.Length; i++)
                        {
                            // Add the view in its new location
                            viewGroup.AddView(viewsToMove[i], i + e.NewStartingIndex);
                        }

                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                DisposeViewGroup();
            }
        }

        private void HandleAdd(ViewGroup viewGroup, int indexToAddAt, IList addedItems)
        {
            foreach (var item in addedItems)
            {
                viewGroup.AddView(GetViewFromItem(item, viewGroup), indexToAddAt);
                indexToAddAt++;
            }
        }

        private void HandleRemove(ViewGroup viewGroup, int indexRemoveStartedFrom, int countRemoved)
        {
            viewGroup.RemoveViews(indexRemoveStartedFrom, countRemoved);
        }

        private View GetViewFromItem(object item, ViewGroup root)
        {
            if (ItemTemplate == null)
                throw new NullReferenceException("ItemTeplate was null");

            return ItemTemplate.CreateView(item, root);
        }
    }
}