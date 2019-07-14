using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.ListItems;
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAndroid.Adapters
{
    public abstract class GroupedHomeworkAdapter<THeaderItem> : ObservableRecyclerViewAdapter
    {
        public const int NORMAL_ITEM_TYPE = 0;
        public const int HEADER_ITEM_TYPE = 1;

        public event EventHandler<BaseViewItemHomeworkExam> ItemClick;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case NORMAL_ITEM_TYPE:
                    return CreateViewHolderForHomework(parent);

                case HEADER_ITEM_TYPE:
                    return OnCreateViewHolderForHeader(parent);

                default:
                    return base.OnCreateViewHolder(parent, viewType);
            }
        }

        private RecyclerView.ViewHolder CreateViewHolderForHomework(ViewGroup parent)
        {
            var view = new ListItemHomeworkView(parent);

            view.Click += ListItemHomeworkView_Click;

            return new GenericRecyclerViewHolder(view);
        }

        private void ListItemHomeworkView_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this, (sender as ListItemHomeworkView).DataContext as BaseViewItemHomeworkExam);
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