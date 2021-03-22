using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxStackPanel : VxView
    {
        public VxView[] Children()
        {
            return GetProperty<VxView[]>();
        }

        public VxStackPanel Children(params VxView[] value)
        {
            SetProperty(value);
            return this;
        }
    }
}
