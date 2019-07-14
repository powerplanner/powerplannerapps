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
    public class ListItemHomeworkView : InflatedViewWithBinding
    {
        public ListItemHomeworkView(ViewGroup root) : base(Resource.Layout.ListItemHomework, root)
        {
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            base.OnDataContextChanged(oldValue, newValue);

            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            var barView = FindViewById<View>(Resource.Id.ListItemHomeworkPercentCompleteBar);

            if (DataContext is ViewItemHomework)
            {
                var h = DataContext as ViewItemHomework;

                if (h.IsComplete)
                {
                    barView.Visibility = ViewStates.Gone;
                }

                else
                {
                    barView.Visibility = ViewStates.Visible;
                }
            }

            else
            {
                if (DataContext is ViewItemExam && (DataContext as ViewItemExam).Date.Date >= DateTime.Today)
                {
                    barView.Visibility = ViewStates.Visible;
                }
                else
                {
                    barView.Visibility = ViewStates.Gone;
                }
            }
        }
    }
}