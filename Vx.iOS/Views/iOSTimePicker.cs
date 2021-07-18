using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTimePicker : iOSView<Vx.Views.TimePicker, UIView>
    {
        private UILabel _header;
        private UIDatePicker _datePicker;

        public iOSTimePicker()
        {
            _header = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _datePicker = new UIDatePicker
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Mode = UIDatePickerMode.Time,
                PreferredDatePickerStyle = UIDatePickerStyle.Inline
            };
            _datePicker.ValueChanged += View_ValueChanged;

            View.AddSubview(_header);
            View.AddSubview(_datePicker);

            _header.StretchWidth(View);
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[datePicker]->=0-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "datePicker", _datePicker));

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header][datePicker]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", _header,
                "datePicker", _datePicker));
        }

        private void View_ValueChanged(object sender, EventArgs e)
        {
            var newTime = BareUIHelper.NSDateToDateTime(_datePicker.Date).TimeOfDay;

            if (newTime != VxView.Value && VxView.ValueChanged != null)
            {
                VxView.ValueChanged(newTime);
            }
        }

        protected override void ApplyProperties(TimePicker oldView, TimePicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;
            _datePicker.Date = BareUIHelper.DateTimeToNSDate(DateTime.Today.Add(newView.Value));
        }
    }
}
