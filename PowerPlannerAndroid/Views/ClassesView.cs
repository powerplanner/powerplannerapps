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
using PowerPlannerAndroid.Vx;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;

namespace PowerPlannerAndroid.Views
{
    public class ClassesView : MainScreenVxViewHostDescendant<ClassesViewModel>
    {
        private RecyclerView _recyclerView;

        public ClassesView(Context context) : base(context)
        {
            View = new FrameLayout(context)
                .VxLayoutParams().StretchHeight().Apply()
                .VxChildren(

                    // Classes
                    new RecyclerView(context)
                    {
                        // Use this setting to improve performance if you know that changes
                        // in content do not change the layout size of the RecyclerView
                        HasFixedSize = false
                    }
                        .VxLayoutParams().StretchHeight().Apply()
                        .VxPadding(0, 12, 0, 12)
                        .VxClipToPadding(false)
                        .VxReference(ref _recyclerView),

                    // No classes info
                    new LinearLayout(context)
                        .VxOrientation(Orientation.Vertical)
                        .VxLayoutParams().Gravity(GravityFlags.CenterVertical).Margins(16,0,16,0).Apply()
                        .VxVisibility(Binding<bool, bool>(nameof(ViewModel.HasClasses), c => !c))
                        .VxChildren(

                            new TextView(context, null, 0, Resource.Style.TextAppearance_AppCompat_Large)
                                .VxTextLocalized("ClassesPage_TextBlockNoClassesHeader.Text")
                                .VxLayoutParams().Gravity(GravityFlags.Center).Apply(),

                            new TextView(context, null, 0, Resource.Style.TextAppearance_AppCompat_Small)
                                .VxTextLocalized("ClassesPage_TextBlockNoClassesDescription.Text")
                                .VxLayoutParams().Gravity(GravityFlags.Center).Apply()

                        )

                );
        }

        public override void OnViewModelLoadedOverride()
        {
            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            _recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new ClassesAdapter(ViewModel)
            {
                ItemsSource = ViewModel.MainScreenViewModel.Classes
            };
            _recyclerView.SetAdapter(adapter);

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
                    var view = new ListItemClassMenuItemView(parent.Context);

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