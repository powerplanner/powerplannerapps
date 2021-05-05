using System;
namespace Vx.Views
{
    public class Switch : View
    {
        public string Title { get; set; }
        public bool IsOn { get; set; }
        public Action<bool> IsOnChanged { get; set; }
        public bool IsEnabled { get; set; }
    }
}
