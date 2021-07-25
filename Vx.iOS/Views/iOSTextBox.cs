using CoreAnimation;
using CoreGraphics;
using Foundation;
using InterfacesiOS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTextBox : iOSView<Vx.Views.TextBox, UIRoundedTextFieldWithHeader>
    {
        public iOSTextBox()
        {
            View.TextChanged += View_TextChanged;
        }

        private void View_TextChanged(object sender, string e)
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

            View.Header = newView.Header;
            View.Placeholder = newView.PlaceholderText;
        }
    }

    public class UIRoundedTextField : UITextField
    {
        public UIRoundedTextField()
        {
            BackgroundColor = UIColorCompat.TertiarySystemFillColor;
            ClipsToBounds = true;
            Layer.CornerRadius = 10;

            EditingDidBegin += UIRoundedTextField_EditingDidBegin;
            EditingDidEnd += UIRoundedTextField_EditingDidEnd;
            EditingDidEndOnExit += UIRoundedTextField_EditingDidEndOnExit;
        }

        private void UIRoundedTextField_EditingDidEndOnExit(object sender, EventArgs e)
        {
            UpdateFocus(false);
        }

        private void UIRoundedTextField_EditingDidEnd(object sender, EventArgs e)
        {
            UpdateFocus(false);
        }

        private void UIRoundedTextField_EditingDidBegin(object sender, EventArgs e)
        {
            UpdateFocus(true);
        }

        public override CGRect TextRect(CGRect forBounds)
        {
            return base.TextRect(forBounds).Inset(10, 0);
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            return base.EditingRect(forBounds).Inset(10, 0);
        }

        private void UpdateFocus(bool focused)
        {
            if (focused)
            {
                Layer.BorderColor = Theme.Current.AccentColor.ToUI().CGColor;
                Layer.BorderWidth = 2;
            }
            else
            {
                Layer.BorderWidth = 0;
            }
        }
    }

    public class UIRoundedTextFieldWithHeader : UIView
    {
        public event EventHandler<string> TextChanged;

        private UILabel _header;
        private UIRoundedTextField _textField;

        public UIRoundedTextFieldWithHeader()
        {
            _header = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _textField = new UIRoundedTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _textField.EditingChanged += TextUpdated;
            _textField.EditingDidEnd += TextUpdated;
            _textField.EditingDidEndOnExit += TextUpdated;

            Add(_header);
            Add(_textField);

            _header.StretchWidth(this);
            _textField.StretchWidth(this);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header]-4-[textField(36)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", _header,
                "textField", _textField));
        }

        private void TextUpdated(object sender, EventArgs e)
        {
            if (_text != _textField.Text)
            {
                _text = _textField.Text;
                TextChanged?.Invoke(this, _textField.Text);
            }
        }

        public string Header
        {
            get => _header.Text;
            set => _header.Text = value;
        }

        public string Placeholder
        {
            get => _textField.Placeholder;
            set => _textField.Placeholder = value;
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _textField.Text = value;
            }
        }

        public UIKeyboardType KeyboardType
        {
            get => _textField.KeyboardType;
            set => _textField.KeyboardType = value;
        }
    }
}