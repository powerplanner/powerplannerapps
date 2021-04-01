using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;

namespace InterfacesDroid.Adapters
{
    public static class ObservableAdapter
    {
        public static BaseAdapter<T> Create<T>(IList<T> list, Func<ViewGroup, T, View> createViewFunction)
        {
            return new ObservableAdapterImplementation<T>(list, createViewFunction);
        }

        public static BaseAdapter<T> Create<T>(IList<T> list, int itemResourceId, int? dropDownItemResourceId = null)
        {
            Func<ViewGroup, View> createDropDownViewFunction = null;

            if (dropDownItemResourceId != null)
                createDropDownViewFunction = (root) =>
                {
                    return new InflatedViewWithBinding(dropDownItemResourceId.Value, root);
                };

            return new ObserableAdapterWithBinding<T>(list, (root) =>
            {
                return new InflatedViewWithBinding(itemResourceId, root);
            }, createDropDownViewFunction);
        }

        /// <summary>
        /// Creates an adapter using the default spinners
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static BaseAdapter<string> CreateSimple(IEnumerable<string> strings)
        {
            return new SimpleStringAdapter(strings);
        }

        private class SimpleStringAdapter : BaseAdapter<string>
        {
            private string[] _strings;

            public SimpleStringAdapter(IEnumerable<string> strings)
            {
                _strings = strings.ToArray();
            }

            public override string this[int position] => _strings[position];

            public override int Count => _strings.Length;

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                return GetViewSharedHelper(Android.Resource.Layout.SimpleSpinnerItem, position, convertView, parent);
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                return GetViewSharedHelper(Android.Resource.Layout.SimpleSpinnerDropDownItem, position, convertView, parent);
            }

            private View GetViewSharedHelper(int resource, int position, View convertView, ViewGroup parent)
            {
                string text = _strings[position];

                if (convertView is TextView)
                {
                    (convertView as TextView).Text = text;
                    return convertView;
                }

                var textView = (TextView)LayoutInflater.FromContext(parent.Context).Inflate(resource, parent, attachToRoot: false);
                textView.Text = _strings[position];
                return textView;
            }
        }

        private class ObservableAdapterImplementation<T> : BaseAdapter<T>
        {
            public Func<ViewGroup, T, View> CreateView { get; private set; }

            public IList<T> List { get; private set; }

            public ObservableAdapterImplementation(IList<T> list, Func<ViewGroup, T, View> createViewFunction)
            {
                List = list;
                CreateView = createViewFunction;
            }

            public override T this[int position]
            {
                get
                {
                    return List[position];
                }
            }

            public override int Count
            {
                get
                {
                    return List.Count;
                }
            }

            public override long GetItemId(int position)
            {
                return List[position].GetHashCode();
            }

            private Dictionary<int, WeakReference<View>> _cachedViews = new Dictionary<int, WeakReference<View>>();

            private void CacheCreatedView(T dataItem, View view)
            {
                _cachedViews[dataItem.GetHashCode()] = new WeakReference<View>(view);
            }

            private View GetCachedView(T dataItem)
            {
                WeakReference<View> reference;

                if (_cachedViews.TryGetValue(dataItem.GetHashCode(), out reference))
                {
                    View view;
                    if (reference.TryGetTarget(out view))
                        return view;
                }

                return null;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                // The convertView is the previous View at that position (if there is one).
                // So that means if something was inserted at the top, the convertView would be off by one index.
                // We could theoretically assign the new data context on each one, but that means there would
                // be O(N) changes to the data context of each.

                // Alternatively, we can cache mappings between the item and its convertView, and return the corresponding
                // view if it exists, and otherwise create and map it. That should be O(1).

                // However, the convertView is also used for recycling views when scrolling through long lists...
                // So theoretically, as we scroll down, the item's view at the top would end up appearing for an item further down,
                // which we wouldn't return, and instead we would create a new one, and the top item's view would lose its reference.

                // Best design would probably have a ViewWithDataContext, and then assign DataContext on them.

                // We could also support options for keeping all views loaded.

                if (CreateView == null)
                    throw new NullReferenceException("CreateView function cannot be null.");

                var dataItem = this[position];

                var cachedView = GetCachedView(dataItem);
                if (cachedView != null)
                    return cachedView;

                var newView = CreateView.Invoke(parent, dataItem);
                CacheCreatedView(dataItem, newView);
                return newView;
            }
        }


        private class ObserableAdapterWithBinding<T> : BaseAdapter<T>
        {
            public IList<T> List { get; private set; }

            public Func<ViewGroup, View> CreateView { get; private set; }

            public Func<ViewGroup, View> CreateDropDownViewFunction { get; private set; }

            public ObserableAdapterWithBinding(IList<T> list, Func<ViewGroup, View> createViewFunction, Func<ViewGroup, View> createDropDownViewFunction)
            {
                if (list == null)
                    throw new ArgumentNullException(nameof(list));

                if (createViewFunction == null)
                    throw new ArgumentNullException(nameof(createViewFunction));

                List = list;
                CreateView = createViewFunction;
                CreateDropDownViewFunction = createDropDownViewFunction;
            }

            public override T this[int position]
            {
                get
                {
                    return List[position];
                }
            }

            public override int Count
            {
                get
                {
                    return List.Count;
                }
            }

            public override long GetItemId(int position)
            {
                return List[position].GetHashCode();
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                T item = this[position];

                if (convertView == null)
                {
                    convertView = CreateView.Invoke(parent);
                }

                var dataContextProperty = convertView.GetType().GetProperties().FirstOrDefault(i => i.Name.Equals("DataContext"));

                if (dataContextProperty != null)
                    dataContextProperty.SetValue(convertView, item);

                return convertView;
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                if (CreateDropDownViewFunction == null)
                    return this.GetView(position, convertView, parent);

                T item = this[position];

                if (convertView == null)
                {
                    convertView = CreateDropDownViewFunction.Invoke(parent);
                }

                var dataContextProperty = convertView.GetType().GetProperties().FirstOrDefault(i => i.Name.Equals("DataContext"));

                if (dataContextProperty != null)
                    dataContextProperty.SetValue(convertView, item);

                return convertView;
            }
        }
    }
}