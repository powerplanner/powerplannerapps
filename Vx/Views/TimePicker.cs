using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class TimePicker : View
    {
        public VxValue<TimeSpan> Value { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string Header { get; set; }
    }

    public class EndTimePicker : TimePicker
    {
        public TimeSpan StartTime { get; set; }
    }
}
