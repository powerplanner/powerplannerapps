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
using InterfacesDroid.Views;
using PowerPlannerAndroid.Adapters;
using InterfacesDroid.Themes;
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewItems;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;

namespace PowerPlannerAndroid.Views
{
    public class ClassTasksOrEventsView : InterfacesDroid.Views.PopupViewHost<ClassTasksOrEventsViewModel>
    {
        public ClassTasksOrEventsView(ViewGroup root, ClassTasksOrEventsViewModel viewModel) : base(Resource.Layout.ClassTasksOrEvents, root)
        {
            ViewModel = viewModel;
        }

        private Button _buttonShowOldItems;

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAdd).Click += ButtonAdd_Click;
            FindViewById<Button>(Resource.Id.ButtonHideOldItems).Click += ButtonHideOldItems_Click;
            FindViewById<Button>(Resource.Id.ButtonHideOldItems).Text = PowerPlannerResources.GetString(ViewModel.Type == TaskOrEventType.Task ?
                    "ClassPage_ButtonHideOldTasksString" : "ClassPage_ButtonHideOldEventsString");
            RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.RecyclerViewAgenda);

            //recyclerView.AddItemDecoration(new DividerItemDecoration(Context));

            // Use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            //recyclerView.HasFixedSize = true;

            // Use a linear layout manager
            var layoutManager = new LinearLayoutManager(Context);
            recyclerView.SetLayoutManager(layoutManager);

            // Specify the adapter
            var adapter = new DateGroupedTasksOrEventsAdapter()
            {
                ItemsSource = ViewModel.ItemsWithHeaders,
                CreateViewHolderForFooter = CreateFooterViewHolder,
                Footer = "footer" // Don't need an object, but need this so footer counts towards items
            };

            adapter.ItemClick += Adapter_ItemClick;
            recyclerView.SetAdapter(adapter);

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            UpdateOldItems();
        }

        private GenericRecyclerViewHolder CreateFooterViewHolder(ViewGroup parent, object footer)
        {
            if (_buttonShowOldItems != null)
            {
                _buttonShowOldItems.Click -= ButtonShowOldItems_Click;
            }

            _buttonShowOldItems = new Button(Context)
            {
                Text = PowerPlannerResources.GetString(ViewModel.Type == TaskOrEventType.Task ?
                    "ClassPage_ButtonShowOldTasksString" : "ClassPage_ButtonShowOldEventsString"),
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    LinearLayout.LayoutParams.WrapContent)
                {
                    TopMargin = ThemeHelper.AsPx(Context, 20),
                    BottomMargin = ThemeHelper.AsPx(Context, 80)
                }
            };
            UpdateShowOldItemsVisibility();
            _buttonShowOldItems.Click += ButtonShowOldItems_Click;

            return new GenericRecyclerViewHolder(_buttonShowOldItems);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.PastCompletedItemsWithHeaders):
                    UpdateOldItems();
                    break;

                case nameof(ViewModel.HasPastCompletedItems):
                    UpdateShowOldItemsVisibility();
                    break;
            }
        }

        private void UpdateShowOldItemsVisibility()
        {
            if (_buttonShowOldItems != null)
            {
                if (ViewModel.HasPastCompletedItems)
                {
                    _buttonShowOldItems.Visibility = ViewStates.Visible;
                }
                else
                {
                    _buttonShowOldItems.Visibility = ViewStates.Gone;
                }
            }
        }

        private DateGroupedTasksOrEventsAdapter _oldItemsAdapter;
        private void UpdateOldItems()
        {
            if (ViewModel.PastCompletedItemsWithHeaders != null)
            {
                if (_oldItemsAdapter == null)
                {
                    var recyclerOldItems = FindViewById<RecyclerView>(Resource.Id.RecyclerViewOldItems);

                    // Use a linear layout manager
                    var layoutManager = new LinearLayoutManager(Context);
                    recyclerOldItems.SetLayoutManager(layoutManager);

                    _oldItemsAdapter = new DateGroupedTasksOrEventsAdapter();
                    _oldItemsAdapter.ItemClick += _oldItemsAdapter_ItemClick;

                    // Set the adapter
                    recyclerOldItems.SetAdapter(_oldItemsAdapter);
                }

                _oldItemsAdapter.ItemsSource = ViewModel.PastCompletedItemsWithHeaders;
            }
            else
            {
                if (_oldItemsAdapter != null)
                {
                    _oldItemsAdapter.ItemsSource = null;
                }
            }
        }

        private void _oldItemsAdapter_ItemClick(object sender, ViewItemTaskOrEvent e)
        {
            ViewModel.ShowItem(e);
        }

        private void ButtonHideOldItems_Click(object sender, EventArgs e)
        {
            ViewModel.HidePastCompletedItems();
        }

        private void ButtonShowOldItems_Click(object sender, EventArgs e)
        {
            ViewModel.ShowPastCompletedItems();
        }

        private void Adapter_ItemClick(object sender, ViewItemTaskOrEvent e)
        {
            ViewModel.ShowItem(e);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            ViewModel.Add();
        }
    }
}