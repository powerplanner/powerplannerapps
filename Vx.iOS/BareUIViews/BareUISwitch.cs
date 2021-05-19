using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUISwitch : UIControl
    {
        public event EventHandler ToggledViaHeader;
        private UILabel _label;

        public UISwitch Switch { get; private set; }

        public BareUISwitch()
        {
            _label = new UILabel()
            {
                Lines = 1,
                Font = UIFont.PreferredBody
            };
            this.Add(_label);

            Switch = new UISwitch();
            this.Add(Switch);

            // Idk why, but on the add task page, if the keyboard is up, this doesn't get hit
            // even though on the inline edit controls the same code works. I investigated for 20 mins
            // and couldn't figure it out. It works on the edit schedule times page for some reason.
            this.TouchUpInside += BareUISwitch_TouchUpInside;
        }

        private void BareUISwitch_TouchUpInside(object sender, EventArgs e)
        {
            Switch.On = !Switch.On;
            ToggledViaHeader?.Invoke(this, null);
        }

        public override void LayoutSubviews()
        {
            var switchSize = Switch.SizeThatFits(Frame.Size);
            Switch.Frame = new CoreGraphics.CGRect(
                x: Frame.Width - switchSize.Width - 16,
                y: (Frame.Height - switchSize.Height) / 2f,
                width: switchSize.Width,
                height: switchSize.Height);

            _label.Frame = new CoreGraphics.CGRect(
                x: 16,
                y: 0,
                width: Switch.Frame.X - 32,
                height: Frame.Height);
        }

        public string Header
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }
    }
}