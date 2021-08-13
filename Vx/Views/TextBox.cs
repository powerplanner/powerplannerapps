using BareMvvm.Core;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace Vx.Views
{
    public class TextBox : View
    {
        public TextBox() { }

        /// <summary>
        /// Creates a text box connected to the text field.
        /// </summary>
        /// <param name="textField"></param>
        public TextBox(TextField textField)
        {
            Text = VxValue.Create(textField);
            ValidationState = textField.ValidationState;
            HasFocusChanged = f => textField.HasFocus = f;
        }

        public string Header { get; set; }

        public VxValue<string> Text { get; set; }

        public string PlaceholderText { get; set; }

        public InputValidationState ValidationState { get; set; }

        public Action<bool> HasFocusChanged { get; set; }

        /// <summary>
        /// Set this to true to have the text box auto focus when first loaded
        /// </summary>
        public bool AutoFocus { get; set; }

        public InputScope InputScope { get; set; } = InputScope.Normal;

        public Action OnSubmit { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
