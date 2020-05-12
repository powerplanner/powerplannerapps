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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary;
using Google.Android.Material.TextField;
using AndroidX.Core.Content;

namespace PowerPlannerAndroid.Views
{
    public class ConfirmIdentityView : PopupViewHost<ConfirmIdentityViewModel>
    {
        public ConfirmIdentityView(ViewGroup root) : base(Resource.Layout.ConfirmIdentity, root)
        {
            Title = PowerPlannerResources.GetString("Settings_ConfirmIdentityPage.Title");

            AutofillHelper.EnableForAll(this);

            FindViewById<Button>(Resource.Id.ButtonConfirmIdentity).Click += delegate { ViewModel.Continue(); };
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.ActionIncorrectPassword += ViewModel_ActionIncorrectPassword;

            KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextPassword));
        }

        private void ViewModel_ActionIncorrectPassword(object sender, EventArgs e)
        {
            FindViewById<TextInputEditText>(Resource.Id.EditTextPassword).SetError("Incorrect password.", ContextCompat.GetDrawable(Context, Android.Resource.Drawable.StatNotifyError));
        }
    }
}