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

namespace PowerPlannerAndroid.Views
{
    public class ChangePasswordView : PopupViewHost<ChangePasswordViewModel>
    {
        private Button _buttonUpdatePassword;
        private TextInputEditText _editTextPassword;
        private TextInputEditText _editTextConfirmPassword;

        public ChangePasswordView(ViewGroup root) : base(Resource.Layout.ChangePassword, root)
        {
            Title = PowerPlannerResources.GetString("Settings_ChangePasswordPage.Title");

            _buttonUpdatePassword = FindViewById<Button>(Resource.Id.ButtonUpdatePassword);
            _editTextPassword = FindViewById<TextInputEditText>(Resource.Id.EditTextPassword);
            _editTextConfirmPassword = FindViewById<TextInputEditText>(Resource.Id.EditTextConfirmPassword);

            _buttonUpdatePassword.Click += delegate { ViewModel.Update(); };
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.ActionError += ViewModel_ActionError;
            ViewModel.ActionPasswordsDidNotMatch += ViewModel_ActionPasswordsDidNotMatch;

            UpdateState();

            KeyboardHelper.FocusAndShow(_editTextPassword);
        }

        private void ViewModel_ActionPasswordsDidNotMatch(object sender, string e)
        {
            SetError(e);
        }

        private void ViewModel_ActionError(object sender, string e)
        {
            SetError(e);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsUpdatingPassword):
                    UpdateState();
                    break;
            }
        }

        private void UpdateState()
        {
            if (ViewModel.IsUpdatingPassword)
            {
                Enabled = false;
                _buttonUpdatePassword.Text = "Updating password...";
            }

            else
            {
                Enabled = true;
                _buttonUpdatePassword.Text = PowerPlannerResources.GetString("Settings_ChangePasswordPage_ButtonUpdatePassword.Content");
            }
        }

        private void SetError(string error)
        {
            FindViewById<TextView>(Resource.Id.TextViewError).Text = error;
        }
    }
}