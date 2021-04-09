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
            _value = value;
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

                    if (!Silent)
                    {
                        ValueChanged?.Invoke(this, new EventArgs());
                    }
                }
            }
        }

        public void SetValueSilently(object value)
        {
            _value = value;
        }

        protected virtual bool Silent { get => false; }
    }

    public class VxState<T> : VxState
    {
        public VxState(T value = default(T)) : base(value)
        {

        }

        public new T Value
        {
            get => (T)base.Value;
            set => base.Value = value;
        }

        public void SetValueSilently(T value)
        {
            base.SetValueSilently(value);
        }
    }

    public class VxSilentState<T> : VxState<T>
    {
        protected override bool Silent => true;

        public VxSilentState(T value = default(T)) : base(value)
        {

        }
    }
}
