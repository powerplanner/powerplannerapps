using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxButton : VxView
    {
        public VxButton(string text)
        {

        }

        public string Text()
        {
            return GetProperty<string>();
        }

        public VxButton Text(string value)
        {
            SetProperty(value);
            return this;
        }

        public Action Click()
        {
            return GetProperty<Action>();
        }

        public VxButton Click(Action value)
        {
            SetProperty(value);
            return this;
        }
    }
}
