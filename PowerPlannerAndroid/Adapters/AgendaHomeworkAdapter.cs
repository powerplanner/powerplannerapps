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
using PowerPlannerAndroid.Views.ListItemHeaders;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAndroid.Adapters
{
    public class AgendaHomeworkAdapter : GroupedHomeworkAdapter<AgendaViewModel.ItemsGroup>
    {
        protected override RecyclerView.ViewHolder OnCreateViewHolderForHeader(ViewGroup parent)
        {
            var view = new ListItemHeaderAgendaView(parent);

            return new GenericRecyclerViewHolder(view);
        }
    }
}