using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsUniversal
{
    public class ObservableCollectionWithItemsOnBottom<T, K> : ObservableCollection<object>
    {
        private int _countOnBottom = 0;

        public ObservableCollectionWithItemsOnBottom(ObservableCollection<T> watch, IEnumerable<K> onBottom)
        {
            foreach (K item in onBottom)
            {
                base.Add(item);
                _countOnBottom++;
            }

            watch.CollectionChanged += watch_CollectionChanged;
        }

        private void add(object item)
        {
            base.Insert(base.Count - _countOnBottom, item);
        }

        void watch_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (object added in e.NewItems)
                        add(added);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (object removed in e.OldItems)
                        base.Remove(removed);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        base[e.OldStartingIndex + i] = e.NewItems[i];
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    for (int i = 0; i < e.OldItems.Count; i++)
                        base.RemoveAt(e.OldStartingIndex);

                    for (int i = 0; i < e.NewItems.Count; i++)
                        base.Insert(e.NewStartingIndex + i, e.NewItems[i]);

                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    for (int i = 0; i < base.Count - _countOnBottom; i++)
                        base.RemoveAt(0);

                    foreach (object added in e.NewItems)
                        add(added);

                    break;
            }
        }
    }
}
