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
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;

namespace PowerPlannerAndroid.Views
{
    public class WelcomeView : InterfacesDroid.Views.PopupViewHost<WelcomeViewModel>
    {
        public WelcomeView(ViewGroup root) : base(Resource.Layout.Welcome, root)
        {
        }

        protected override void OnViewCreated()
        {
            FindViewById<Button>(Resource.Id.ButtonLogin).Click += ButtonLogin_Click;
            FindViewById<Button>(Resource.Id.ButtonCreateAccount).Click += ButtonCreateAccount_Click;
        }

        private void ButtonCreateAccount_Click(object sender, EventArgs e)
        {
            ViewModel.CreateAccount();
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            ViewModel.Login();
        }
    }
}