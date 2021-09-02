using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx
{
    public interface INativeComponent
    {
        VxComponent Component { get; }

        void ChangeView(View view);
    }
}
