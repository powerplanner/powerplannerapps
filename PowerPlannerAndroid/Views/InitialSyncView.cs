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
using PowerPlannerAppDataLibrary;
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

            FindViewById<TextView>(Resource.Id.InitialSync_SyncingText).Text = R.S("LoginPage_String_SyncingAccount");
            FindViewById<TextView>(Resource.Id.InitialSync_SyncErrorHeader).Text = R.S("String_SyncError");
            FindViewById<Button>(Resource.Id.ButtonSettings).Text = R.S("MainMenuItem_Settings");

            BindingHost.SetBinding<bool>(nameof(ViewModel.IsSyncing), isSyncing =>
            {
                FindViewById(Resource.Id.InitialSync_ProgressBar).Visibility = isSyncing ? ViewStates.Visible : ViewStates.Gone;
                FindViewById(Resource.Id.InitialSync_SyncingText).Visibility = isSyncing ? ViewStates.Visible : ViewStates.Gone;
                FindViewById(Resource.Id.InitialSync_SyncErrorHeader).Visibility = !isSyncing ? ViewStates.Visible : ViewStates.Gone;
                FindViewById(Resource.Id.ButtonSyncAgain).Visibility = !isSyncing ? ViewStates.Visible : ViewStates.Gone;
                FindViewById(Resource.Id.InitialSync_ResyncContainer).Visibility = !isSyncing ? ViewStates.Visible : ViewStates.Gone;
            });

            BindingHost.SetBinding<string>(nameof(ViewModel.Error), error =>
            {
                FindViewById<TextView>(Resource.Id.InitialSync_SyncErrorText).Text = error;
                FindViewById<TextView>(Resource.Id.InitialSync_SyncErrorText).Visibility = string.IsNullOrEmpty(error) ? ViewStates.Gone : ViewStates.Visible;
            });
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