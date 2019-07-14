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
using PowerPlannerSending;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemEditingGradeScaleView : InflatedViewWithBinding
    {
        public event EventHandler<GradeScale> OnRequestRemove;

        public ListItemEditingGradeScaleView(ViewGroup root, GradeScale gradeScale) : base(Resource.Layout.ListItemEditingGradeScale, root)
        {
            DataContext = gradeScale;

            FindViewById<ImageButton>(Resource.Id.ImageButtonRemoveItem).Click += RemoveItem_Click;
        }

        private void RemoveItem_Click(object sender, EventArgs e)
        {
            OnRequestRemove?.Invoke(sender, DataContext as GradeScale);
        }
    }
}