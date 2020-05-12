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
using PowerPlannerAndroid.Views.ListItemHeaders;
using InterfacesDroid.Views;
using AndroidX.RecyclerView.Widget;

namespace PowerPlannerAndroid.Adapters
{
    public class DateGroupedHomeworkAdapter : GroupedHomeworkAdapter<DateTime>
    {
        protected override RecyclerView.ViewHolder OnCreateViewHolderForHeader(ViewGroup parent)
        {
            var view = new ListItemHeaderDateGroup(parent);

            return new GenericRecyclerViewHolder(view);
        }
    }
}