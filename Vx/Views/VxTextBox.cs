using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxTextBox : VxView
    {
        public string Header()
        {
            return GetProperty<string>();
        }

        public VxTextBox Header(string value)
        {
            SetProperty(value);
            return this;
        }

        public VxTextBox TextBinding(VxState<string> textState)
        {
            return this;
        }
        public VxTextBox Error(string value)
        {
            SetProperty(value);
            return this;
        }
    }
}
