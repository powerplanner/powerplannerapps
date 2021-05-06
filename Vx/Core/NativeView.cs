using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx
{
    public abstract class NativeView
    {
        public static Func<View, NativeView> CreateNativeView;

        public static Action<ContextMenu, View> ShowContextMenu;

        /// <summary>
        /// The native view
        /// </summary>
        public object View { get; set; }

        public View VxView { get; set; }

        public View VxParentView { get; set; }

        private VxComponent _originalComponent;
        public void Apply(View newView)
        {
            var oldView = VxView;
            VxView = newView;

            if (newView is VxComponent newComponent)
            {
                HandleInnerComponent(oldView as VxComponent, newComponent);
            }

            if (newView.ViewRef != null)
            {
                // For components, we always return the original component
                newView.ViewRef(_originalComponent ?? VxView);
            }

            ApplyProperties(oldView, newView);

            if (oldView != null)
            {
                newView.NativeView = oldView.NativeView;
            }
        }

        private void HandleInnerComponent(VxComponent oldView, VxComponent newView)
        {
            if (oldView == null)
            {
                _originalComponent = newView;
                return;
            }

            // Only want properties above the base VxComponent
            foreach (var p in newView.GetType().GetProperties().Where(i => i.CanWrite && i.CanRead && typeof(VxComponent).IsAssignableFrom(i.DeclaringType) && i.Name != nameof(VxComponent.NativeComponent)))
            {
                var oldVal = p.GetValue(_originalComponent);
                var newVal = p.GetValue(newView);

                if (object.ReferenceEquals(_originalComponent, newVal) || object.Equals(_originalComponent, newVal))
                {
                    continue;
                }

                p.SetValue(_originalComponent, newVal);
                _originalComponent.MarkInternalComponentDirty();
            }
        }

        protected abstract void ApplyProperties(View oldView, View newView);

        public static void ReconcileList(IList<View> oldList, IList<View> newList, Action<int, View> insert, Action<int> remove, Action<int, View> replace, Action clear)
        {
#if DEBUG
            try
            {
#endif
                if (oldList == null || oldList.Count == 0)
                {
                    for (int newI = 0; newI < newList.Count; newI++)
                    {
                        insert(newI, newList[newI]);
                    }

                    return;
                }

                if (newList.Count == 0)
                {
                    clear();
                    return;
                }

                // Need to copy old list since I'll be modifying it
                oldList = new List<View>(oldList);

                int i = 0;

                for (; i < oldList.Count; i++)
                {
                    var oldItem = oldList[i];
                    var newItem = newList.ElementAtOrDefault(i);

                    if (newItem == null)
                    {
                        remove(i);
                        oldList.RemoveAt(i);
                    }
                    else if (oldItem.GetType() == newItem.GetType())
                    {
                        oldItem.NativeView.Apply(newItem);
                    }
                    else if (oldList.Count < newList.Count)
                    {
                        insert(i, newItem);
                        oldList.Insert(i, newItem);
                    }
                    else if (oldList.Count > newList.Count)
                    {
                        remove(i);
                        oldList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        replace(i, newItem);
                        oldList[i] = newItem;
                    }
                }

                if (oldList.Count < newList.Count)
                {
                    for (; i < newList.Count; i++)
                    {
                        insert(oldList.Count, newList[i]);
                        oldList.Add(newList[i]);
                    }
                }
#if DEBUG
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debugger.Break();
            }
#endif
        }
    }

    public abstract class NativeView<V, N> : NativeView where V : View
    {
        /// <summary>
        /// The native view
        /// </summary>
        public new N View
        {
            get => (N)base.View;
            set => base.View = value;
        }

        public new V VxView
        {
            get => base.VxView as V;
            set => base.VxView = value;
        }

        protected override void ApplyProperties(View oldView, View newView)
        {
            ApplyProperties(oldView as V, newView as V);
        }

        protected abstract void ApplyProperties(V oldView, V newView);
    }
}
