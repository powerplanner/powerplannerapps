using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Collections;
using InterfacesiOS.Controllers;
using ToolsPortable;

namespace InterfacesiOS.Views
{
    public class BareUIInlinePickerView : BareUIInlineEditView
    {
        /// <summary>
        /// Fires every time changed, even programmatically
        /// </summary>
        public event EventHandler<object> SelectionChanged;
        public event EventHandler<SelectionChangingEventArgs> SelectionChanging;

        public class SelectionChangingEventArgs
        {
            public object CurrentSelection { get; private set; }

            public object NewSelection { get; private set; }

            public bool Cancel { get; set; }

            public SelectionChangingEventArgs(object currentSelection, object newSelection)
            {
                CurrentSelection = currentSelection;
                NewSelection = newSelection;
            }
        }

        private UIPickerViewModel _pickerViewModel;

        private IEnumerable<IEnumerable> _collections;
        public IEnumerable<IEnumerable> Components
        {
            get { return _collections; }
            set
            {
                if (ItemsSource != null)
                {
                    throw new InvalidOperationException("You can only assign Components or ItemsSource, not both");
                }

                if (_collections != null)
                {
                    throw new InvalidOperationException("This currently doesn't support changing the currently assigned Components");
                }

                _collections = value;
            }
        }

        private IEnumerable _itemsSource;
        public IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (Components != null)
                {
                    throw new InvalidOperationException("You can only assign Components or ItemsSource, not both");
                }

                _itemsSource = value;
                if (_pickerViewModel is BareUISimplePickerViewModel)
                {
                    (_pickerViewModel as BareUISimplePickerViewModel).ItemsSource = value;
                }
            }
        }

        private Func<object, UIView, UIView> _itemToViewConverter;
        public Func<object, UIView, UIView> ItemToViewConverter
        {
            get { return _itemToViewConverter; }
            set
            {
                _itemToViewConverter = value;
                if (_pickerViewModel is BareUICustomPickerViewModel)
                {
                    (_pickerViewModel as BareUICustomPickerViewModel).ItemToViewConverter = value;
                }
            }
        }

        private Func<object, string> _itemToPreviewStringConverter;
        public Func<object, string> ItemToPreviewStringConverter
        {
            get { return _itemToPreviewStringConverter; }
            set
            {
                _itemToPreviewStringConverter = value;
                UpdateDisplayValue();
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                {
                    return;
                }

                if (FireSelectionChangingAndSeeIfShouldCancel(_selectedItem, value))
                {
                    return;
                }

                _selectedItem = value;

                UpdateDisplayValue();
                SelectionChanged?.Invoke(this, value);
            }
        }

        private int GetExpectedSelectedIndex(object selectedItem, IEnumerable itemsSource)
        {
            if (selectedItem == null || itemsSource == null)
            {
                return 0;
            }

            int index = itemsSource.OfType<object>().ToList().IndexOf(selectedItem);
            if (index == -1)
            {
                return 0;
            }
            return index;
        }

        public BareUIInlinePickerView(UIViewController controller, int left = 0, int right = 0)
            : base(controller, left, right)
        {
            HeaderText = "Item";
            UpdateDisplayValue();
        }

        private void UpdateDisplayValue()
        {
            string text = "";
            if (SelectedItem != null)
            {
                if (ItemToPreviewStringConverter != null)
                {
                    text = ItemToPreviewStringConverter(SelectedItem);
                }
                else
                {
                    if (Components != null && SelectedItem is object[])
                    {
                        text = string.Join(" ", SelectedItem as object[]);
                    }
                    else
                    {
                        text = SelectedItem.ToString();
                    }
                }
            }

            DisplayValue = text;
        }

        protected override ModalEditViewController CreateModalEditViewController(UIViewController parent)
        {
            var modalPicker = new ModalPickerViewController(HeaderText, parent);
            if (Components != null)
            {
                _pickerViewModel = new BareUISimplePickerWithMultipleComponentsViewModel(modalPicker.PickerView)
                {
                    Collections = Components
                };
            }
            else if (ItemToViewConverter != null || ItemsSource.OfType<UIView>().Any())
            {
                _pickerViewModel = new BareUICustomPickerViewModel(modalPicker.PickerView)
                {
                    ItemToViewConverter = ItemToViewConverter,
                    ItemsSource = ItemsSource
                };
            }
            else
            {
                _pickerViewModel = new BareUISimplePickerViewModel(modalPicker.PickerView)
                {
                    ItemsSource = ItemsSource
                };
            }
            modalPicker.PickerView.Model = _pickerViewModel;
            return modalPicker;
        }

        private new ModalPickerViewController ModalController => base.ModalController as ModalPickerViewController;

        protected override void PrepareModalControllerValues()
        {
            if (Components != null)
            {
                int i = 0;
                foreach (var c in Components)
                {
                    object selectedItem = (SelectedItem as object[])?[i];

                    ModalController.PickerView.Select(GetExpectedSelectedIndex(selectedItem, c), i, false);

                    i++;
                }
            }
            else
            {
                ModalController.PickerView.Select(GetExpectedSelectedIndex(SelectedItem, ItemsSource), 0, false);
            }
        }

        protected override void UpdateValuesFromModalController()
        {
            if (Components != null)
            {
                object[] newItem = new object[Components.Count()];
                int i = 0;
                foreach (var itemsSource in Components)
                {
                    newItem[i] = itemsSource.OfType<object>().ElementAt((int)ModalController.PickerView.SelectedRowInComponent(i));

                    i++;
                }

                if (!(SelectedItem is object[]) || !newItem.SequenceEqual(SelectedItem as object[]))
                {
                    SelectedItem = newItem;
                }
            }
            else
            {
                object newItem = ItemsSource.OfType<object>().ElementAt((int)ModalController.PickerView.SelectedRowInComponent(0));

                SelectedItem = newItem;
            }
        }

        private bool FireSelectionChangingAndSeeIfShouldCancel(object oldItem, object newItem)
        {
            if (SelectionChanging != null)
            {
                var changingArgs = new SelectionChangingEventArgs(_selectedItem, newItem);
                SelectionChanging(this, changingArgs);
                if (changingArgs.Cancel)
                {
                    return true;
                }
            }

            return false;
        }
    }
}