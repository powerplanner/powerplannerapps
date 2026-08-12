using System;
using CoreGraphics;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSSwitch : iOSView<Switch, ManualLayoutControl>
    {
        private UILabel _label;
        private UISwitch _switch;

        public iOSSwitch()
        {
            _label = new UILabel()
            {
                Lines = 1,
                Font = UIFont.PreferredBody
            };
            View.Add(_label);

            _switch = new UISwitch();
            View.Add(_switch);

            // Idk why, but on the add task page, if the keyboard is up, this doesn't get hit
            // even though on the inline edit controls the same code works. I investigated for 20 mins
            // and couldn't figure it out. It works on the edit schedule times page for some reason.
            View.TouchUpInside += View_TouchUpInside;

            View.LayoutAction = LayoutSubviews;
            View.MeasureAction = Measure;

            _switch.ValueChanged += _switch_ValueChanged;
        }

        private CGSize Measure(CGSize availableSize)
        {
            var switchSize = _switch.SizeThatFits(availableSize);
            var labelSize = _label.SizeThatFits(new CGSize(Math.Max(0, availableSize.Width - switchSize.Width - 16), availableSize.Height));
            return new CGSize(labelSize.Width + 16 + switchSize.Width, Math.Max(labelSize.Height, switchSize.Height));
        }

        private void LayoutSubviews()
        {
            var switchSize = _switch.SizeThatFits(View.Bounds.Size);
            _switch.Frame = new CGRect(View.Bounds.Width - switchSize.Width, (View.Bounds.Height - switchSize.Height) / 2, switchSize.Width, switchSize.Height);
            _label.Frame = new CGRect(0, 0, Math.Max(0, View.Bounds.Width - switchSize.Width - 16), View.Bounds.Height);
        }

        private void _switch_ValueChanged(object sender, EventArgs e)
        {
            VxView.IsOn?.ValueChanged?.Invoke(_switch.On);
        }

        private void View_TouchUpInside(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            _switch.On = !_switch.On;

            VxView.IsOn?.ValueChanged?.Invoke(_switch.On);
        }

        protected override void ApplyProperties(Switch oldView, Switch newView)
        {
            base.ApplyProperties(oldView, newView);

            _label.Text = newView.Title;
            _switch.On = newView.IsOn?.Value ?? false;
            _switch.Enabled = newView.IsEnabled;
        }
    }
}
