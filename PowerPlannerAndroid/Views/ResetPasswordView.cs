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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using ToolsPortable;
using System.ComponentModel;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class ResetPasswordView : PopupViewHost<ResetPasswordViewModel>
    {
        public ResetPasswordView(ViewGroup root) : base(Resource.Layout.ResetPassword, root)
        {
            Title = PowerPlannerResources.GetString("ForgotPassword_ButtonHeader.Content").ToUpper();

            AutofillHelper.EnableForAll(this);
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonResetPassword).Click += delegate { ViewModel.ResetPassword(); };

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextUsername));
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsResettingPassword):
                    UpdateIsResettingPassword();
                    break;
            }
        }

        private ProgressDialog _progressDialogResettingPassword;
        private void UpdateIsResettingPassword()
        {
            if (ViewModel.IsResettingPassword)
            {
                if (_progressDialogResettingPassword == null)
                {
                    _progressDialogResettingPassword = new ProgressDialog(Context)
                    {
                        Indeterminate = true
                    };
                    _progressDialogResettingPassword.SetCancelable(false);
                    _progressDialogResettingPassword.SetTitle(PowerPlannerResources.GetString("ResetPassword_String_ResettingPasswordStatusText"));
                }

                _progressDialogResettingPassword.Show();
            }

            else
            {
                if (_progressDialogResettingPassword != null)
                {
                    _progressDialogResettingPassword.Hide();
                }
            }
        }
    }
}