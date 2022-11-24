using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace Vx
{
    public interface INativeComponent
    {
        event EventHandler<SizeF> ComponentSizeChanged;
        event EventHandler ThemeChanged;
        event EventHandler<bool> MouseOverChanged;
        SizeF ComponentSize { get; }
        VxComponent Component { get; }

        void ChangeView(View view);
    }
}
