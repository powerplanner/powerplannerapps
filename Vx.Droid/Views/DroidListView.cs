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

        private void ItemClicked(object item)
        {
            VxView.ItemClicked?.Invoke(item);
        }

        private IEnumerable _currentItems;
        protected override void ApplyProperties(Vx.Views.ListView oldView, Vx.Views.ListView newView)
        {
            base.ApplyProperties(oldView, newView);

            _adapter.ItemClicked = newView.ItemClicked != null ? ItemClicked : null;

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
            private Action<object> _itemClicked;
            public Action<object> ItemClicked
            {
                get => _itemClicked;
                set
                {
                    var oldVal = _itemClicked;
                    _itemClicked = value;

                    // If we went from no click handler to one, or vice versa, need to re-render.
                    // We don't always use click handler since otherwise it makes sounds when ppl tap.
                    if (oldVal != null)
                    {
                        if (value == null)
                        {
                            NotifyDataSetChanged();
                        }
                    }
                    else if (oldVal == null)
                    {
                        if (value != null)
                        {
                            NotifyDataSetChanged();
                        }
                    }
                }
            }

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

            private const int CLICKABLE_VIEW_TYPE = 1;
            private const int NORMAL_VIEW_TYPE = 0;

            protected override int GetItemViewType(object item)
            {
                return ItemClicked != null ? CLICKABLE_VIEW_TYPE : NORMAL_VIEW_TYPE;
            }

            public override int GetItemViewType(int position)
            {
                return ItemClicked != null ? CLICKABLE_VIEW_TYPE : NORMAL_VIEW_TYPE;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var componentView = new VxDataTemplateComponent().Render();
                componentView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                if (viewType == CLICKABLE_VIEW_TYPE)
                {
                    componentView.Click += ComponentView_Click;
                }
                var holder = new GenericRecyclerViewHolder(componentView);
                return holder;
            }

            private void ComponentView_Click(object sender, EventArgs e)
            {
                var component = (sender as INativeComponent)?.Component as VxDataTemplateComponent;
                if (component != null && ItemClicked != null)
                {
                    ItemClicked(component.Data);
                }
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