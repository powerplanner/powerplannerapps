using System;
namespace Vx.Views
{
    public class Switch : View
    {
        public string Title { get; set; }
        public VxState<bool> IsOn { get; set; }
        public bool IsEnabled { get; set; }
    }
}
