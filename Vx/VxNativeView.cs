using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
                    throw new NotImplementedException();
                }
            }

            foreach (var prop in newView.Properties)
            {
                if (prop.Value is IEnumerable<VxView> newViews)
                {
                    oldView.Properties.TryGetValue(prop.Key, out object oldValue);
                    IEnumerable<VxView> oldViews = oldValue as IEnumerable<VxView>;

                    VxView currOldView = null;
                    VxView currNewView = null;
                    IEnumerator<VxView> newViewsEnumerator = newViews.GetEnumerator();
                    IEnumerator<VxView> oldViewsEnumerator = oldViews?.GetEnumerator();

                    while (newViewsEnumerator.MoveNext())
                    {
                        oldViewsEnumerator?.MoveNext();

                        if (oldViewsEnumerator.Current == null || oldViewsEnumerator.Current.GetType() != newViewsEnumerator.Current.GetType())
                        {

                        }
                    }
                }

                else if (!oldView.Properties.TryGetValue(prop.Key, out object oldValue) || !object.Equals(oldValue, prop.Value))
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

        private Dictionary<string, VxStateRegistration> _binded = new Dictionary<string, VxStateRegistration>();

        protected void SetBindable<T>(VxState<T> value, Action<T> onSet, [CallerMemberName] string callerName = null)
        {
            if (_binded.TryGetValue(callerName, out VxStateRegistration old))
            {
                if (object.ReferenceEquals(old, value))
                {
                    return;
                }

                if (old.State != null)
                {
                    old.State.ValueChanged -= old.EventHandler;
                }

                value.ValueChanged += old.EventHandler;
                old.State = value;
            }
            else
            {
                EventHandler eventHandler = (object sender, EventArgs e) =>
                {
                    onSet(value.Value);
                };

                value.ValueChanged += eventHandler;

                _binded[callerName] = new VxStateRegistration()
                {
                    State = value,
                    EventHandler = eventHandler
                };
            }
        }

        private void Value_ValueChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private class VxStateRegistration
        {
            public VxState State { get; set; }
            public EventHandler EventHandler { get; set; }
        }
    }
}
