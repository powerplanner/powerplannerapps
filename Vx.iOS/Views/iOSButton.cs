using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSButton : iOSView<Button, UIButton>
    {
        public iOSButton() : base(new UIButton(UIButtonType.System))
        {
            View.TouchUpInside += View_TouchUpInside;
        }

        private void View_TouchUpInside(object sender, EventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Button oldView, Button newView)
        {
            base.ApplyProperties(oldView, newView);

            View.SetTitle(newView.Text, UIControlState.Normal);
        }
    }
}