namespace Vx.Views
{
    public class NumberTextBox : View
    {
        public VxValue<double?> Number { get; set; }

        public string PlaceholderText { get; set; } = "";
    }
}
