using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface ITextBlock : IView
    {
        string Text { get; set; }
    }

    public class TextBlock : View, ITextBlock
    {
        public string Text { get; set; } = "";
    }
}
