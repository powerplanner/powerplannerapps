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
    public static class SpinnerClassesAdapter
    {
        public static BaseAdapter<ViewItemClass> Create(IList<ViewItemClass> classes)
        {
            return ObservableAdapter.Create(classes, Resource.Layout.SpinnerItemClass);
        }
    }
}