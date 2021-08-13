using CoreAnimation;
using CoreGraphics;
using Foundation;
using InterfacesiOS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSTextBox : iOSView<Vx.Views.TextBox, UIRoundedTextFieldWithHeader>
    {
        public iOSTextBox()
        {
            View.TextChanged += View_TextChanged;
            View.FocusChanged += View_FocusChanged;
        }

        private void View_FocusChanged(object sender, bool e)
        {
            VxView.HasFocusChanged?.Invoke(e);
        }

        private void View_TextChanged(object sender, string e)
        {
            VxView.Text?.ValueChanged?.Invoke(View.Text);
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
            View.ValidationState = newView.ValidationState;
            View.Enabled = newView.IsEnabled;

            if (newView is PasswordBox && oldView == null)
            {
                // Only need to set this once
                View.SecureTextEntry = true;
            }
            if (oldView == null || oldView.InputScope != newView.InputScope)
            {
                switch (newView.InputScope)
                {
                    case InputScope.Email:
                        View.KeyboardType = UIKeyboardType.EmailAddress;
                        View.AutocorrectionType = UITextAutocorrectionType.No;
                        View.AutocapitalizationType = UITextAutocapitalizationType.None;
                        break;

                    case InputScope.Username:
                        View.KeyboardType = UIKeyboardType.ASCIICapable;
                        View.AutocorrectionType = UITextAutocorrectionType.No;
                        View.AutocapitalizationType = UITextAutocapitalizationType.None;
                        break;

                    case InputScope.Normal:
                        View.KeyboardType = UIKeyboardType.Default;
                        View.AutocorrectionType = UITextAutocorrectionType.Yes;
                        View.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
                        break;
                }
            }

            if (oldView == null && newView.AutoFocus)
            {
                View.BecomeFirstResponder();
            }
        }
    }

    public class UIRoundedTextField : UITextField
    {
        public event EventHandler<bool> FocusChanged;

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

            FocusChanged?.Invoke(this, focused);
        }
    }

    public class UIRoundedTextFieldWithHeader : UIView
    {
        public event EventHandler<string> TextChanged;
        public event EventHandler<bool> FocusChanged;

        private UILabel _header;
        private UIRoundedTextField _textField;
        private UILabel _errorSymbol;
        private UILabel _errorMessage;
        private InterfacesiOS.Views.BareUIVisibilityContainer _errorMessageContainer;

        public UIRoundedTextFieldWithHeader()
        {
            var headerContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                _header = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                _errorSymbol = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.FromName("Material Icons Outlined", UIFont.PreferredBody.PointSize),
                    Alpha = 0
                };

                headerContainer.Add(_header);
                headerContainer.Add(_errorSymbol);

                _header.StretchHeight(headerContainer);
                _errorSymbol.StretchHeight(headerContainer);

                headerContainer.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[header]->=0-[errorSymbol]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "header", _header,
                    "errorSymbol", _errorSymbol));
            }

            _textField = new UIRoundedTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _textField.EditingChanged += TextUpdated;
            _textField.EditingDidEnd += TextUpdated;
            _textField.EditingDidEndOnExit += TextUpdated;
            _textField.FocusChanged += _textField_FocusChanged;

            _errorMessageContainer = new InterfacesiOS.Views.BareUIVisibilityContainer
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                IsVisible = false
            };
            {
                _errorMessage = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextColor = UIColor.Red,
                    Font = UIFont.PreferredCaption1,
                    Lines = 0
                };
                _errorMessageContainer.Child = _errorMessage.WrapInPadding(top: 4);
            }

            Add(headerContainer);
            Add(_textField);
            Add(_errorMessageContainer);

            headerContainer.StretchWidth(this);
            _textField.StretchWidth(this);
            _errorMessageContainer.StretchWidth(this);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header]-4-[textField(36)][errorMessage]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", headerContainer,
                "textField", _textField,
                "errorMessage", _errorMessageContainer));
        }

        public override bool BecomeFirstResponder()
        {
            return _textField.BecomeFirstResponder();
        }

        private void _textField_FocusChanged(object sender, bool e)
        {
            FocusChanged?.Invoke(this, e);
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

        public UITextAutocapitalizationType AutocapitalizationType
        {
            get => _textField.AutocapitalizationType;
            set => _textField.AutocapitalizationType = value;
        }

        public UITextAutocorrectionType AutocorrectionType
        {
            get => _textField.AutocorrectionType;
            set => _textField.AutocorrectionType = value;
        }

        public bool SecureTextEntry
        {
            get => _textField.SecureTextEntry;
            set => _textField.SecureTextEntry = value;
        }

        private InputValidationState _validationState;
        public InputValidationState ValidationState
        {
            get => _validationState;
            set
            {
                _validationState = value;

                if (value != null && value.ErrorMessage != null)
                {
                    _errorSymbol.Alpha = 1;
                    _errorSymbol.Text = MaterialDesign.MaterialDesignIcons.ErrorOutline;
                    _errorSymbol.TextColor = UIColor.SystemRedColor;
                    _errorMessage.Text = value.ErrorMessage;
                    _errorMessageContainer.IsVisible = true;
                }
                else if (value == InputValidationState.Valid)
                {
                    _errorSymbol.Alpha = 1;
                    _errorSymbol.Text = MaterialDesign.MaterialDesignIcons.CheckCircleOutline;
                    _errorSymbol.TextColor = UIColor.SystemGreenColor;
                    _errorMessageContainer.IsVisible = false;
                }
                else
                {
                    _errorSymbol.Alpha = 0;
                    _errorMessageContainer.IsVisible = false;
                }
            }
        }

        public bool Enabled
        {
            get => _textField.Enabled;
            set
            {
                _textField.Enabled = value;
                Alpha = value ? 1.0f : 0.5f;
            }
        }
    }
}