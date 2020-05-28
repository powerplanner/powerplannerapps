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
using InterfacesDroid.Adapters;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.ListItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAndroid.Adapters
{
    public class FlatTasksOrEventsAdapter : ObservableRecyclerViewAdapter
    {
        public event EventHandler<ViewItemTaskOrEvent> ItemClick;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
#if DEBUG
            try
            {
#endif
                if (viewType == 0)
                {
                    var view = new ListItemTaskOrEventView(parent);

                    view.Click += ListItemTaskOrEventView_Click;

                    return new GenericRecyclerViewHolder(view);
                }

                return base.OnCreateViewHolder(parent, viewType);
#if DEBUG
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                throw ex;
            }
#endif
        }

        private void ListItemTaskOrEventView_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this, (sender as ListItemTaskOrEventView).DataContext as ViewItemTaskOrEvent);
        }

        protected override int GetItemViewType(object item)
        {
            return 0;
        }
    }
}