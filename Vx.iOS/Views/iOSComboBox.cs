using System;
using System.Linq;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using ToolsPortable;
using UIKit;
using Vx.iOS.BareUIViews;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSComboBox : iOSView<Vx.Views.ComboBox, UIView>
    {
        private UILabel _header;
        private UIView _valueContainer;
        private UILabel _value;
        private INativeComponent _valueTemplated;

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

            if (newView.ItemTemplate != null)
            {
                if (_value != null)
                {
                    _value.RemoveFromSuperview();
                    _value = null;
                }

                if (DataTemplateHelper.ProcessAndIsNewComponent(newView.SelectedItem?.Value, newView.ItemTemplate, _valueTemplated, out VxComponent newComponent))
                {
                    _valueTemplated = newComponent.Render();
                    (_valueTemplated as UIView).TranslatesAutoresizingMaskIntoConstraints = false;
                    _valueContainer.Add(_valueTemplated as UIView);
                    (_valueTemplated as UIView).StretchWidthAndHeight(_valueContainer, 10, 10, 10, 10);
                }
            }
            else
            {
                if (_valueTemplated != null)
                {
                    (_valueTemplated as UIView).RemoveFromSuperview();
                    _valueTemplated = null;
                }

                if (_value == null)
                {
                    _value = new UILabel
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Lines = 1
                    };
                    _valueContainer.Add(_value);
                    _value.StretchWidthAndHeight(_valueContainer, 10, 0, 10, 0);
                }

                _value.Text = newView.SelectedItem?.Value?.ToString();
            }

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
            if (VxView.ItemTemplate != null)
            {
                _pickerViewModel = new BareUICustomPickerViewModel(ModalController.PickerView)
                {
                    ItemsSource = VxView.Items,
                    ItemToViewConverter = ConvertItemToView
                };
            }
            else
            {
                _pickerViewModel = new BareUISimplePickerViewModel(ModalController.PickerView)
                {
                    ItemsSource = VxView.Items
                };
            }
            ModalController.PickerView.Model = _pickerViewModel;
            ModalController.PickerView.Select(VxView.Items.OfType<object>().ToArray().FindIndex(i => i == VxView.SelectedItem?.Value), 0, false);
        }

        private UIView ConvertItemToView(object item, UIView recycledView)
        {
            if (DataTemplateHelper.ProcessAndIsNewComponent(item, VxView.ItemTemplate, (recycledView as BareUICenteredView)?.Content as INativeComponent, out VxComponent newComponent))
            {
                return new BareUICenteredView(newComponent.Render());
            }

            // Otherwise recycled and was already updated
            return recycledView;
        }

        private void ModalController_OnModalEditSubmitted(object sender, EventArgs e)
        {
            UpdateValuesFromModalController();
        }

        private void UpdateValuesFromModalController()
        {
            object newItem = VxView.Items.OfType<object>().ElementAt((int)ModalController.PickerView.SelectedRowInComponent(0));

            VxView.SelectedItem?.ValueChanged?.Invoke(newItem);
        }
    }
}
