using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IButton
    {
        string Text { get; set; }

        Action Click { get; set; }
    }
    public class Button : View, IButton
    {
        public string Text { get; set; } = "";
        public Action Click { get; set; }
    }
}
