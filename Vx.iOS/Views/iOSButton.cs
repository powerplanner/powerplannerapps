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
        public iOSButton() : this(CreateRoundedButton(Theme.Current.AccentColor.ToUI(), InterfacesiOS.Helpers.UIColorCompat.TertiarySystemFillColor)) { }

        protected iOSButton(UIButton button) : base(button)
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
            View.Enabled = newView.IsEnabled;
        }

        protected static UIButton CreateRoundedButton(UIColor foregroundColor, UIColor backgroundColor)
        {
            var button = new UIButton(UIButtonType.System);
            button.SetTitleColor(foregroundColor, UIControlState.Normal);
            button.BackgroundColor = backgroundColor;
            button.ContentEdgeInsets = new UIEdgeInsets()
            {
                Left = 8,
                Right = 8,
                Top = 8,
                Bottom = 8
            };
            button.Layer.CornerRadius = 10;

            return button;
        }
    }

    public class iOSAccentButton : iOSButton
    {
        public iOSAccentButton() : base(CreateRoundedButton(UIColor.White, Theme.Current.AccentColor.ToUI())) { }
    }

    public class iOSTextButton : iOSButton
    {
        public iOSTextButton() : base(new UIButton(UIButtonType.System)) { }
    }
}