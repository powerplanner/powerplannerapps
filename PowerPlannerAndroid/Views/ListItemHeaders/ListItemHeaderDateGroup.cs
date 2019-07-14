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
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views.ListItemHeaders
{
    public class ListItemHeaderDateGroup : InflatedViewWithBinding
    {
        public ListItemHeaderDateGroup(ViewGroup root) : base(Resource.Layout.ListItemHeaderAgenda, root)
        {
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            if (newValue is DateTime)
            {
                var date = (DateTime)newValue;

                FindViewById<TextView>(Resource.Id.TextViewHeaderText).Text = GetHeaderText(date);
            }
        }

        private string GetHeaderText(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return PowerPlannerResources.GetRelativeDateToday().ToUpper();

            else if (date.Date == DateTime.Today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow().ToUpper();

            else if (date.Date == DateTime.Today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday().ToUpper();

            return date.ToString("dddd, MMM d").ToUpper();
        }
    }
}