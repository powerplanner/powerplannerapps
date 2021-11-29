using System;

namespace Vx.Views
{
    public class NumberTextBox : View
    {
        public VxValue<double?> Number { get; set; }

        public string PlaceholderText { get; set; } = "";

        public bool IsEnabled { get; set; } = true;

        public string Header { get; set; }

        public Action OnSubmit { get; set; }
    }
}
