using System;
namespace Vx.Views
{
    public class NumberTextBox : View
    {
        public VxState<double?> Number { get; set; }

        public string PlaceholderText { get; set; } = "";
    }
}
