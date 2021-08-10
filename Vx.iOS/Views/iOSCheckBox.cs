using System;
using Foundation;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSCheckBox : iOSView<CheckBox, UIControl>
    {
        private UILabel _label;
        private UISwitch _switch;

        public iOSCheckBox()
        {
            _label = new UILabel()
            {
                Lines = 1,
                Font = UIFont.PreferredBody,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_label);
            _label.StretchHeight(View, 5, 5); // We pad to match how Windows checkboxes are padded

            _switch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false };
            View.Add(_switch);
            _switch.StretchHeight(View, 5, 5);

            // Idk why, but on the add task page, if the keyboard is up, this doesn't get hit
            // even though on the inline edit controls the same code works. I investigated for 20 mins
            // and couldn't figure it out. It works on the edit schedule times page for some reason.
            View.TouchUpInside += View_TouchUpInside;

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[label]-16-[switch]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null,
                new NSDictionary(
                    "label", _label,
                    "switch", _switch
                    )));

            _switch.ValueChanged += _switch_ValueChanged;
        }

        private void _switch_ValueChanged(object sender, EventArgs e)
        {
            if (VxView.IsChecked != null && VxView.IsChecked.Value != _switch.On)
            {
                VxView.IsChecked.ValueChanged?.Invoke(_switch.On);
            }
        }

        private void View_TouchUpInside(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            _switch.On = !_switch.On;

            if (VxView.IsChecked != null && VxView.IsChecked.Value != _switch.On)
            {
                VxView.IsChecked.ValueChanged?.Invoke(_switch.On);
            }
        }

        protected override void ApplyProperties(CheckBox oldView, CheckBox newView)
        {
            base.ApplyProperties(oldView, newView);

            _label.Text = newView.Text;

            _switch.On = newView.IsChecked?.Value ?? false;

            _switch.Enabled = newView.IsEnabled;
        }
    }
}
