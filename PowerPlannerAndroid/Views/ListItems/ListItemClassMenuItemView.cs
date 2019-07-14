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

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemClassMenuItemView : InflatedViewWithBinding
    {
        public ListItemClassMenuItemView(ViewGroup root) : base(Resource.Layout.ListItemClassMenuItem, root)
        {
        }
    }
}