using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxGrid : VxView
    {
        public VxView[] Children()
        {
            return GetProperty<VxView[]>();
        }

        public VxGrid Children(params VxView[] value)
        {
            SetProperty(value);
            return this;
        }
    }
}
