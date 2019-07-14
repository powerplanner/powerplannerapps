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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemEditingWeightCategoryView : InflatedViewWithBinding
    {
        public event EventHandler<EditingWeightCategoryViewModel> OnRequestRemove;

        public ListItemEditingWeightCategoryView(ViewGroup root, EditingWeightCategoryViewModel weightCategory) : base(Resource.Layout.ListItemEditingWeightCategory, root)
        {
            DataContext = weightCategory;

            FindViewById<ImageButton>(Resource.Id.ImageButtonRemoveItem).Click += RequestRemove_Click;
        }

        private void RequestRemove_Click(object sender, EventArgs e)
        {
            OnRequestRemove?.Invoke(sender, DataContext as EditingWeightCategoryViewModel);
        }
    }
}