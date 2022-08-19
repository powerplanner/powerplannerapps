using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using InterfacesDroid.Adapters;
using InterfacesDroid.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ToolsPortable;
using static Vx.Views.DataTemplateHelper;

namespace Vx.Droid.Views
{
    public class DroidListView : DroidView<Vx.Views.ListView, RecyclerView>
    {
        private VxDroidRecyclerAdapter _adapter;
        public DroidListView() : base(new RecyclerView(VxDroidExtensions.ApplicationContext))
        {
            var layoutManager = new LinearLayoutManager(View.Context);
            View.SetLayoutManager(layoutManager);

            _adapter = new VxDroidRecyclerAdapter();
            View.SetAdapter(_adapter);

            // Ensures padding will be added within the scrolling portion
            View.SetClipToPadding(false);
        }

        private IEnumerable _currentItems;
        protected override void ApplyProperties(Vx.Views.ListView oldView, Vx.Views.ListView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.ReferenceEquals(_currentItems, newView.Items) || !object.ReferenceEquals(oldView?.ItemTemplate, newView.ItemTemplate))
            {
                _adapter.ItemsSource = newView.Items as IReadOnlyList<object>;
                _adapter.ItemTemplate = newView.ItemTemplate;
                _currentItems = newView.Items;
            }

            View.SetPadding(AsPx(newView.Padding.Left), AsPx(newView.Padding.Top), AsPx(newView.Padding.Right), AsPx(newView.Padding.Bottom));
        }

        private class VxDroidRecyclerAdapter : ObservableRecyclerViewAdapter
        {
            private Func<object, Vx.Views.View> _itemTemplate;
            public Func<object, Vx.Views.View> ItemTemplate
            {
                get => _itemTemplate;
                set
                {
                    if (_itemTemplate != value)
                    {
                        _itemTemplate = value;
                        NotifyDataSetChanged();
                    }
                }
            }

            protected override int GetItemViewType(object item)
            {
                return 0;
            }

            public override int GetItemViewType(int position)
            {
                return 0;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var componentView = new VxDataTemplateComponent().Render();
                componentView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var holder = new GenericRecyclerViewHolder(componentView);
                return holder;
            }

            protected override void OnBindViewHolder(RecyclerView.ViewHolder holder, object item)
            {
                var componentView = holder.ItemView;
                var component = (componentView as INativeComponent).Component as VxDataTemplateComponent;
                component.Data = item;
                component.Template = _itemTemplate;
            }
        }
    }
}