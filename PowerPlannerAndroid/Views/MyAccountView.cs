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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAndroid.Views
{
    public class MyAccountView : PopupViewHost<MyAccountViewModel>
    {
        public MyAccountView(ViewGroup root) : base(Resource.Layout.MyAccount, root)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Title");

            FindViewById<Button>(Resource.Id.ButtonLogOut).Click += delegate { ViewModel.LogOut(); };
            FindViewById<Button>(Resource.Id.ButtonChangeUsername).Click += delegate { ViewModel.ChangeUsername(); };
            FindViewById<Button>(Resource.Id.ButtonChangePassword).Click += delegate { ViewModel.ChangePassword(); };
            FindViewById<Button>(Resource.Id.ButtonChangeEmail).Click += delegate { ViewModel.ChangeEmail(); };

            FindViewById<Button>(Resource.Id.ButtonDeleteAccount).Click += ButtonDeleteAccount_Click;
        }

        private void ButtonDeleteAccount_Click(object sender, EventArgs e)
        {
            ViewModel.ConfirmIdentityAndThen(ConfirmDeleteAccount);
        }

        private bool _deleteOnlineToo;
        private void ConfirmDeleteAccount()
        {
            var builder = new AlertDialog.Builder(Context);

            _deleteOnlineToo = false;

            builder
                .SetTitle(PowerPlannerResources.GetString("Settings_MyAccount_DeleteAccountConfirmation_Title"))
                .SetPositiveButton(PowerPlannerResources.GetStringCancel(), (s, e) => { })
                .SetNegativeButton(PowerPlannerResources.GetString("MenuItemDelete"), DeleteClicked)
                .SetMultiChoiceItems(new string[] { PowerPlannerResources.GetString("Settings_DeleteAccountPage_CheckBoxDeleteOnlineToo.Content") }, new bool[] { false }, (s, e) => { _deleteOnlineToo = e.IsChecked; });

            var dialog = builder.Create();
            dialog.Show();
        }

        private async void DeleteClicked(object sender, DialogClickEventArgs e)
        {
            try
            {
                Enabled = false;
                await ViewModel.DeleteAccount(_deleteOnlineToo);
            }

            finally
            {
                Enabled = true;
            }
        }
    }
}