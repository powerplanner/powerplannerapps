using System;
using Foundation;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSSwitch : iOSView<Switch, UIControl>
    {
        private UILabel _label;
        private UISwitch _switch;

        public iOSSwitch()
        {
            _label = new UILabel()
            {
                Lines = 1,
                Font = UIFont.PreferredBody,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_label);
            _label.StretchHeight(View);

            _switch = new UISwitch() { TranslatesAutoresizingMaskIntoConstraints = false };
            View.Add(_switch);
            _switch.StretchHeight(View);

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
            if (VxView.IsOn != null)
            {
                VxView.IsOn.Value = _switch.On;
            }
        }

        private void View_TouchUpInside(object sender, EventArgs e)
        {
            _switch.On = !_switch.On;

            if (VxView.IsOn != null)
            {
                VxView.IsOn.Value = _switch.On;
            }
        }

        protected override void ApplyProperties(Switch oldView, Switch newView)
        {
            base.ApplyProperties(oldView, newView);

            _label.Text = newView.Title;

            if (newView.IsOn != null)
            {
                _switch.On = newView.IsOn.Value;
            }

            _switch.Enabled = newView.IsEnabled;
        }
    }
}
