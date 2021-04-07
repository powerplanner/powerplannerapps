using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxBinding<T>
    {
        public VxState<T> State { get; set; }
        public VxBindingMode BindingMode { get; set; }

        public void SetValue(T value)
        {
            if (BindingMode == VxBindingMode.Silent)
            {
                State.SetValueSilently(value);
            }
            else
            {
                State.Value = value;
            }
        }
    }
}
