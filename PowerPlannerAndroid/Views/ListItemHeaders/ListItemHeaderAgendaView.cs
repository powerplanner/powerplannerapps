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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views.ListItemHeaders
{
    public class ListItemHeaderAgendaView : InflatedViewWithBinding
    {
        public ListItemHeaderAgendaView(ViewGroup root) : base(Resource.Layout.ListItemHeaderAgenda, root)
        {
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            if (newValue is AgendaViewModel.ItemsGroupHeader itemsGroup)
            {
                string header = itemsGroup.Header;

                FindViewById<TextView>(Resource.Id.TextViewHeaderText).Text = header;
            }
        }
    }
}