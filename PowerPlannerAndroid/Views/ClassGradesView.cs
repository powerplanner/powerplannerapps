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
    public class ClassGradesView : InterfacesDroid.Views.PopupViewHost<ClassGradesViewModel>
    {
        public ClassGradesView(ViewGroup root, ClassGradesViewModel viewModel) : base(Resource.Layout.ClassGrades, root)
        {
            ViewModel = viewModel;
        }
        
        public override void OnViewModelLoadedOverride()
        {
            FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAdd).Click += ButtonAdd_Click;
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.RecyclerViewGrades);

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new GradesAdapter()
            {
                ItemsSource = ViewModel.ItemsWithHeaders
            };
            adapter.ItemClick += Adapter_ItemClick;
            adapter.UnassignedItemClick += Adapter_UnassignedItemClick;
            adapter.ButtonWhatIfModeClick += Adapter_ButtonWhatIfModeClick;
            adapter.ButtonEditGradeOptionsClick += Adapter_ButtonEditGradeOptionsClick;
            recyclerView.SetAdapter(adapter);
        }

        private void Adapter_ButtonEditGradeOptionsClick(object sender, EventArgs e)
        {
            ViewModel.ConfigureGrades();
        }

        private void Adapter_ButtonWhatIfModeClick(object sender, EventArgs e)
        {
            ViewModel.OpenWhatIf();
        }

        private void Adapter_UnassignedItemClick(object sender, BaseViewItemHomeworkExam e)
        {
            ViewModel.ShowUnassignedItem(e);
        }

        private void Adapter_ItemClick(object sender, BaseViewItemHomeworkExamGrade e)
        {
            ViewModel.ShowItem(e);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            ViewModel.Add();
        }
    }
}