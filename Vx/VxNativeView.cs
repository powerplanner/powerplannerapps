using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx
{
    public class VxNativeView
    {
        public static Dictionary<Type, Type> Mappings { get; private set; } = new Dictionary<Type, Type>();

        public VxView View { get; private set; }

        public object NativeView { get; private set; }

        public VxNativeView(VxView view, object nativeView)
        {
            View = view;
            NativeView = nativeView;

            foreach (var prop in view.Properties)
            {
                SetProperty(prop.Key, prop.Value);
            }
        }

        public static VxNativeView Create(VxView view)
        {
            if (Mappings.TryGetValue(view.GetType(), out Type nativeType))
            {
                return Activator.CreateInstance(nativeType, new object[] { view }) as VxNativeView;
            }

            throw new NotImplementedException();
        }

        public void ApplyDifferentView(VxView view)
        {
            var oldView = View;
            var newView = view;

            View = view;

            foreach (var prop in oldView.Properties)
            {
                if (!newView.Properties.ContainsKey(prop.Key))
                {
                    // TODO: Remove
                }
            }

            foreach (var prop in newView.Properties)
            {
                if (!oldView.Properties.TryGetValue(prop.Key, out object oldValue) || !object.Equals(oldValue, prop.Value))
                {
                    SetProperty(prop.Key, prop.Value);
                }
            }
        }

        private void SetProperty(string propName, object value)
        {
            this.GetType().GetProperty(propName).SetValue(this, value);
        }
    }

    public class VxNativeView<V, N> : VxNativeView where V : VxView
    {
        public new V View => base.View as V;

        public new N NativeView => (N)base.NativeView;

        public VxNativeView(V view, N nativeView) : base(view, nativeView) { }
    }
}
