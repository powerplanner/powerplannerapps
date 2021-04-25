using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class Button : View
    {
        public string Text { get; set; } = "";
        public Action Click { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
