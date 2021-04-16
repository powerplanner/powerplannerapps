using System;
using System.Collections.Generic;
using System.Reflection;
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

        protected PropertyInfo _sourceProperty;
        protected object _source;

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                if (!object.Equals(_value, value))
                {
                    _value = value;

                    if (_source != null)
                    {
                        _sourceProperty.SetValue(_source, value);
                    }

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

        internal static VxState<T> CreateBound(string propertyName, object source)
        {
            var prop = source.GetType().GetProperty(propertyName);
            var val = (T)prop.GetValue(source);

            return new VxState<T>(val)
            {
                _source = source,
                _sourceProperty = prop
            };
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
