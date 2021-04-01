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
    public  class BareUITableViewSource<V> : UITableViewSource where V : UIView
    {
        public IEnumerable ItemsSource { get; private set; }
        public UITableView TableView { get; private set; }
        public event EventHandler<object> ItemSelected;
        private const string CELL_IDENTIFIER = "TableCell";

        public BareUITableViewSource(UITableView tableView, IEnumerable itemsSource)
        {
            ItemsSource = itemsSource;
            TableView = tableView;

            if (ItemsSource is INotifyCollectionChanged)
            {
                (ItemsSource as INotifyCollectionChanged).CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(ItemsSource_CollectionChanged).Handler;
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TableView.ReloadData();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            object item = ItemsSource.OfType<object>().ElementAt(indexPath.Row);
            CustomUITableViewCell cell = tableView.DequeueReusableCell(CELL_IDENTIFIER) as CustomUITableViewCell;
            if (cell == null)
            {
                cell = new CustomUITableViewCell(CELL_IDENTIFIER)
                {
                    ContentView = CreateView()
                };
            }

            var dataContextProp = cell.ContentView.GetType().GetProperty("DataContext");
            if (dataContextProp != null)
            {
                dataContextProp.SetValue(cell.ContentView, item);
            }

            return cell;
        }

        protected virtual UIView CreateView()
        {
            return Activator.CreateInstance<V>();
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return ItemsSource.OfType<object>().Count();
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            ItemSelected?.Invoke(this, ItemsSource.OfType<object>().ElementAtOrDefault(indexPath.Row));
        }

        private class CustomUITableViewCell : UITableViewCell
        {
            public CustomUITableViewCell(string reuseIdentifier)
            {
                _reuseIdentifier = new NSString(reuseIdentifier);
            }

            private NSString _reuseIdentifier;
            public override NSString ReuseIdentifier
            {
                get { return _reuseIdentifier; }
            }

            private UIView _contentView;
            public new UIView ContentView
            {
                get
                {
                    return _contentView;
                }
                set
                {
                    _contentView = value;
                    value.TranslatesAutoresizingMaskIntoConstraints = false;
                    base.ContentView.AddSubview(value);
                    value.StretchWidthAndHeight(base.ContentView);
                }
            }
        }
    }
}