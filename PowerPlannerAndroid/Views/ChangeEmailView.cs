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
using Android.Support.Design.Widget;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class ChangeEmailView : PopupViewHost<ChangeEmailViewModel>
    {
        private Button _buttonUpdateEmail;
        private TextView _textViewError;
        private TextInputEditText _editTextEmail;

        public ChangeEmailView(ViewGroup root) : base(Resource.Layout.ChangeEmail, root)
        {
            _buttonUpdateEmail = FindViewById<Button>(Resource.Id.ButtonUpdateEmail);
            _textViewError = FindViewById<TextView>(Resource.Id.TextViewError);
            _editTextEmail = FindViewById<TextInputEditText>(Resource.Id.EditTextEmail);

            _buttonUpdateEmail.Click += _buttonChangeEmail_Click;

            Title = PowerPlannerResources.GetString("Settings_ChangeEmailPage.Title");

            AutofillHelper.EnableForAll(this);
        }

        private void _buttonChangeEmail_Click(object sender, EventArgs e)
        {
            ViewModel.UpdateEmail();
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateState();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsRetrievingEmail):
                case nameof(ViewModel.IsUpdatingEmail):
                    UpdateState();
                    break;
            }
        }

        private void UpdateState()
        {
            if (ViewModel.IsRetrievingEmail)
            {
                DisableInput();
                _buttonUpdateEmail.Text = "Retrieving email...";
            }

            else if (ViewModel.IsUpdatingEmail)
            {
                DisableInput();
                _buttonUpdateEmail.Text = "Updating email...";
            }

            else
            {
                EnableInput();
                _buttonUpdateEmail.Text = PowerPlannerResources.GetString("Settings_ChangeEmailPage_ButtonUpdateEmail.Content");
            }
        }

        private void DisableInput()
        {
            _buttonUpdateEmail.Enabled = false;
            _editTextEmail.Enabled = false;
        }

        private void EnableInput()
        {
            _buttonUpdateEmail.Enabled = true;
            _editTextEmail.Enabled = true;
        }
    }
}