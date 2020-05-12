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
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlannerAndroid.Views.SettingsViews.Grades
{
    public class ConfigureClassPassingGradeView : PopupViewHost<ConfigureClassPassingGradeViewModel>
    {
        public ConfigureClassPassingGradeView(ViewGroup root) : base(Resource.Layout.ConfigureClassPassingGrade, root)
        {
            Title = PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title").ToUpper();

            SetMenu(Resource.Menu.configure_class_passinggrade_menu);
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