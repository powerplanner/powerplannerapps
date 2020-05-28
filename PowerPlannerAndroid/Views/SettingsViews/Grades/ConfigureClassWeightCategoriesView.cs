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
using InterfacesDroid.DataTemplates;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.ListItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlannerAndroid.Views.SettingsViews.Grades
{
    public class ConfigureClassWeightCategoriesView : PopupViewHost<ConfigureClassWeightCategoriesViewModel>
    {
        private ItemsControlWrapper _itemsWrapperWeights;

        public ConfigureClassWeightCategoriesView(ViewGroup root) : base(Resource.Layout.ConfigureClassWeightCategories, root)
        {
            Title = PowerPlannerResources.GetString("ConfigureClassGrades_Items_WeightCategories.Title").ToUpper();

            SetMenu(Resource.Menu.configure_class_weightcategories_menu);

            FindViewById<Button>(Resource.Id.ButtonAddWeightCategory).Text = PowerPlannerResources.GetString("ClassPage_ButtonAddWeightCategory.Content").Trim('+', ' ');
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonAddWeightCategory).Click += delegate { ViewModel.AddWeightCategory(); };

            _itemsWrapperWeights = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupWeightCategories))
            {
                ItemsSource = ViewModel.WeightCategories,
                ItemTemplate = new CustomDataTemplate<EditingWeightCategoryViewModel>(CreateWeightCategoryItemView)
            };
        }

        private View CreateWeightCategoryItemView(ViewGroup parent, EditingWeightCategoryViewModel item)
        {
            var view = new ListItemEditingWeightCategoryView(parent, item);
            view.OnRequestRemove += WeightCategory_OnRequestRemove;
            return view;
        }

        private void WeightCategory_OnRequestRemove(object sender, EditingWeightCategoryViewModel e)
        {
            ViewModel.RemoveWeightCategory(e);
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel?.Save();
                    break;
            }
        }
    }
}