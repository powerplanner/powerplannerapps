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
using InterfacesDroid.DataTemplates;
using System.Collections.Specialized;
using ToolsPortable;
using InterfacesDroid.Views;
using AndroidX.RecyclerView.Widget;

namespace InterfacesDroid.Adapters
{
    public abstract class ObservableRecyclerViewAdapter : RecyclerView.Adapter
    {
        public const int LIST_EMPTY_ITEM_TYPE = 101;
        public const int LIST_FOOTER_ITEM_TYPE = 102;

        private object _emptyObject = new object();

        private object _footer;
        /// <summary>
        /// You need to assign <see cref="CreateViewHolderForFooter"/> before assigning this.
        /// </summary>
        public object Footer
        {
            get { return _footer; }
            set
            {
                if (_footer == value)
                {
                    return;
                }

                var oldFooter = _footer;
                _footer = value;

                // If added footer
                if (oldFooter == null)
                {
                    NotifyItemInserted(GetFooterPosition());
                }
                else
                {
                    // If removed footer
                    if (value == null)
                    {
                        NotifyItemRemoved(GetFooterPosition());
                    }
                    // Else footer was changed
                    else
                    {
                        NotifyItemChanged(GetFooterPosition());
                    }
                }
            }
        }

        public Func<ViewGroup, object, RecyclerView.ViewHolder> CreateViewHolderForEmptyList { get; set; }

        public Func<ViewGroup, object, RecyclerView.ViewHolder> CreateViewHolderForFooter { get; set; }

        private int GetFooterPosition()
        {
            return GetCountIncludingEmpty();
        }

        private int GetCountIncludingEmpty()
        {
            if (ItemsSource == null || ItemsSource.Count == 0)
            {
                return CreateViewHolderForEmptyList != null ? 1 : 0;
            }

            return ItemsSource.Count;
        }

        private IReadOnlyList<object> _itemsSource;
        private NotifyCollectionChangedEventHandler _collectionChangedHandler;
        public IReadOnlyList<object> ItemsSource
        {
            get
            {
                return _itemsSource;
            }

            set
            {
                // Unregister old
                var old = ItemsSource;
                if (old is INotifyCollectionChanged)
                {
                    (old as INotifyCollectionChanged).CollectionChanged -= _collectionChangedHandler;
                }

                _itemsSource = value;

                // Register new
                if (value is INotifyCollectionChanged)
                {
                    _collectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(ItemsSource_CollectionChanged).Handler;
                    (value as INotifyCollectionChanged).CollectionChanged += _collectionChangedHandler;
                }

                NotifyDataSetChanged();
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // If list was currently empty and was showing the empty view
                    if (ItemsSource.Count == e.NewItems.Count && CreateViewHolderForEmptyList != null)
                    {
                        NotifyItemRangeChanged(0, 1);

                        if (e.NewItems.Count > 1)
                        {
                            NotifyItemRangeInserted(1, e.NewItems.Count - 1);
                        }
                    }
                    else
                    {
                        NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    // If removed all items
                    if (ItemsSource.Count == 0 && CreateViewHolderForEmptyList != null)
                    {
                        NotifyItemRangeChanged(0, 1);

                        if (e.OldItems.Count > 1)
                        {
                            NotifyItemRangeRemoved(1, e.OldItems.Count - 1);
                        }
                    }
                    else
                    {
                        NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    NotifyItemRangeChanged(e.NewStartingIndex, e.NewItems.Count);
                    break;

                case NotifyCollectionChangedAction.Move:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        NotifyItemMoved(e.OldStartingIndex + i, e.NewStartingIndex + i);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    NotifyDataSetChanged();
                    break;
            }
        }

        public override int ItemCount
        {
            get
            {
                int countWithEmpty = GetCountIncludingEmpty();
                if (Footer != null)
                {
                    countWithEmpty++;
                }
                return countWithEmpty;
            }
        }

        public object GetItem(int position)
        {
            if (IsEmptyPosition(position))
            {
                return _emptyObject;
            }

            if (ItemsSource != null && position < ItemsSource.Count)
            {
                return ItemsSource[position];
            }

            // If it's a footer
            if (IsFooterPosition(position))
            {
                return Footer;
            }

#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            throw new IndexOutOfRangeException($"position {position} wasn't found");
        }

        public bool IsFooterPosition(int position)
        {
            return Footer != null && position == GetFooterPosition();
        }

        public bool IsEmptyPosition(int position)
        {
            if (CreateViewHolderForEmptyList != null && position == 0 && (ItemsSource == null || ItemsSource.Count == 0))
            {
                return true;
            }

            return false;
        }

        public override int GetItemViewType(int position)
        {
            if (IsEmptyPosition(position))
            {
                return LIST_EMPTY_ITEM_TYPE;
            }

            if (IsFooterPosition(position))
            {
                return LIST_FOOTER_ITEM_TYPE;
            }

            return GetItemViewType(GetItem(position));
        }

        protected abstract int GetItemViewType(object item);

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            OnBindViewHolder(holder, GetItem(position));
        }

        /// <summary>
        /// Will automatically assign data context on view, so that if your view has a DataContext property, you don't need to do anything!
        /// </summary>
        /// <param name="holder"></param>
        /// <param name="item"></param>
        protected virtual void OnBindViewHolder(RecyclerView.ViewHolder holder, object item)
        {
#if DEBUG
            try
            {
#endif
                var view = holder.ItemView;

                var dataContextProperty = view.GetType().GetProperties().FirstOrDefault(i => i.Name.Equals("DataContext"));

                if (dataContextProperty != null)
                    dataContextProperty.SetValue(view, item);
#if DEBUG
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
#endif
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case LIST_EMPTY_ITEM_TYPE:
                    if (CreateViewHolderForEmptyList != null)
                    {
                        return CreateViewHolderForEmptyList(parent, _emptyObject);
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        throw new NotImplementedException("CreateViewHolderForEmptyList wasn't assigned");
                    }
                case LIST_FOOTER_ITEM_TYPE:
                    if (CreateViewHolderForFooter != null)
                    {
                        return CreateViewHolderForFooter(parent, Footer);
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        throw new NotImplementedException("CreateViewHolderForFooter wasn't assigned");
                    }

                default:
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    throw new NotImplementedException($"viewType {viewType} was unhandled.");
            }
        }

        public override long GetItemId(int position)
        {
            return GetItem(position).GetHashCode();
        }
    }
}