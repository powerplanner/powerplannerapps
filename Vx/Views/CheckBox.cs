using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class CheckBox : View
    {
        public string Text { get; set; }
        public bool IsChecked { get; set; }
        public Action<bool> IsCheckedChanged { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
