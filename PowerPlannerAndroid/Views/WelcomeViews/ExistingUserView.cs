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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using ToolsPortable;

namespace PowerPlannerAndroid.Views.WelcomeViews
{
    public class ExistingUserView : PopupViewHost<ExistingUserViewModel>
    {
        public ExistingUserView(ViewGroup root) : base(Resource.Layout.WelcomeExistingUser, root)
        {
            Title = PowerPlannerResources.GetString("Welcome_ExistingUserPage.Title");

            FindViewById<Button>(Resource.Id.ButtonHasAccount).Click += new WeakEventHandler(ButtonHasAccount_Click).Handler;
            FindViewById<Button>(Resource.Id.ButtonNoAccount).Click += new WeakEventHandler(ButtonNoAccount_Click).Handler;
        }

        private void ButtonHasAccount_Click(object sender, EventArgs e)
        {
            ViewModel.HasAccount();
        }

        private void ButtonNoAccount_Click(object sender, EventArgs e)
        {
            ViewModel.NoAccount();
        }
    }
}