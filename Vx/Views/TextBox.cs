using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class TextBox : View
    {
        public string Header { get; set; }

        public VxState<string> Text { get; set; }

        public string PlaceholderText { get; set; }
    }
}
