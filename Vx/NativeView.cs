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

        /// <summary>
        /// The native view
        /// </summary>
        public object View { get; set; }

        public View VxView { get; set; }

        public View VxParentView { get; set; }

        public void Apply(View newView)
        {
            var oldView = VxView;
            ApplyProperties(oldView, newView);
            VxView = newView;

            if (oldView != null)
            {
                newView.NativeView = oldView.NativeView;
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
