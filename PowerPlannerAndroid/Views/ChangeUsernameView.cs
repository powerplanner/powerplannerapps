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
    public class ChangeUsernameView : PopupViewHost<ChangeUsernameViewModel>
    {
        private Button _buttonUpdateUsername;
        private TextInputEditText _editTextUsername;

        public ChangeUsernameView(ViewGroup root) : base(Resource.Layout.ChangeUsername, root)
        {
            Title = PowerPlannerResources.GetString("Settings_ChangeUsernamePage.Title");

            _buttonUpdateUsername = FindViewById<Button>(Resource.Id.ButtonUpdateUsername);
            _editTextUsername = FindViewById<TextInputEditText>(Resource.Id.EditTextUsername);

            _buttonUpdateUsername.Click += delegate { ViewModel.Update(); };
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.ActionError += ViewModel_ActionError;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateState();

            KeyboardHelper.FocusAndShow(_editTextUsername);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsUpdatingUsername):
                    UpdateState();
                    break;
            }
        }

        private void ViewModel_ActionError(object sender, string e)
        {
            _editTextUsername.SetError(e, null);
        }

        private void UpdateState()
        {
            if (ViewModel.IsUpdatingUsername)
            {
                Enabled = false;
                _buttonUpdateUsername.Text = "Updating username...";
            }

            else
            {
                Enabled = true;
                _buttonUpdateUsername.Text = PowerPlannerResources.GetString("Settings_ChangeUsernamePage_ButtonUpdateUsername.Content");
            }
        }
    }
}