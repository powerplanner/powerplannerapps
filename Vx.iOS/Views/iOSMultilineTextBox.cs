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
            View.Placeholder = newView.PlaceholderText;
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

        private readonly UILabel _placeholderLabel;

        public string Placeholder
        {
            get => _placeholderLabel.Text;
            set
            {
                _placeholderLabel.Text = value;
                UpdatePlaceholderVisibility();
            }
        }

        public UIRoundedTextView()
        {
            BackgroundColor = UIColorCompat.TertiarySystemFillColor;
            ClipsToBounds = true;
            Layer.CornerRadius = 10;
            Font = UIFont.PreferredBody;
            TextContainerInset = new UIEdgeInsets(8, 5, 8, 5); // Matches exactly with our single line text field

            Editable = true;

            _placeholderLabel = new UILabel
            {
                TextColor = UIColor.PlaceholderText,
                Font = UIFont.PreferredBody,
                Lines = 0
            };
            Add(_placeholderLabel);

            Changed += (s, e) => UpdatePlaceholderVisibility();

            ShouldBeginEditing = CustomShouldBeginEditing;
            ShouldEndEditing = CustomShouldEndEditing;
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                UpdatePlaceholderVisibility();
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            _placeholderLabel.Hidden = !string.IsNullOrEmpty(Text);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _placeholderLabel.Frame = new CGRect(10, 8, Math.Max(0, Bounds.Width - 20), Math.Max(0, Bounds.Height - 16));
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
        private UIView _headerContainer;
        private UIRoundedTextView _textField;
        private UILabel _errorSymbol;
        private UILabel _errorMessage;
        private UIView _errorMessageContainer;

        public UIRoundedTextViewWithHeader()
        {
            _headerContainer = new UIView();
            {
                _header = new UILabel();

                _errorSymbol = new UILabel
                {
                    Font = UIFont.FromName("Material Icons Outlined", UIFont.PreferredBody.PointSize),
                    Alpha = 0
                };

                _headerContainer.Add(_header);
                _headerContainer.Add(_errorSymbol);
            }

            _textField = new UIRoundedTextView();
            _textField.Changed += TextUpdated;
            _textField.FocusChanged += _textField_FocusChanged;

            _errorMessageContainer = new UIView
            {
                Hidden = true
            };
            {
                _errorMessage = new UILabel
                {
                    TextColor = UIColor.Red,
                    Font = UIFont.PreferredCaption1,
                    Lines = 0
                };
                _errorMessageContainer.Add(_errorMessage);
            }

            Add(_headerContainer);
            Add(_textField);
            Add(_errorMessageContainer);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            nfloat width = size.Width >= UIViewWrapper.UnboundedSize ? NMax(_header.SizeThatFits(size).Width, _textField.SizeThatFits(size).Width) : size.Width;
            var headerHeight = HeaderHeight(width);
            var errorHeight = ErrorHeight(width);
            nfloat textHeight = size.Height >= UIViewWrapper.UnboundedSize ? (nfloat)36 : NMax(36, size.Height - headerHeight - 4 - errorHeight);
            return new CGSize(width, headerHeight + 4 + textHeight + errorHeight);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var headerHeight = HeaderHeight(Bounds.Width);
            var errorHeight = ErrorHeight(Bounds.Width);
            var symbolSize = _errorSymbol.SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, headerHeight));
            _headerContainer.Frame = new CGRect(0, 0, Bounds.Width, headerHeight);
            _header.Frame = new CGRect(0, 0, NMax(0, Bounds.Width - symbolSize.Width), headerHeight);
            _errorSymbol.Frame = new CGRect(Bounds.Width - symbolSize.Width, 0, symbolSize.Width, headerHeight);
            var textY = headerHeight + 4;
            _textField.Frame = new CGRect(0, textY, Bounds.Width, NMax(36, Bounds.Height - textY - errorHeight));
            _errorMessageContainer.Frame = new CGRect(0, Bounds.Height - errorHeight, Bounds.Width, errorHeight);
            _errorMessage.Frame = new CGRect(0, 4, Bounds.Width, NMax(0, errorHeight - 4));
        }

        private nfloat HeaderHeight(nfloat width)
        {
            return NMax(_header.SizeThatFits(new CGSize(width, UIViewWrapper.UnboundedSize)).Height, _errorSymbol.SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, UIViewWrapper.UnboundedSize)).Height);
        }

        private nfloat ErrorHeight(nfloat width)
        {
            return _errorMessageContainer.Hidden ? (nfloat)0 : _errorMessage.SizeThatFits(new CGSize(width, UIViewWrapper.UnboundedSize)).Height + 4;
        }

        private static nfloat NMax(nfloat a, nfloat b) => a > b ? a : b;

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
                    _errorMessageContainer.Hidden = false;
                }
                else if (value == InputValidationState.Valid)
                {
                    _errorSymbol.Alpha = 1;
                    _errorSymbol.Text = MaterialDesign.MaterialDesignIcons.CheckCircleOutline;
                    _errorSymbol.TextColor = UIColor.SystemGreen;
                    _errorMessageContainer.Hidden = true;
                }
                else
                {
                    _errorSymbol.Alpha = 0;
                    _errorMessageContainer.Hidden = true;
                }

                InvalidateIntrinsicContentSize();
                SetNeedsLayout();
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
