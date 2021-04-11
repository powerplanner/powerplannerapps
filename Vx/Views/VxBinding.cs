using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public class VxBinding : IEquatable<VxBinding>
    {
        public VxState State { get; set; }
        public VxBindingMode BindingMode { get; set; }
        public string PropertyPath { get; set; }
        public object Source { get; set; }

        public void SetValue(object value)
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

        public bool Equals(VxBinding other)
        {
            if (other == null)
            {
                return false;
            }

            return object.ReferenceEquals(State, other.State)
                && BindingMode == other.BindingMode
                && PropertyPath == other.PropertyPath
                && object.ReferenceEquals(Source, other.Source);
        }

        public object BindingValue
        {
            get => State.Value;
            set => SetValue(value);
        }
    }

    public class VxBinding<T> : VxBinding
    {
        public new VxState<T> State
        {
            get => base.State as VxState<T>;
            set => base.State = value;
        }

        public void SetValue(T value)
        {
            base.SetValue(value);
        }
    }
}
