using BareMvvm.Core.Strings;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace BareMvvm.Core
{
    public class TextField : BindableBase
    {
        public TextField(string initialText = "", bool required = false, IInputValidator inputValidator = null, int? minLength = null, int? maxLength = null, TextField mustMatch = null, BaseViewModel viewModel = null, bool ignoreOuterSpaces = false, bool reportValidatorInvalidInstantly = false)
        {
            _text = initialText;
            Required = required;
            MinLength = minLength;
            MaxLength = maxLength;
            InputValidator = inputValidator;
            MustMatch = mustMatch;
            IgnoreOuterSpaces = ignoreOuterSpaces;
            ReportValidatorInvalidInstantly = reportValidatorInvalidInstantly;

            if (MustMatch != null)
            {
                MustMatch.PropertyChanged += MustMatch_PropertyChanged;
            }
        }

        private void MustMatch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Validate(forceValidate: false);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                SetProperty(ref _text, value, nameof(Text));

                Validate(forceValidate: false);
            }
        }

        private InputValidationState _validationState;
        public InputValidationState ValidationState
        {
            get => _validationState;
            private set => SetProperty(ref _validationState, value, nameof(ValidationState));
        }

        private bool _hasFocus;
        public bool HasFocus
        {
            get => _hasFocus;
            set
            {
                _hasFocus = value;

                if (!value)
                {
                    Validate(forceValidate: true);
                }
            }
        }

        public bool Required { get; private set; }

        public int? MinLength { get; private set; }

        public int? MaxLength { get; private set; }

        public IInputValidator InputValidator { get; private set; }

        public TextField MustMatch { get; private set; }

        public BaseViewModel ViewModel { get; private set; }

        public bool IgnoreOuterSpaces { get; private set; }

        public bool ReportValidatorInvalidInstantly { get; private set; }

        public void Validate(bool forceValidate = true, bool? overrideRequired = null)
        {
            string text = Text;
            if (IgnoreOuterSpaces)
            {
                text = text.Trim();
            }

            bool required = overrideRequired != null ? overrideRequired.Value : Required;

            // We instantly stop and fail if this is the case
            if (MaxLength != null && text.Length > MaxLength)
            {
                ValidationState = InputValidationState.Invalid(string.Format(Resources.InvalidInput_TooLong, MaxLength.Value));
                return;
            }

            // We instantly succeed if this is the case
            if (MustMatch != null && MustMatch.ValidationState == InputValidationState.Valid && text == MustMatch.Text)
            {
                ValidationState = InputValidationState.Valid;
                return;
            }

            bool ranValidator = false;
            InputValidationState validatorAnswer = null;
            if (InputValidator != null && ReportValidatorInvalidInstantly)
            {
                validatorAnswer = InputValidator.Validate(text);
                ranValidator = true;

                if (validatorAnswer != null && validatorAnswer != InputValidationState.Valid)
                {
                    ValidationState = validatorAnswer;
                    return;
                }
            }

            if (ValidationState == null && !forceValidate)
            {
                return;
            }

            if (ValidationState == InputValidationState.Valid && !forceValidate)
            {
                ValidationState = null;
                return;
            }

            // After this point, all validations should return values and run

            if (text.Length == 0)
            {
                if (required)
                {
                    ValidationState = InputValidationState.Invalid(Resources.InvalidInput_Required);
                }
                else
                {
                    ValidationState = InputValidationState.Valid;
                }

                return;
            }

            if (MinLength != null && text.Length < MinLength)
            {
                ValidationState = InputValidationState.Invalid(string.Format(Resources.InvalidInput_TooShort, MinLength.Value));
                return;
            }

            if (InputValidator != null)
            {
                ValidationState = ranValidator ? validatorAnswer : InputValidator.Validate(text);
                return;
            }

            if (MustMatch != null)
            {
                if (MustMatch.ValidationState == InputValidationState.Valid)
                {
                    if (text == MustMatch.Text)
                    {
                        ValidationState = InputValidationState.Valid;
                        return;
                    }

                    if (forceValidate)
                    {
                        ValidationState = InputValidationState.Invalid(Resources.InvalidInput_DidntMatch);
                        return;
                    }
                }

                ValidationState = null;
                return;
            }

            ValidationState = InputValidationState.Valid;
        }
    }
}
