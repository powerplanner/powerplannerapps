using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class TimePicker : View
    {
        public TimeSpan Value { get; set; }

        public Action<TimeSpan> ValueChanged { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string Header { get; set; }
    }

    public class EndTimePicker : TimePicker
    {
        public TimeSpan StartTime { get; set; }
    }
}
