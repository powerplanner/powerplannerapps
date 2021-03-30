using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Vx.Reconciler;
using Vx.Views;

namespace Vx
{
    public abstract class VxNativeView
    {
        public static Dictionary<Type, Type> Mappings { get; private set; } = new Dictionary<Type, Type>();

        public VxView View { get; private set; }

        public object NativeView { get; private set; }

        protected abstract object CreateNativeView();

        internal void SetInitialView(VxView view)
        {
            View = view;
            NativeView = CreateNativeView();

            Initialize();

            foreach (var prop in view.Properties)
            {
                SetProperty(prop.Key, prop.Value);
            }
        }

        public static VxNativeView Create(VxView view)
        {
            if (Mappings.TryGetValue(view.GetType(), out Type nativeType))
            {
                VxNativeView nativeView = Activator.CreateInstance(nativeType) as VxNativeView;
                nativeView.SetInitialView(view);
                return nativeView;
            }

#if DEBUG
            Debugger.Break();
#endif

            throw new NotImplementedException();
        }

        protected virtual void Initialize()
        {
            // Nothing
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
                    SetProperty(prop.Key, prop.Value);
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

    public abstract class VxNativeView<V, N> : VxNativeView where V : VxView
    {
        public new V View => base.View as V;

        public new N NativeView => (N)base.NativeView;

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

        private Dictionary<string, VxView[]> _currListsOfViews = new Dictionary<string, VxView[]>();
        private Dictionary<string, List<VxNativeView>> _listsOfNativeViews = new Dictionary<string, List<VxNativeView>>();

        protected void SetListOfViews(VxView[] views, Action<VxNativeViewListItemChange, int, VxNativeView> onChange, [CallerMemberName] string callerName = null)
        {
            _currListsOfViews.TryGetValue(callerName, out VxView[] oldViews);

            var changes = VxReconciler.ReconcileList(oldViews, views);

            // TODO: Create the native views here and then call back with the actual native views to add/change
            _listsOfNativeViews.TryGetValue(callerName, out List<VxNativeView> nativeViews);
            if (nativeViews == null)
            {
                nativeViews = new List<VxNativeView>();
                _listsOfNativeViews[callerName] = nativeViews;
            }

            foreach (var change in changes)
            {
                if (change is VxReconcilerInsertListItem insert)
                {
                    var newView = VxNativeView.Create(insert.NewView);
                    nativeViews.Insert(insert.Index, newView);
                    onChange(VxNativeViewListItemChange.Insert, insert.Index, newView);
                }

                else if (change is VxReconcilerUpdateListItem update)
                {
                    nativeViews[update.Index].ApplyDifferentView(update.NewView);
                }

                else if (change is VxReconcilerReplaceListItem replace)
                {
                    var newView = VxNativeView.Create(replace.NewView);
                    nativeViews[replace.Index] = newView;
                    onChange(VxNativeViewListItemChange.Replace, replace.Index, newView);
                }

                else if (change is VxReconcilerRemoveListItem remove)
                {
                    nativeViews.RemoveAt(remove.Index);
                    onChange(VxNativeViewListItemChange.Remove, remove.Index, null);
                }

                else
                {
                    throw new NotImplementedException();
                }
            }

            _currListsOfViews[callerName] = views;
        }

        protected enum VxNativeViewListItemChange
        {
            Insert,
            Replace,
            Remove
        }

        private class VxStateRegistration
        {
            public VxState State { get; set; }
            public EventHandler EventHandler { get; set; }
        }
    }
}
