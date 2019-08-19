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
    public class ConnectAccountView : PopupViewHost<ConnectAccountViewModel>
    {
        public ConnectAccountView(ViewGroup root) : base(Resource.Layout.WelcomeConnectAccount, root)
        {
            Title = PowerPlannerResources.GetString("Welcome_ConnectAccountPage.Title");

            FindViewById<Button>(Resource.Id.ButtonLogIn).Click += new WeakEventHandler(ConnectAccountView_Click).Handler;
        }

        private void ConnectAccountView_Click(object sender, EventArgs e)
        {
            ViewModel.LogIn();
        }
    }
}