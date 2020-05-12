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
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAndroid.Adapters
{
    public class FlatHomeworkAdapter : ObservableRecyclerViewAdapter
    {
        public event EventHandler<BaseViewItemHomeworkExam> ItemClick;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
#if DEBUG
            try
            {
#endif
                if (viewType == 0)
                {
                    var view = new ListItemHomeworkView(parent);

                    view.Click += ListItemHomeworkView_Click;

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

        private void ListItemHomeworkView_Click(object sender, EventArgs e)
        {
            ItemClick?.Invoke(this, (sender as ListItemHomeworkView).DataContext as BaseViewItemHomeworkExam);
        }

        protected override int GetItemViewType(object item)
        {
            return 0;
        }
    }
}