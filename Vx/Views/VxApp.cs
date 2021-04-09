using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public abstract class VxApp : VxPageWithPopups
    {
        public VxApp()
        {
            IsRootComponent = true;
        }
    }
}
