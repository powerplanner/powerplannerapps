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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using ToolsPortable;

namespace PowerPlannerAndroid.Views.SettingsViews
{
    public class SuccessfullyCreatedAccountView : PopupViewHost<SuccessfullyCreatedAccountViewModel>
    {
        public SuccessfullyCreatedAccountView(ViewGroup root) : base(Resource.Layout.SettingsSuccessfullyCreatedAccount, root)
        {
            Title = PowerPlannerResources.GetString("Settings_SuccessfullyCreatedAccountPage.Title");

            FindViewById<Button>(Resource.Id.ButtonContinue).Click += new WeakEventHandler(SuccessfullyCreatedAccountView_Click).Handler;
        }

        private void SuccessfullyCreatedAccountView_Click(object sender, EventArgs e)
        {
            ViewModel.Continue();
        }
    }
}