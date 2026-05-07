using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace BareMvvm.Core.Binding
{
    public class BindingRegistration
    {
        public BindingHost BindingHost { get; private set; }
        internal string PropertyName { get; private set; }
        internal BindingHost.InternalBindingRegistration InternalRegistration { get; private set; }
        internal BindingRegistration SubRegistration { get; private set; }
        private string[] _fullPropertyPath;

        private bool IsEmptyRegistration => InternalRegistration == null && SubRegistration == null;

        internal BindingRegistration(BindingHost host, string propertyName, BindingHost.InternalBindingRegistration internalRegistration, string[] fullPropertyPath)
        {
            BindingHost = host;
            PropertyName = propertyName;
            InternalRegistration = internalRegistration;
            _fullPropertyPath = fullPropertyPath;
        }

        internal BindingRegistration(BindingHost host, string propertyName, BindingRegistration subRegistration, string[] fullPropertyPath)
        {
            BindingHost = host;
            PropertyName = propertyName;
            SubRegistration = subRegistration;
            _fullPropertyPath = fullPropertyPath;
        }

        public void Unregister()
        {
            BindingHost.UnregisterBinding(this);
        }

        /// <summary>
        /// Will ensure this binding registration's callback isn't triggered when value is set
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="value"></param>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "BindingHost is inherently reflection-based; callers are warned via RequiresUnreferencedCode on public API.")]
        public void SetSourceValue(object value, PropertyInfoAndObject preObtainedSourceProperty = null)
        {
            BindingHost.SetValue(_fullPropertyPath, value, IsEmptyRegistration ? null : this, preObtainedSourceProperty: preObtainedSourceProperty);
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "BindingHost is inherently reflection-based; callers are warned via RequiresUnreferencedCode on public API.")]
        public PropertyInfoAndObject GetSourceProperty()
        {
            return BindingHost.GetProperty(_fullPropertyPath);
        }

        internal BindingHost.InternalBindingRegistration GetFinalInternalRegistration()
        {
            if (InternalRegistration != null)
            {
                return InternalRegistration;
            }

            return SubRegistration.GetFinalInternalRegistration();
        }
    }
}
