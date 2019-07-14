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
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class ForgotUsernameView : PopupViewHost<ForgotUsernameViewModel>
    {
        public ForgotUsernameView(ViewGroup root) : base(Resource.Layout.ForgotUsername, root)
        {
            Title = PowerPlannerResources.GetString("ForgotUsername_String_MessageHeader").ToUpper();

            AutofillHelper.EnableForAll(this);
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonRecoverUsername).Click += delegate { ViewModel.Recover(); };

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextEmail));
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsRecoveringUsernames):
                    UpdateIsRecoveringUsernames();
                    break;
            }
        }

        private ProgressDialog _progressDialogRecoveringUsernames;
        private void UpdateIsRecoveringUsernames()
        {
            if (ViewModel.IsRecoveringUsernames)
            {
                if (_progressDialogRecoveringUsernames == null)
                {
                    _progressDialogRecoveringUsernames = new ProgressDialog(Context)
                    {
                        Indeterminate = true
                    };
                    _progressDialogRecoveringUsernames.SetCancelable(false);
                    _progressDialogRecoveringUsernames.SetTitle(PowerPlannerResources.GetString("ForgotUsername_String_FindingUsernameStatusText"));
                }

                _progressDialogRecoveringUsernames.Show();
            }

            else
            {
                if (_progressDialogRecoveringUsernames != null)
                {
                    _progressDialogRecoveringUsernames.Hide();
                }
            }
        }
    }
}