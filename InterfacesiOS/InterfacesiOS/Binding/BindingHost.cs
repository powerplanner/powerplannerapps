using System;
using System.Collections.Generic;
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
        public void SetTextFieldTextBinding(UITextField textField, string propertyPath, Func<object, string> converter = null, Func<string, object> backConverter = null)
        {
            var registration = SetBinding(propertyPath, sourceValue =>
            {
                string text;

                if (converter != null)
                {
                    text = converter(sourceValue);
                }
                else
                {
                    text = sourceValue as string;
                }

                textField.Text = text;
            });

            EventHandler handler = new WeakEventHandler<EventArgs>(delegate
            {
                object value;

                if (backConverter != null)
                {
                    value = backConverter(textField.Text);
                }
                else
                {
                    value = textField.Text;
                }

                registration.SetSourceValue(value);

            }).Handler;

            // Note that need to listen to EditingDidEnd so that autosuggest corrections are consumed: https://github.com/MvvmCross/MvvmCross/pull/2682
            textField.AddTarget(handler, UIControlEvent.EditingChanged);
            textField.AddTarget(handler, UIControlEvent.EditingDidEnd);
        }

        /// <summary>
        /// Binds two-way to a <see cref="TextField"/> value.
        /// </summary>
        /// <param name="textField"></param>
        /// <param name="propertyPath"></param>
        public void SetTextFieldBinding(BareUITextField textField, string propertyPath)
        {
            SetBinding(propertyPath, (TextField sourceTextField) =>
            {
                textField.TextField = sourceTextField;
            });
        }

        public void SetTextFieldTextBinding<T>(UITextField textField, string propertyPath, Func<T, string> converter, Func<string, T> backConverter)
        {
            SetTextFieldTextBinding(textField, propertyPath, converter: (o) =>
            {
                if (o is T)
                {
                    return converter((T)o);
                }
                throw new NotImplementedException();
            }, backConverter: (text) =>
            {
                return (object)backConverter(text);
            });
        }

        public void SetTextViewTextBinding(UITextView textView, string propertyPath)
        {
            var registration = SetBinding<string>(propertyPath, txt =>
            {
                textView.Text = txt;
            });

            textView.Changed += new WeakEventHandler(delegate
            {
                registration.SetSourceValue(textView.Text);
            }).Handler;
        }

        public void SetSliderBinding(UISlider slider, string propertyPath, bool twoWay = false)
        {
            var registration = SetBinding(propertyPath, value =>
            {
                float val = float.Parse(value.ToString());
                slider.Value = val;
            });

            if (twoWay)
            {
                slider.ValueChanged += delegate
                {
                    registration.SetSourceValue(slider.Value);
                };
            }
        }

        public void SetDateBinding(BareUIInlineDatePicker datePicker, string propertyPath)
        {
            var reg = SetBinding(propertyPath, value =>
            {
                datePicker.GetType().GetProperty(nameof(datePicker.Date)).SetValue(datePicker, value);
            });

            datePicker.DateChanged += new WeakEventHandler<DateTime?>((sender, date) =>
            {
                var objAndProp = reg.GetSourceProperty();
                if (objAndProp != null)
                {
                    var prop = objAndProp.PropertyInfo;

                    if (prop.PropertyType == typeof(DateTime?))
                    {
                        reg.SetSourceValue(date, objAndProp);
                    }
                    else
                    {
                        reg.SetSourceValue(date.GetValueOrDefault(), objAndProp);
                    }
                }
            }).Handler;
        }

        public void SetTimeBinding(UIDatePicker datePicker, string propertyPath)
        {
            var reg = SetBinding<TimeSpan>(propertyPath, value =>
            {
                datePicker.Date = BareUIHelper.DateTimeToNSDate(DateTime.Today.Add(value));
            });

            datePicker.ValueChanged += delegate
            {
                reg.SetSourceValue(BareUIHelper.NSDateToDateTime(datePicker.Date).TimeOfDay);
            };
        }

        public void SetTimeBinding(BareUIInlineTimePicker timePicker, string propertyPath)
        {
            var reg = SetBinding(propertyPath, value =>
            {
                timePicker.GetType().GetProperty(nameof(timePicker.Time)).SetValue(timePicker, value);
            });

            timePicker.TimeChanged += new WeakEventHandler<TimeSpan?>((sender, time) =>
            {
                var objAndProp = reg.GetSourceProperty();
                if (objAndProp != null)
                {
                    var prop = objAndProp.PropertyInfo;

                    if (prop.PropertyType == typeof(TimeSpan?))
                    {
                        reg.SetSourceValue(time, objAndProp);
                    }
                    else
                    {
                        reg.SetSourceValue(time.GetValueOrDefault(), objAndProp);
                    }
                }
            }).Handler;
        }

        private List<EventHandler<EventArgs>> _toggledViaHeaderHandlers = new List<EventHandler<EventArgs>>();
        public void SetSwitchBinding(BareUISwitch switchView, string propertyPath)
        {
            var reg = SetSwitchBinding(switchView.Switch, propertyPath);

            var handler = new EventHandler<EventArgs>(delegate
            {
                reg.SetSourceValue(switchView.Switch.On);
            });

            // In order to avoid disposing, we need to store the handler.
            // Using a weak event handler causes it to get disposed.
            // Using a strong event handler causes it to not get let go.
            _toggledViaHeaderHandlers.Add(handler);

            switchView.ToggledViaHeader += new WeakEventHandler(handler).Handler;

        }

        public BindingRegistration SetSwitchBinding(UISwitch switchView, string propertyPath)
        {
            var reg = SetBinding<bool>(propertyPath, value =>
            {
                switchView.On = value;
            });

            switchView.ValueChanged += new WeakEventHandler(delegate
            {
                reg.SetSourceValue(switchView.On);
            }).Handler;

            return reg;
        }

        public void SetSelectedColorBinding(BareUIInlineColorPickerView pickerView, string propertyPath)
        {
            var reg = SetBinding(propertyPath, value =>
            {
                if (value is CGColor color)
                {
                    pickerView.SelectedColor = color;
                }
                else if (value is byte[] bytes)
                {
                    pickerView.SelectedColor = BareUIHelper.ToCGColor(bytes);
                }
            });

            pickerView.SelectionChanged += new WeakEventHandler<CGColor>((sender, color) =>
            {
                var objAndProperty = reg.GetSourceProperty();
                if (objAndProperty != null)
                {
                    var property = objAndProperty.PropertyInfo;
                    if (property.PropertyType == typeof(byte[]))
                    {
                        reg.SetSourceValue(BareUIHelper.ToColorBytes(color), objAndProperty);
                    }
                    else if (property.PropertyType == typeof(CGColor))
                    {
                        reg.SetSourceValue(color, objAndProperty);
                    }
                }
            }).Handler;
        }

        public void SetSelectedItemBinding(BareUIInlinePickerView pickerView, string propertyPath)
        {
            var reg = SetBinding(propertyPath, selectedItem =>
            {
                pickerView.SelectedItem = selectedItem;
            });

            pickerView.SelectionChanged += new WeakEventHandler<object>((sender, item) =>
            {
                reg.SetSourceValue(item);
            }).Handler;
        }

        public void SetItemsSourceBinding(BareUIInlinePickerView pickerView, string propertyPath)
        {
            SetBinding<IEnumerable>(propertyPath, itemsSource =>
            {
                pickerView.ItemsSource = itemsSource;
            });
        }

        public void SetVisibilityBinding(BareUIVisibilityContainer visibilityContainer, string propertyPath, bool invert = false)
        {
            SetBinding(propertyPath, value =>
            {
                bool boolean;
                if (value is bool b)
                {
                    boolean = b;
                }
                else if (value is string s)
                {
                    boolean = !string.IsNullOrWhiteSpace(s);
                }
                else
                {
                    boolean = value != null;
                }

                if (invert)
                {
                    boolean = !boolean;
                }

                visibilityContainer.IsVisible = boolean;
            });
        }

        public void SetTableViewSourceBinding(UITableView tableView, string propertyPath, Func<UITableViewSource> createTableSourceAction)
        {
            SetBinding(propertyPath, value =>
            {
                if (DataContext.GetType().GetProperty(propertyPath).GetValue(DataContext) == null)
                {
                    tableView.Source = null;
                }
                else
                {
                    tableView.Source = createTableSourceAction();
                }
            });
        }

        public void SetIsEnabledBinding(UIView view, string propertyPath)
        {
            SetBinding<bool>(propertyPath, (isEnabled) =>
            {
                if (isEnabled)
                {
                    view.UserInteractionEnabled = true;
                    view.Alpha = 1;
                }
                else
                {
                    view.UserInteractionEnabled = false;
                    view.Alpha = 0.5f;
                }
            });
        }

        public void SetLabelTextBinding(UILabel label, string propertyPath, Func<object, string> converter = null)
        {
            SetBinding(propertyPath, value =>
            {
                string valueText;
                if (converter != null)
                {
                    valueText = converter.Invoke(value);
                }
                else if (value is string valueStr)
                {
                    valueText = valueStr;
                }
                else if (value is DayOfWeek)
                {
                    valueText = DateTools.ToLocalizedString((DayOfWeek)value);
                }
                else if (value == null)
                {
                    valueText = "";
                }
                else
                {
                    valueText = value.ToString();
                }

                label.Text = valueText;
            });
        }

        public void SetLabelTextBinding<T>(UILabel label, string propertyPath, Func<T, string> converter = null)
        {
            SetLabelTextBinding(label, propertyPath, obj =>
            {
                return converter(obj is T ? (T)obj : default(T));
            });
        }

        /// <summary>
        /// Only one-way binding
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="propertyPath"></param>
        public void SetIsCheckedBinding(UITableViewCell cell, string propertyPath)
        {
            SetBinding<bool>(propertyPath, isChecked =>
            {
                cell.Accessory = isChecked ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });
        }

        public void SetColorBinding(CAShapeLayer layer, string propertyPath)
        {
            SetBinding<byte[]>(propertyPath, colorArray =>
            {
                if (colorArray != null)
                {
                    layer.FillColor = BareUIHelper.ToCGColor(colorArray);
                }
                else
                {
                    layer.FillColor = null;
                }
            });
        }

        public void SetBackgroundColorBinding(UIView view, string propertyPath)
        {
            SetBinding(propertyPath, value =>
            {
                UIColor colorValue = null;

                if (value is byte[] colorArray)
                {
                    colorValue = BareUIHelper.ToColor(colorArray);
                }
                else if (value is CGColor cgColor)
                {
                    colorValue = new UIColor(cgColor);
                }
                else if (value is UIColor uiColor)
                {
                    colorValue = uiColor;
                }

                view.BackgroundColor = colorValue;
            });
        }

        public void SetVisibilityBinding(UIView view, string propertyPath, bool invert = false)
        {
            SetBinding<bool>(propertyPath, isVisible =>
            {
                if (invert)
                {
                    isVisible = !isVisible;
                }

                view.Hidden = !isVisible;
            });
        }
    }
}