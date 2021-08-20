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
        void ChangeView(View view);
    }
}
