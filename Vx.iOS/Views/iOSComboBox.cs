using System;
using System.Linq;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSComboBox : iOSView<Vx.Views.ComboBox, UIView>
    {
        private UILabel _header;
        private UIView _valueContainer;
        private UILabel _value;

        private ModalPickerViewController ModalController;
        private UIPickerViewModel _pickerViewModel;

        public iOSComboBox()
        {
            _header = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _valueContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _valueContainer.BackgroundColor = UIColorCompat.TertiarySystemFillColor;
            _valueContainer.ClipsToBounds = true;
            _valueContainer.Layer.CornerRadius = 10;

            _value = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 1
            };
            _valueContainer.Add(_value);
            _value.StretchWidthAndHeight(_valueContainer, 10, 0, 10, 0);

            View.Add(_header);
            View.Add(_valueContainer);

            _header.StretchWidth(View);
            _valueContainer.StretchWidth(View);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header]-4-[valueContainer(36)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", _header,
                "valueContainer", _valueContainer));

            // Handle clicks
            UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer();
            tapRecognizer.AddTarget(ShowOptions);
            View.AddGestureRecognizer(tapRecognizer);
        }

        protected override void ApplyProperties(ComboBox oldView, ComboBox newView)
        {
            _header.Text = newView.Header;
            _value.Text = newView.SelectedItem?.ToString();

            base.ApplyProperties(oldView, newView);
        }

        private void ShowOptions()
        {
            if (ModalController == null)
            {
                // http://sharpmobilecode.com/a-replacement-for-actionsheet-date-picker/
                ModalController = CreateModalEditViewController(View.GetViewController());

                ModalController.OnModalEditSubmitted += new WeakEventHandler(ModalController_OnModalEditSubmitted).Handler;
            }

            PrepareModalControllerValues();

            ModalController.ShowAsModal();
        }

        private ModalPickerViewController CreateModalEditViewController(UIViewController parent)
        {
            var modalPicker = new ModalPickerViewController(_header.Text, parent);
            return modalPicker;
        }

        private void PrepareModalControllerValues()
        {
            _pickerViewModel = new BareUISimplePickerViewModel(ModalController.PickerView)
            {
                ItemsSource = VxView.Items
            };
            ModalController.PickerView.Model = _pickerViewModel;
            ModalController.PickerView.Select(VxView.Items.OfType<object>().ToArray().FindIndex(i => i == VxView.SelectedItem), 0, false);
        }

        private void ModalController_OnModalEditSubmitted(object sender, EventArgs e)
        {
            UpdateValuesFromModalController();
        }

        private void UpdateValuesFromModalController()
        {
            object newItem = VxView.Items.OfType<object>().ElementAt((int)ModalController.PickerView.SelectedRowInComponent(0));

            VxView.SelectedItemChanged?.Invoke(newItem);
        }
    }
}
