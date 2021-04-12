using BareMvvm.Core.Strings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ToolsPortable;

namespace BareMvvm.Core
{
    public class EmailInputValidator : RegexInputValidator
    {
        public static readonly EmailInputValidator Instance = new EmailInputValidator();

        private EmailInputValidator() : base(StringTools.EmailRegex, Resources.InvalidInput_InvalidEmail) { }
    }

    public class RegexInputValidator : IInputValidator
    {
        public Regex Regex { get; private set; }
        public string ErrorText { get; private set; }

        public RegexInputValidator(string regex, string errorText)
        {
            ErrorText = errorText;

            if (!regex.StartsWith("^"))
            {
                regex = "^" + regex;
            }
            if (!regex.EndsWith("$"))
            {
                regex += "$";
            }

            Regex = new Regex(regex);
        }

        public InputValidationState Validate(string text)
        {
            if (Regex.IsMatch(text))
            {
                return InputValidationState.Valid;
            }
            else
            {
                return InputValidationState.Invalid(ErrorText);
            }
        }
    }

    public class CustomInputValidator : IInputValidator
    {
        private Func<string, InputValidationState> _validate;

        public CustomInputValidator(Func<string, InputValidationState> validate)
        {
            _validate = validate;
        }

        public InputValidationState Validate(string text)
        {
            return _validate(text);
        }

        public static string GetInvalidCharactersError(char[] invalidCharacters)
        {
            return string.Format(Resources.InvalidInputs_InvalidCharacters, string.Join(", ", invalidCharacters));
        }
    }
}
