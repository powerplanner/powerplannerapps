using System;

namespace Vx.Views
{
    public class VxValue<T>
    {
        public T Value { get; private set; }
        public Action<T> ValueChanged { get; private set; }

        public VxValue(T value, Action<T> valueChanged)
        {
            Value = value;
            ValueChanged = valueChanged;
        }
    }

    public static class VxValue
    {
        public static VxValue<T> Create<T>(T value, Action<T> valueChanged)
        {
            return new VxValue<T>(value, valueChanged);
        }

        public static VxValue<string> Create(BareMvvm.Core.TextField textField)
        {
            return new VxValue<string>(textField.Text, v => textField.Text = v);
        }
    }
}
