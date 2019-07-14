using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class ViewGradeView : PopupViewHost<ViewGradeViewModel>
    {
        public ViewGradeView(ViewGroup root) : base(Resource.Layout.ViewGrade, root)
        {
            Title = PowerPlannerResources.GetString("ViewGradePage.Title");
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.Grade.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Grade_PropertyChanged).Handler;

            UpdateMenu();
        }

        private void Grade_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewItemGrade.IsDropped):
                    UpdateMenu();
                    break;
            }
        }

        private void UpdateMenu()
        {
            if (ViewModel.Grade.IsDropped)
            {
                SetMenu(Resource.Menu.view_dropped_grade_menu);
            }
            else
            {
                SetMenu(Resource.Menu.view_undropped_grade_menu);
            }
        }

        public override void OnMenuItemClicked(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemEdit:
                    ViewModel.Edit();
                    break;

                case Resource.Id.MenuItemDelete:
                    ViewModel.Delete();
                    break;

                case Resource.Id.MenuItemDropGrade:
                    ViewModel.DropGrade();
                    break;

                case Resource.Id.MenuItemUndropGrade:
                    ViewModel.UndropGrade();
                    break;
            }
        }
    }
}