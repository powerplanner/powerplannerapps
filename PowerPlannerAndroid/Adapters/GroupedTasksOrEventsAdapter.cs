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
using AndroidX.RecyclerView.Widget;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.ListItems;
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerAndroid.Adapters
{
    public abstract class GroupedTasksOrEventsAdapter<THeaderItem> : ObservableRecyclerViewAdapter
    {
        public const int NORMAL_ITEM_TYPE = 0;
        public const int HEADER_ITEM_TYPE = 1;

        public event EventHandler<ViewItemTaskOrEvent> ItemClick;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case NORMAL_ITEM_TYPE:
                    return CreateViewHolderForTaskOrEvent(parent);

                case HEADER_ITEM_TYPE:
                    return OnCreateViewHolderForHeader(parent);

                default:
                    return base.OnCreateViewHolder(parent, viewType);
            }
        }

        private RecyclerView.ViewHolder CreateViewHolderForTaskOrEvent(ViewGroup parent)
        {
            var view = new ListItemTaskOrEventView(parent);

            view.Click += ListItemTaskOrEventView_Click;

            return new GenericRecyclerViewHolder(view);
        }

        private void ListItemTaskOrEventView_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this, (sender as ListItemTaskOrEventView).DataContext as ViewItemTaskOrEvent);
        }

        protected abstract RecyclerView.ViewHolder OnCreateViewHolderForHeader(ViewGroup parent);

        protected override int GetItemViewType(object item)
        {
            if (item is THeaderItem)
                return HEADER_ITEM_TYPE;

            return NORMAL_ITEM_TYPE;
        }
    }
}