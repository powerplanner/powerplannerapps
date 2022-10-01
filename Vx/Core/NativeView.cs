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

        public View VxViewRef => _originalComponent ?? VxView;

        private VxComponent _originalComponent;
        public void Apply(View newView)
        {
#if DEBUG
            newView.Validate();
#endif

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

            bool changed = false;

            // Only want properties above the base VxComponent
            foreach (var p in newView.GetType().GetProperties().Where(i => i.CanWrite && i.CanRead && typeof(VxComponent).IsAssignableFrom(i.DeclaringType) && i.Name != nameof(VxComponent.NativeComponent)))
            {
                var oldVal = p.GetValue(_originalComponent);
                var newVal = p.GetValue(newView);

                if (object.ReferenceEquals(oldVal, newVal) || object.Equals(oldVal, newVal))
                {
                    continue;
                }

                p.SetValue(_originalComponent, newVal);
                changed = true;
            }

            if (changed)
            {
                // We render on demand here to stay in sync with the parent view changes.
                // This is critical for iOS ListView to work correctly with changing views
                _originalComponent.RenderOnDemand();
            }
        }

        protected abstract void ApplyProperties(View oldView, View newView);

        public static void ReconcileList(IList<View> oldList, IList<View> newList, Action<int, View> insert, Action<int> remove, Action<int, View> replace, Action clear)
        {
#if DEBUG
            try
            {
#endif
                // Exclude rendering null items
                newList = newList.Where(v => v != null).ToList();

                if (oldList == null || oldList.Count == 0)
                {
                    for (int newI = 0; newI < newList.Count; newI++)
                    {
                        insert(newI, newList[newI]);

#if DEBUG
                        if (newList[newI].NativeView == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif
                    }

                    return;
                }

                if (newList.Count == 0)
                {
                    clear();
                    return;
                }

                // Need to copy old list since I'll be modifying it
                oldList = oldList.Where(v => v != null).ToList();

                int i = 0;

                for (; i < oldList.Count; i++)
                {
                    var oldItem = oldList[i];
                    var newItem = newList.ElementAtOrDefault(i);

                    if (newItem == null)
                    {
                        remove(i);
                        oldList.RemoveAt(i);
                        i--;
                    }
                    else if (oldItem.GetType() == newItem.GetType())
                    {
#if DEBUG
                        if (oldItem.NativeView == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif

                        oldItem.NativeView.Apply(newItem);

#if DEBUG
                        if (newItem.NativeView == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif
                    }
                    else if (oldList.Count < newList.Count)
                    {
                        insert(i, newItem);
                        oldList.Insert(i, newItem);

#if DEBUG
                        if (newItem.NativeView == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif
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

#if DEBUG
                        if (newItem.NativeView == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif
                    }
                }

                if (oldList.Count < newList.Count)
                {
                    for (; i < newList.Count; i++)
                    {
                        insert(oldList.Count, newList[i]);
                        oldList.Add(newList[i]);

#if DEBUG
                        if (newList[i].NativeView == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
#endif
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
