using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;

namespace InterfacesiOS.Views
{
    public class BareUICustomPickerViewModel : BareUISimplePickerViewModel
    {
        public BareUICustomPickerViewModel(UIPickerView pickerView) : base(pickerView) { }

        private Func<object, UIView, UIView> _itemToViewConverter;
        public Func<object, UIView, UIView> ItemToViewConverter
        {
            get { return _itemToViewConverter; }
            set
            {
                _itemToViewConverter = value;

                if (ItemsSource != null)
                {
                    ReloadData();
                }
            }
        }

        private WeakReferenceList<UIView> _views = new WeakReferenceList<UIView>();

        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            // We have to make this be in a separate specific class, because otherwise if we override GetView,
            // we can't utilize GetTitle for items that are just simple and only a string of text. We would have to
            // return a UILabel for those string-based items, which considering how the views aren't recycled well
            // would be awful for performance.
            var item = ItemsSource.OfType<object>().ElementAt((int)row);

            if (ItemToViewConverter != null)
            {
                // Have to implement our own recycling since the view it provides is always null
                var recycledView = _views.FirstOrDefault(i => i.Superview == null);

                var customView = ItemToViewConverter(item, recycledView);
                if (customView != recycledView)
                {
                    _views.Add(customView);
                }
                return customView;
            }

            if (item is UIView)
            {
                return item as UIView;
            }

            throw new InvalidOperationException("No custom views available");
        }
    }
}