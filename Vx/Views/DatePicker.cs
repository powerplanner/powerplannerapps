using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class DatePicker : View
    {
        public VxValue<DateTime?> Value { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string Header { get; set; }
    }
}
