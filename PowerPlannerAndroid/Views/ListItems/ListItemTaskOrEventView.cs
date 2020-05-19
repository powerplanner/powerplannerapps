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
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemTaskOrEventView : InflatedViewWithBinding
    {
        public ListItemTaskOrEventView(ViewGroup root) : base(Resource.Layout.ListItemTaskOrEvent, root)
        {
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            base.OnDataContextChanged(oldValue, newValue);

            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            var barView = FindViewById<View>(Resource.Id.ListItemTaskPercentCompleteBar);

            if (DataContext is ViewItemTaskOrEvent taskOrEvent)
            {
                if (taskOrEvent.IsComplete)
                {
                    barView.Visibility = ViewStates.Gone;
                }

                else
                {
                    barView.Visibility = ViewStates.Visible;
                }
            }
        }
    }
}