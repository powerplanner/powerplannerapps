using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.ComponentModel;
using ToolsPortable;
using InterfacesiOS.Views;
using CoreAnimation;
using System.Collections;
using CoreGraphics;
using BareMvvm.Core;
using BareMvvm.Core.Binding;

namespace InterfacesiOS.Binding
{
    public class BindingHost : BareMvvm.Core.Binding.BindingHost
    {
        public void SetTextFieldTextBinding<TDataContext, T>(UITextField textField, string propertyName, Func<TDataContext, T> getValue, Action<TDataContext, T> setValue, Func<T, string> converter = null, Func<string, T> backConverter = null)
        {
            SetBinding(propertyName, getValue, value => textField.Text = converter != null ? converter(value) : value is string text ? text : null);

            EventHandler handler = new WeakEventHandler<EventArgs>(delegate
            {
                if (DataContext is TDataContext dataContext)
                {
                    T value = backConverter != null ? backConverter(textField.Text) : (T)(object)textField.Text;
                    setValue(dataContext, value);
                }
            }).Handler;

            textField.AddTarget(handler, UIControlEvent.EditingChanged);
            textField.AddTarget(handler, UIControlEvent.EditingDidEnd);
        }

        public void SetTextFieldBinding<TDataContext>(BareUITextField textField, string propertyName, Func<TDataContext, TextField> getValue)
        {
            SetBinding(propertyName, getValue, value => textField.TextField = value);
        }

        public void SetTextViewTextBinding<TDataContext>(UITextView textView, string propertyName, Func<TDataContext, string> getValue, Action<TDataContext, string> setValue)
        {
            SetBinding(propertyName, getValue, value => textView.Text = value);
            textView.Changed += new WeakEventHandler(delegate
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, textView.Text);
                }
            }).Handler;
        }

        public void SetSliderBinding<TDataContext>(UISlider slider, string propertyName, Func<TDataContext, float> getValue, Action<TDataContext, float> setValue = null)
        {
            SetBinding(propertyName, getValue, value => slider.Value = value);
            if (setValue != null)
            {
                slider.ValueChanged += new WeakEventHandler(delegate
                {
                    if (DataContext is TDataContext dataContext)
                    {
                        setValue(dataContext, slider.Value);
                    }
                }).Handler;
            }
        }

        public void SetDateBinding<TDataContext>(BareUIInlineDatePicker datePicker, string propertyName, Func<TDataContext, DateTime?> getValue, Action<TDataContext, DateTime?> setValue)
        {
            SetBinding(propertyName, getValue, value => datePicker.Date = value);
            datePicker.DateChanged += new WeakEventHandler<DateTime?>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, value);
                }
            }).Handler;
        }

        public void SetDateBinding<TDataContext>(BareUIInlineDatePicker datePicker, string propertyName, Func<TDataContext, DateTime> getValue, Action<TDataContext, DateTime> setValue)
        {
            SetBinding(propertyName, getValue, value => datePicker.Date = value);
            datePicker.DateChanged += new WeakEventHandler<DateTime?>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, value.GetValueOrDefault());
                }
            }).Handler;
        }

        public void SetTimeBinding<TDataContext>(UIDatePicker datePicker, string propertyName, Func<TDataContext, TimeSpan> getValue, Action<TDataContext, TimeSpan> setValue)
        {
            SetBinding(propertyName, getValue, value => datePicker.Date = BareUIHelper.DateTimeToNSDate(DateTime.Today.Add(value)));
            datePicker.ValueChanged += new WeakEventHandler(delegate
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, BareUIHelper.NSDateToDateTime(datePicker.Date).TimeOfDay);
                }
            }).Handler;
        }

        public void SetTimeBinding<TDataContext>(BareUIInlineTimePicker timePicker, string propertyName, Func<TDataContext, TimeSpan?> getValue, Action<TDataContext, TimeSpan?> setValue)
        {
            SetBinding(propertyName, getValue, value => timePicker.Time = value);
            timePicker.TimeChanged += new WeakEventHandler<TimeSpan?>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, value);
                }
            }).Handler;
        }

        public void SetTimeBinding<TDataContext>(BareUIInlineTimePicker timePicker, string propertyName, Func<TDataContext, TimeSpan> getValue, Action<TDataContext, TimeSpan> setValue)
        {
            SetBinding(propertyName, getValue, value => timePicker.Time = value);
            timePicker.TimeChanged += new WeakEventHandler<TimeSpan?>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, value.GetValueOrDefault());
                }
            }).Handler;
        }

        public BindingRegistration SetSwitchBinding<TDataContext>(UISwitch switchView, string propertyName, Func<TDataContext, bool> getValue, Action<TDataContext, bool> setValue)
        {
            BindingRegistration registration = SetBinding(propertyName, getValue, value => switchView.On = value);
            switchView.ValueChanged += new WeakEventHandler(delegate
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, switchView.On);
                }
            }).Handler;
            return registration;
        }

        public void SetSelectedColorBinding<TDataContext>(BareUIInlineColorPickerView pickerView, string propertyName, Func<TDataContext, byte[]> getValue, Action<TDataContext, byte[]> setValue)
        {
            SetBinding(propertyName, getValue, value => pickerView.SelectedColor = BareUIHelper.ToCGColor(value));
            pickerView.SelectionChanged += new WeakEventHandler<CGColor>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, BareUIHelper.ToColorBytes(value));
                }
            }).Handler;
        }

        public void SetSelectedColorBinding<TDataContext>(BareUIInlineColorPickerView pickerView, string propertyName, Func<TDataContext, CGColor> getValue, Action<TDataContext, CGColor> setValue)
        {
            SetBinding(propertyName, getValue, value => pickerView.SelectedColor = value);
            pickerView.SelectionChanged += new WeakEventHandler<CGColor>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, value);
                }
            }).Handler;
        }

        public void SetSelectedItemBinding<TDataContext>(BareUIInlinePickerView pickerView, string propertyName, Func<TDataContext, object> getValue, Action<TDataContext, object> setValue)
        {
            SetBinding(propertyName, getValue, value => pickerView.SelectedItem = value);
            pickerView.SelectionChanged += new WeakEventHandler<object>((sender, value) =>
            {
                if (DataContext is TDataContext dataContext)
                {
                    setValue(dataContext, value);
                }
            }).Handler;
        }

        public void SetItemsSourceBinding<TDataContext>(BareUIInlinePickerView pickerView, string propertyName, Func<TDataContext, IEnumerable> getValue)
        {
            SetBinding(propertyName, getValue, value => pickerView.ItemsSource = value);
        }

        public void SetVisibilityBinding<TDataContext, T>(BareUIVisibilityContainer visibilityContainer, string propertyName, Func<TDataContext, T> getValue, bool invert = false)
        {
            SetBinding(propertyName, getValue, value => visibilityContainer.IsVisible = invert ? !ToBoolean(value) : ToBoolean(value));
        }

        public void SetTableViewSourceBinding<TDataContext, T>(UITableView tableView, string propertyName, Func<TDataContext, T> getValue, Func<UITableViewSource> createTableSourceAction)
        {
            SetBinding(propertyName, getValue, value => tableView.Source = value is null ? null : createTableSourceAction());
        }

        public void SetIsEnabledBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, bool> getValue)
        {
            SetBinding(propertyName, getValue, isEnabled =>
            {
                view.UserInteractionEnabled = isEnabled;
                view.Alpha = isEnabled ? 1 : 0.5f;
            });
        }

        public void SetLabelTextBinding<TDataContext, T>(UILabel label, string propertyName, Func<TDataContext, T> getValue, Func<T, string> converter = null)
        {
            SetBinding(propertyName, getValue, value =>
            {
                if (converter != null)
                {
                    label.Text = converter(value);
                }
                else if (value is DayOfWeek dayOfWeek)
                {
                    label.Text = DateTools.ToLocalizedString(dayOfWeek);
                }
                else
                {
                    label.Text = value?.ToString() ?? string.Empty;
                }
            });
        }

        public void SetIsCheckedBinding<TDataContext>(UITableViewCell cell, string propertyName, Func<TDataContext, bool> getValue)
        {
            SetBinding(propertyName, getValue, value => cell.Accessory = value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None);
        }

        public void SetColorBinding<TDataContext>(CAShapeLayer layer, string propertyName, Func<TDataContext, byte[]> getValue)
        {
            SetBinding(propertyName, getValue, value => layer.FillColor = value != null ? BareUIHelper.ToCGColor(value) : null);
        }

        public void SetBackgroundColorBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, byte[]> getValue)
        {
            SetBinding(propertyName, getValue, value => view.BackgroundColor = value != null ? BareUIHelper.ToColor(value) : null);
        }

        public void SetBackgroundColorBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, CGColor> getValue)
        {
            SetBinding(propertyName, getValue, value => view.BackgroundColor = value != null ? new UIColor(value) : null);
        }

        public void SetBackgroundColorBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, UIColor> getValue)
        {
            SetBinding(propertyName, getValue, value => view.BackgroundColor = value);
        }

        public void SetVisibilityBinding<TDataContext>(UIView view, string propertyName, Func<TDataContext, bool> getValue, bool invert = false)
        {
            SetBinding(propertyName, getValue, value => view.Hidden = invert ? value : !value);
        }

        private static bool ToBoolean<T>(T value)
        {
            if (value is bool boolean)
            {
                return boolean;
            }
            if (value is string text)
            {
                return !string.IsNullOrWhiteSpace(text);
            }
            return value != null;
        }

    }
}