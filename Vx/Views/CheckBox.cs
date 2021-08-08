using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class CheckBox : View
    {
        public string Text { get; set; }
        public VxValue<bool> IsChecked { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
