using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemCache
    {
        private class ViewItemCacheHelper<T> where T : BaseViewItem
        {
            private Dictionary<Guid, WeakReference<T>> _items = new Dictionary<Guid, WeakReference<T>>();

            public T GetOrNull(Guid identifier)
            {
                WeakReference<T> weakRef;

                if (_items.TryGetValue(identifier, out weakRef))
                {
                    T val;

                    if (weakRef.TryGetTarget(out val))
                        return val;
                }

                return null;
            }

            public void Set(Guid identifier, T item)
            {
                _items.Add(identifier, new WeakReference<T>(item));
            }
        }
    }
}
