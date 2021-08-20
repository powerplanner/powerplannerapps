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
        SizeF ComponentSize { get; }
        void ChangeView(View view);
    }
}
