using BareMvvm.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class PasswordBox : TextBox
    {
        public PasswordBox() { }

        /// <summary>
        /// Creates a password box connected to the text field.
        /// </summary>
        /// <param name="textField"></param>
        public PasswordBox(TextField textField) : base(textField) { }
    }
}
