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
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.Adapters;

namespace PowerPlannerAndroid.Adapters
{
    public static class SpinnerWeightCategoriesAdapter
    {
        public static BaseAdapter<ViewItemWeightCategory> Create(IList<ViewItemWeightCategory> categories)
        {
            return ObservableAdapter.Create(categories, Resource.Layout.SpinnerItemWeightCategory);
        }
    }
}