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
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemGradeView : InflatedViewWithBinding
    {
        public ListItemGradeView(ViewGroup root) : base(Resource.Layout.ListItemGrade, root)
        {
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            base.OnDataContextChanged(oldValue, newValue);

            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            var barView = FindViewById<View>(Resource.Id.ListItemGradeColorBar);

            if (DataContext is BaseViewItemHomeworkExamGrade)
            {
                var g = DataContext as BaseViewItemHomeworkExamGrade;
                
                if (g.IsDropped)
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