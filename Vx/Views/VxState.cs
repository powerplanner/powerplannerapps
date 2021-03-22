using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class VxState
    {
        public event EventHandler ValueChanged;

        public VxState(object value)
        {
            Value = value;
        }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                if (!object.Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke(this, new EventArgs());
                }
            }
        }
    }

    public class VxState<T> : VxState
    {
        public VxState(T value) : base(value)
        {
            
        }

        public new T Value
        {
            get => (T)base.Value;
            set => base.Value = value;
        }
    }
}
