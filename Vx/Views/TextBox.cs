using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace Vx.Views
{
    public class TextBox : View
    {
        public string Header { get; set; }

        public VxState<string> Text { get; set; }

        public string PlaceholderText { get; set; }

        public InputValidationState ValidationState { get; set; }

        public Action<bool> HasFocusChanged { get; set; }

        /// <summary>
        /// Set this to true to have the text box auto focus when first loaded
        /// </summary>
        public bool AutoFocus { get; set; }
    }
}
