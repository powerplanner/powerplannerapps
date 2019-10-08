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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using PowerPlannerAndroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAndroid.Views
{
    public class ClassWhatIfView : InterfacesDroid.Views.PopupViewHost<ClassWhatIfViewModel>
    {
        public ClassWhatIfView(ViewGroup root) : base(Resource.Layout.ClassWhatIf, root)
        {
        }
        
        public override void OnViewModelLoadedOverride()
        {
            FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAdd).Click += ButtonAdd_Click;
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.RecyclerViewGrades);

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new ClassWhatIfGradesAdapter()
            {
                ItemsSource = ViewModel.ItemsWithHeaders
            };
            adapter.ItemClick += Adapter_ItemClick;
            recyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, BaseViewItemMegaItem e)
        {
            ViewModel.ShowItem(e);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            ViewModel.AddGrade();
        }
    }
}