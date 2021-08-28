using System;
namespace Vx.Views
{
    public class DatePicker : View
    {
        public VxValue<DateTime?> Value { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string Header { get; set; }
    }
}
