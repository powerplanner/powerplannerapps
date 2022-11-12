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

        /// <summary>
        /// On touch-based devices, this will cause the "return" key to search for the next text box
        /// </summary>
        public bool AutoMoveToNextTextBox { get; set; }

        /// <summary>
        /// Used to "submit" the form. On keyboard-based devices, pressing enter on the 1st of 2 text boxes should submit the form (keyboard users use tab to go to the next field).
        /// On touch-based devices, pressing the virtual "return" key on the 1st of 2 text boxes should continue to the next text box, unless it's the last text box, which it should then execute submit.
        /// </summary>
        public Action OnSubmit { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
