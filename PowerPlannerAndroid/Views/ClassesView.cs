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
using AndroidX.RecyclerView.Widget;
using InterfacesDroid.Adapters;
using InterfacesDroid.Views;
using PowerPlannerAndroid.ViewHosts;
using PowerPlannerAndroid.Views.ListItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;

namespace PowerPlannerAndroid.Views
{
    public class ClassesView : MainScreenViewHostDescendant<ClassesViewModel>
    {
        public ClassesView(ViewGroup root) : base(Resource.Layout.Classes, root)
        {
            
        }

        public override void OnViewModelLoadedOverride()
        {
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.RecyclerViewClasses);

            // Use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            recyclerView.HasFixedSize = true;

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new ClassesAdapter(ViewModel)
            {
                ItemsSource = ViewModel.MainScreenViewModel.Classes
            };
            recyclerView.SetAdapter(adapter);

            base.OnViewModelLoadedOverride();
        }

        protected override int GetMenuResource()
        {
            return Resource.Menu.classes_menu;
        }

        public override void OnMenuItemClick(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemAdd:
                    ViewModel.AddClass();
                    break;
            }
        }

        private class ClassesAdapter : ObservableRecyclerViewAdapter
        {
            private ClassesViewModel _viewModel;

            public ClassesAdapter(ClassesViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == 0)
                {
                    var view = new ListItemClassMenuItemView(parent);

                    view.Click += View_Click;

                    return new GenericRecyclerViewHolder(view);
                }

                return base.OnCreateViewHolder(parent, viewType);
            }

            private void View_Click(object sender, EventArgs e)
            {
                _viewModel.OpenClass((sender as ListItemClassMenuItemView).DataContext as ViewItemClass);
            }

            protected override int GetItemViewType(object item)
            {
                // Only one item type
                return 0;
            }
        }
    }
}