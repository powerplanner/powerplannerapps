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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAndroid.Views
{
    public class InitialSyncView : InterfacesDroid.Views.PopupViewHost<InitialSyncViewModel>
    {
        public InitialSyncView(ViewGroup root) : base(Resource.Layout.InitialSync, root)
        {
        }

        protected override void OnViewCreated()
        {
            FindViewById<Button>(Resource.Id.ButtonSyncAgain).Click += ButtonSyncAgain_Click;
            FindViewById<Button>(Resource.Id.ButtonSettings).Click += ButtonSettings_Click;
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            ViewModel.OpenSettings();
        }

        private void ButtonSyncAgain_Click(object sender, EventArgs e)
        {
            ViewModel.TryAgain();
        }
    }
}