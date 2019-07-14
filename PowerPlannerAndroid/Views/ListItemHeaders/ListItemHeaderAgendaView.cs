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
            if (newValue is AgendaViewModel.ItemsGroup)
            {
                var itemsGroup = (AgendaViewModel.ItemsGroup)newValue;
                string header = GetHeaderText(itemsGroup);

                FindViewById<TextView>(Resource.Id.TextViewHeaderText).Text = header;
            }
        }

        private static string GetHeaderText(AgendaViewModel.ItemsGroup itemsGroup)
        {
            switch (itemsGroup)
            {
                case AgendaViewModel.ItemsGroup.Overdue:
                    return PowerPlannerResources.GetRelativeDateInThePast();

                case AgendaViewModel.ItemsGroup.Today:
                    return PowerPlannerResources.GetRelativeDateToday();

                case AgendaViewModel.ItemsGroup.Tomorrow:
                    return PowerPlannerResources.GetRelativeDateTomorrow();

                case AgendaViewModel.ItemsGroup.InTwoDays:
                    return PowerPlannerResources.GetRelativeDateInXDays(2);

                case AgendaViewModel.ItemsGroup.WithinSevenDays:
                    return PowerPlannerResources.GetRelativeDateWithinXDays(7);

                case AgendaViewModel.ItemsGroup.WithinFourteenDays:
                    return PowerPlannerResources.GetRelativeDateWithinXDays(14);

                case AgendaViewModel.ItemsGroup.WithinThirtyDays:
                    return PowerPlannerResources.GetRelativeDateWithinXDays(30);

                case AgendaViewModel.ItemsGroup.WithinSixtyDays:
                    return PowerPlannerResources.GetRelativeDateWithinXDays(60);

                case AgendaViewModel.ItemsGroup.InTheFuture:
                    return PowerPlannerResources.GetRelativeDateFuture();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}