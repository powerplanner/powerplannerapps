using CoreAnimation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTextBox : iOSView<Vx.Views.TextBox, UITextFieldWithUnderline>
    {
        public iOSTextBox()
        {
            View.EditingChanged += View_EditingDidEnd;
            View.EditingDidEnd += View_EditingDidEnd;
            View.EditingDidEndOnExit += View_EditingDidEnd;
            View.TextColor = Theme.Current.ForegroundColor.ToUI();
            View.SetHeight(34);

        }

        private void View_EditingDidEnd(object sender, EventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = View.Text;
            }
        }

        protected override void ApplyProperties(TextBox oldView, TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Text != null)
            {
                View.Text = newView.Text.Value;
            }
        }
    }

    public class UITextFieldWithUnderline : UITextField
    {
        private CALayer _bottomLine;

        public UITextFieldWithUnderline()
        {
            _bottomLine = new CALayer();
            _bottomLine.BackgroundColor = Theme.Current.SubtleForegroundColor.ToUI().CGColor;
            BorderStyle = UITextBorderStyle.None;
            Layer.AddSublayer(_bottomLine);

            base.EditingDidBegin += UITextFieldWithUnderline_FocusChanged;
            base.EditingDidEnd += UITextFieldWithUnderline_FocusChanged;
            base.EditingDidEndOnExit += UITextFieldWithUnderline_FocusChanged;
        }

        private void UITextFieldWithUnderline_FocusChanged(object sender, EventArgs e)
        {
            if (base.IsEditing)
            {
                _bottomLine.BackgroundColor = Theme.Current.AccentColor.ToUI().CGColor;
            }
            else
            {
                _bottomLine.BackgroundColor = Theme.Current.SubtleForegroundColor.ToUI().CGColor;
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _bottomLine.Frame = new CoreGraphics.CGRect(0, Frame.Height - 1, Frame.Width, 1);
        }

        public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
        {
            base.DidUpdateFocus(context, coordinator);

            if (Focused)
            {
                _bottomLine.BackgroundColor = Theme.Current.AccentColor.ToUI().CGColor;
            }
            else
            {
                _bottomLine.BackgroundColor = Theme.Current.SubtleForegroundColor.ToUI().CGColor;
            }
        }
    }
}