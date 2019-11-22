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
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAndroid.Views
{
    public class SettingsSyncOptionsView : PopupViewHost<SyncOptionsSimpleViewModel>
    {
        public SettingsSyncOptionsView(ViewGroup root) : base(Resource.Layout.SettingsSyncOptions, root)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_SyncOptionsItem.Title");

            FindViewById<View>(Resource.Id.SettingsImageUploadOptions).Click += delegate { ViewModel.OpenImageUploadOptions(); };
        }
    }
}