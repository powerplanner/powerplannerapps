using System;
using InterfacesiOS.Helpers;
using UIKit;
using Vx.Extensions;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTimePicker : iOSView<Vx.Views.TimePicker, UIView>
    {
        private UILabel _header;
        private UIControl _valueContainer;
        private UILabel _value;

        public iOSTimePicker()
        {
            _header = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubview(_header);

            _header.StretchWidth(View);

            _valueContainer = new UIControl
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                BackgroundColor = UIColorCompat.TertiarySystemFillColor
            };
            _valueContainer.Layer.CornerRadius = 10;
            _valueContainer.TouchUpInside += _valueContainer_TouchUpInside;

            _value = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 1
            };
            _valueContainer.Add(_value);
            _value.StretchWidthAndHeight(_valueContainer, 10, 0, 10, 0);

            View.AddSubview(_valueContainer);

            _valueContainer.StretchWidth(View);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header]-4-[valueContainer(36)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", _header,
                "valueContainer", _valueContainer));
        }

        private async void _valueContainer_TouchUpInside(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            var minTime = VxView is EndTimePicker endTimePicker ? endTimePicker.StartTime : new TimeSpan();
            var resp = await new Controllers.ImprovedModalTimePickerViewController(_valueContainer, VxView.Value?.Value ?? new TimeSpan(DateTime.Today.Hour, 0, 0), minTime).ShowAsync();

            if (resp != null)
            {
                var newTime = resp.Value;

                if (_value != null)
                {
                    UpdateText(newTime);
                }

                if (VxView.Value != null && newTime != VxView.Value.Value)
                {
                    VxView.Value.ValueChanged?.Invoke(newTime);
                }
            }
        }

        protected override void ApplyProperties(TimePicker oldView, TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;
            _valueContainer.Alpha = newView.IsEnabled ? 1f : 0.5f;

            if (newView.Value?.Value != null)
            {
                UpdateText(newView.Value.Value);
            }
        }

        private void UpdateText(TimeSpan timeSpan)
        {
            _value.Text = DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(timeSpan));
        }
    }
}
