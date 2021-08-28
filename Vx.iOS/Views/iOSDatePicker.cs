using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSDatePicker : iOSView<DatePicker, UIView>
    {
        private UILabel _header;
        private UIDatePicker _datePicker;

        public iOSDatePicker()
        {
            _header = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _datePicker = new UIDatePicker
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Mode = UIDatePickerMode.Date
            };
            if (InterfacesiOS.Helpers.SdkSupportHelper.IsUIDatePickerCompactStyleSupported)
            {
                _datePicker.PreferredDatePickerStyle = UIDatePickerStyle.Compact;
            }
            _datePicker.ValueChanged += View_ValueChanged;

            View.AddSubview(_header);
            View.AddSubview(_datePicker);

            _header.StretchWidth(View);
            _datePicker.StretchWidth(View);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header][datePicker]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", _header,
                "datePicker", _datePicker));
        }

        private void View_ValueChanged(object sender, EventArgs e)
        {
            var newDate = BareUIHelper.NSDateToDateTime(_datePicker.Date).Date;

            if (VxView.Value != null && newDate != VxView.Value.Value)
            {
                VxView.Value.ValueChanged?.Invoke(newDate);
            }
        }

        protected override void ApplyProperties(DatePicker oldView, DatePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;

            if (newView.Value?.Value != null)
            {
                _datePicker.Date = BareUIHelper.DateTimeToNSDate(newView.Value.Value.Value.Date);
            }
        }
    }
}
