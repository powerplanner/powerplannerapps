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
using PowerPlannerSending;

namespace PowerPlannerAndroid.Views.SettingsViews.Grades
{
    public class ConfigureClassGradeScaleView : PopupViewHost<ConfigureClassGradeScaleViewModel>
    {
        private ItemsControlWrapper _itemsWrapperGradeScales;

        public ConfigureClassGradeScaleView(ViewGroup root) : base(Resource.Layout.ConfigureClassGradeScale, root)
        {
            Title = PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Title").ToUpper();

            SetMenu(Resource.Menu.configure_class_gradescale_menu);

            FindViewById<Button>(Resource.Id.ButtonAddGradeScale).Text = PowerPlannerResources.GetString("ClassPage_ButtonAddGradeScale.Content").Trim('+', ' ');
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonAddGradeScale).Click += delegate { ViewModel.AddGradeScale(); };

            _itemsWrapperGradeScales = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupGradeScales))
            {
                ItemsSource = ViewModel.GradeScales,
                ItemTemplate = new CustomDataTemplate<GradeScale>(CreateGradeScaleItemView)
            };
        }

        private View CreateGradeScaleItemView(ViewGroup parent, GradeScale scale)
        {
            var view = new ListItemEditingGradeScaleView(parent, scale);
            view.OnRequestRemove += GradeScale_OnRequestRemove;
            return view;
        }

        private void GradeScale_OnRequestRemove(object sender, GradeScale e)
        {
            ViewModel.RemoveGradeScale(e);
        }

        public override void OnMenuItemClicked(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
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