using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Foundation;
using ToolsPortable;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUISimplePickerWithMultipleComponentsViewModel : UIPickerViewModel
    {
        private UIPickerView _pickerView;

        public BareUISimplePickerWithMultipleComponentsViewModel(UIPickerView pickerView)
        {
            _pickerView = pickerView;
        }

        private IEnumerable<IEnumerable> _collections;
        /// <summary>
        /// A collection of items sources. Note that the collections don't support observing changes (you can't dynamically add a collection), but the items sources inside DO.
        /// </summary>
        public IEnumerable<IEnumerable> Collections
        {
            get { return _collections; }
            set
            {
                if (_collections == value)
                {
                    return;
                }

                if (_collections != null)
                {
                    throw new InvalidOperationException("This model only allows assigning the collections once. Supporting changing this would require more coding.");
                }

                if (value == null)
                {
                    return;
                }

                _collections = value;
                foreach (var itemsSource in value)
                {
                    if (itemsSource is INotifyCollectionChanged)
                    {
                        (itemsSource as INotifyCollectionChanged).CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(BareUISimplePickerWithMultipleComponentsViewModel_CollectionChanged).Handler;
                    }
                }
            }
        }

        private void BareUISimplePickerWithMultipleComponentsViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int index = 0;
            foreach (var c in Collections)
            {
                if (c == sender)
                {
                    break;
                }

                index++;
            }

            _pickerView.ReloadComponent(index);
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            if (Collections == null)
            {
                return 0;
            }

            // Number of spinner columns
            return Collections.Count();
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            // Total items to display
            if (Collections == null)
            {
                return 0;
            }

            return Collections.ElementAt((int)component).OfType<object>().Count();
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return Collections.ElementAt((int)component).OfType<object>().ElementAt((int)row).ToString();
        }
    }
}