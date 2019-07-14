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

namespace PowerPlannerAndroid.Views.ListItemHeaders
{
    public class ListItemHeaderWeightCategoryView : InflatedViewWithBinding
    {
        public ListItemHeaderWeightCategoryView(ViewGroup root) : base(Resource.Layout.ListItemHeaderWeightCategory, root)
        {
        }
    }
}