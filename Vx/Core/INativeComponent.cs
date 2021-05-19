using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx
{
    public interface INativeComponent
    {
        void ChangeView(View view);
    }
}
