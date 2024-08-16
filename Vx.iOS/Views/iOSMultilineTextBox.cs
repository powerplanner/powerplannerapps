using System;
using CoreGraphics;
using InterfacesiOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSMultilineTextBox : iOSView<Vx.Views.MultilineTextBox, UIRoundedTextViewWithHeader>
    {
        public iOSMultilineTextBox()
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

        protected override void ApplyProperties(MultilineTextBox oldView, MultilineTextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Text != null)
            {
                View.Text = newView.Text.Value;
            }

            View.Header = newView.Header;
            //View.Placeholder = newView.PlaceholderText;
            View.ValidationState = newView.ValidationState;
            View.Enabled = newView.IsEnabled;

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

    public class UIRoundedTextView : UITextView
    {
        public event EventHandler<bool> FocusChanged;

        public bool Enabled { get; set; }
        public string Placeholder { get; set; }

        public UIRoundedTextView()
        {
            BackgroundColor = UIColorCompat.TertiarySystemFillColor;
            ClipsToBounds = true;
            Layer.CornerRadius = 10;
            Font = UIFont.PreferredBody;
            TextContainerInset = new UIEdgeInsets(8, 5, 8, 5); // Matches exactly with our single line text field

            Editable = true;

            ShouldBeginEditing = CustomShouldBeginEditing;
            ShouldEndEditing = CustomShouldEndEditing;
        }

        private bool CustomShouldBeginEditing(UITextView tv)
        {
            UpdateFocus(true);
            return true;
        }

        private bool CustomShouldEndEditing(UITextView tv)
        {
            UpdateFocus(false);
            return true;
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

    public class UIRoundedTextViewWithHeader : UIView
    {
        public event EventHandler<string> TextChanged;
        public event EventHandler<bool> FocusChanged;

        private UILabel _header;
        private UIRoundedTextView _textField;
        private UILabel _errorSymbol;
        private UILabel _errorMessage;
        private InterfacesiOS.Views.BareUIVisibilityContainer _errorMessageContainer;

        public UIRoundedTextViewWithHeader()
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

            _textField = new UIRoundedTextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _textField.Changed += TextUpdated;
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

            // Ensure text field expands when fixed height
            _textField.SetContentHuggingPriority(0, UILayoutConstraintAxis.Vertical);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[header]-4-[textField(>=36)][errorMessage]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
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
                if (_text == value)
                {
                    return;
                }

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
                if (_validationState == value)
                {
                    return;
                }

                _validationState = value;

                if (value != null && value.ErrorMessage != null)
                {
                    _errorSymbol.Alpha = 1;
                    _errorSymbol.Text = MaterialDesign.MaterialDesignIcons.ErrorOutline;
                    _errorSymbol.TextColor = UIColor.SystemRed;
                    _errorMessage.Text = value.ErrorMessage;
                    _errorMessageContainer.IsVisible = true;
                }
                else if (value == InputValidationState.Valid)
                {
                    _errorSymbol.Alpha = 1;
                    _errorSymbol.Text = MaterialDesign.MaterialDesignIcons.CheckCircleOutline;
                    _errorSymbol.TextColor = UIColor.SystemGreen;
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
                if (Enabled == value)
                {
                    return;
                }

                _textField.Enabled = value;
                Alpha = value ? 1.0f : 0.5f;
            }
        }

        public void SelectAll()
        {
            _textField.PerformSelector(new ObjCRuntime.Selector("selectAll"), null, 0.0f);
        }
    }
}
