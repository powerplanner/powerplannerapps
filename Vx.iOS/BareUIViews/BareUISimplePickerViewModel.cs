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
    /// <summary>
    /// Simply returns strings for given items, UNLESS items are UIViews, which in that case returns the views themselves
    /// </summary>
    public class BareUISimplePickerViewModel : UIPickerViewModel
    {
        private UIPickerView _pickerView;

        public BareUISimplePickerViewModel(UIPickerView pickerView)
        {
            _pickerView = pickerView;
        }

        private NotifyCollectionChangedEventHandler _itemsSourceCollectionChangedHandler;
        private IEnumerable _itemsSource;
        public IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource == value)
                {
                    return;
                }

                if (_itemsSource is INotifyCollectionChanged && _itemsSourceCollectionChangedHandler != null)
                {
                    (_itemsSource as INotifyCollectionChanged).CollectionChanged -= _itemsSourceCollectionChangedHandler;
                }

                bool hadExisting = _itemsSource != null;

                _itemsSource = value;
                _itemsSourceCollectionChangedHandler = null;

                if (value is INotifyCollectionChanged)
                {
                    _itemsSourceCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(BareUISimplePickerViewModel_CollectionChanged).Handler;
                    (value as INotifyCollectionChanged).CollectionChanged += _itemsSourceCollectionChangedHandler;
                }

                if (hadExisting)
                {
                    ReloadData();
                }
            }
        }

        private void BareUISimplePickerViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == ItemsSource)
            {
                ReloadData();
            }
        }

        protected void ReloadData()
        {
            _pickerView.ReloadAllComponents();
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            // Number of spinner columns
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            // Total items to display
            if (ItemsSource == null)
            {
                return 0;
            }

            return ItemsSource.OfType<object>().Count();
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return ItemsSource.OfType<object>().ElementAt((int)row).ToString();
        }
    }
}