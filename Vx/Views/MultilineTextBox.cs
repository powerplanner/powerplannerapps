using System;
using BareMvvm.Core;

namespace Vx.Views
{
    public class MultilineTextBox : TextBox
    {
        public MultilineTextBox() { }

        /// <summary>
        /// Creates a text box connected to the text field.
        /// </summary>
        /// <param name="textField"></param>
        public MultilineTextBox(TextField textField) : base(textField) { }
    }
}
