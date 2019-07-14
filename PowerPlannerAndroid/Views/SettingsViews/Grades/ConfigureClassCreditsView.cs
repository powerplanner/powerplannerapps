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
    public class ConfigureClassCreditsView : PopupViewHost<ConfigureClassCreditsViewModel>
    {
        public ConfigureClassCreditsView(ViewGroup root) : base(Resource.Layout.ConfigureClassCredits, root)
        {
            Title = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header").ToUpper();

            SetMenu(Resource.Menu.configure_class_credits_menu);
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