using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Database;

namespace InterfacesDroid.Views
{
    public class AdapterLinearLayout : LinearLayout
    {
        private MyDataSetObserver _myDataSetObserver;

        public AdapterLinearLayout(Context context) : base(context)
        {
            _myDataSetObserver = new MyDataSetObserver(this);
        }

        public AdapterLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _myDataSetObserver = new MyDataSetObserver(this);
        }

        public AdapterLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            _myDataSetObserver = new MyDataSetObserver(this);
        }

        private BaseAdapter _adapter;
        public BaseAdapter Adapter
        {
            get { return _adapter; }
            set
            {
                // Unregister old
                if (_adapter != null)
                {
                    _adapter.UnregisterDataSetObserver(_myDataSetObserver);
                }

                _adapter = value;

                // Register new
                if (_adapter != null)
                {
                    _adapter.RegisterDataSetObserver(_myDataSetObserver);
                }
                
                // Reset since has been swapped completely
                ResetChildViews();
            }
        }

        private void ResetChildViews()
        {
            RemoveAllViews();

            if (Adapter == null)
            {
                return;
            }

            int count = Adapter.Count;
            for (int i = 0; i < count; i++)
            {
                View v = Adapter.GetView(i, null, this);

                if (v != null)
                {
                    AddView(v);
                    CacheItem(v, Adapter.GetItem(i));
                }
            }
        }

        private List<WeakReference<ViewAndObject>> _viewsAndObjects = new List<WeakReference<ViewAndObject>>();

        private void CleanUpCache()
        {
            for (int i = 0; i < _viewsAndObjects.Count; i++)
            {
                ViewAndObject obj;

                // If reference was dropped, remove from list
                if (!_viewsAndObjects[i].TryGetTarget(out obj))
                {
                    _viewsAndObjects.RemoveAt(i);
                    i--;
                }
            }
        }

        private void CacheItem(View view, object obj)
        {
            _viewsAndObjects.Add(new WeakReference<ViewAndObject>(new ViewAndObject(view, obj)));
        }

        private View GetCachedView(object obj)
        {
            foreach (var reference in _viewsAndObjects)
            {
                ViewAndObject entry;

                if (reference.TryGetTarget(out entry))
                {
                    if (entry.Object == obj)
                    {
                        return entry.View;
                    }
                }
            }

            return null;
        }

        private void OnItemsChanged()
        {
            if (Adapter == null)
            {
                return;
            }

            int count = Adapter.Count;
            for (int i = 0; i < count; i++)
            {
                object item = Adapter.GetItem(i);

                View cachedView = GetCachedView(item);

                // If we already have this view cached
                if (cachedView != null)
                {
                    // If the view is not already at this position
                    if (i >= base.ChildCount || base.GetChildAt(i) != cachedView)
                    {
                        // Remove it from its current location
                        base.RemoveView(cachedView);

                        // Insert it to its new location
                        base.AddView(cachedView, i);
                    }

                    // Otherwise, view is already at its position, we're good
                }

                // Otherwise, view isn't cached
                else
                {
                    // Create the view
                    View v = Adapter.GetView(i, null, this);

                    if (v != null)
                    {
                        // Add the view to the children
                        AddView(v, i);

                        // And also cache it
                        CacheItem(v, item);
                    }
                }
            }
        }

        private class ViewAndObject
        {
            public View View { get; private set; }
            public object Object { get; private set; }

            public ViewAndObject(View view, object obj)
            {
                View = view;
                Object = obj;
            }
        }

        private class MyDataSetObserver : DataSetObserver
        {
            private AdapterLinearLayout _adapterLinearLayout;
            public MyDataSetObserver(AdapterLinearLayout adapterLinearLayout)
            {
                _adapterLinearLayout = adapterLinearLayout;
            }
            
            public override void OnChanged()
            {
                _adapterLinearLayout.OnItemsChanged();
            }

            public override void OnInvalidated()
            {
                _adapterLinearLayout.OnItemsChanged();
            }
        }
    }
}