using System;
using System.Threading.Tasks;
using InterfacesiOS.Helpers;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSDatePicker : iOSView<DatePicker, UIView>
    {
        private UILabel _header;
        private UIControl _valueContainer;
        private UILabel _value;

        public iOSDatePicker()
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
            var resp = await new Controllers.ImprovedModalDatePickerViewController(_valueContainer, VxView.Value?.Value ?? DateTime.Today).ShowAsync();

            if (resp != null)
            {
                var newDate = resp.Value;

                if (_value != null)
                {
                    _value.Text = newDate.ToShortDateString();
                }

                if (VxView.Value != null && newDate != VxView.Value.Value)
                {
                    VxView.Value.ValueChanged?.Invoke(newDate);
                }
            }
        }

        protected override void ApplyProperties(DatePicker oldView, DatePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;

            if (newView.Value?.Value != null)
            {
                _value.Text = newView.Value.Value.Value.ToShortDateString();
            }
        }
    }
}
