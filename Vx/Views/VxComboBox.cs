using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxComboBox : VxView
    {
        public VxComboBox Header(string value)
        {
            SetProperty(value);
            return this;
        }
    }
}
