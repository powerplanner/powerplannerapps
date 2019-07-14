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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerAppDataLibrary;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Extensions;
using System.ComponentModel;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class LoginView : PopupViewHost<LoginViewModel>
    {
        public LoginView(ViewGroup root) : base(Resource.Layout.Login, root)
        {
            Title = PowerPlannerResources.GetString("LoginPage.Title");
            AutofillHelper.EnableForAll(this);
        }

        protected override void OnViewCreated()
        {
            FindViewById<Button>(Resource.Id.ButtonLogin).Click += ButtonLogin_Click;
        }

        public override void OnViewModelSetOverride()
        {
            ViewModel.AlertUserUpgradeAccountNeeded = AlertUserUpgradeAccountNeeded;
            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            FindViewById<Button>(Resource.Id.ButtonForgotUsername).Click += delegate { ViewModel.ForgotUsername(); };
            FindViewById<Button>(Resource.Id.ButtonForgotPassword).Click += delegate { ViewModel.ForgotPassword(); };

            KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextUsername));
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsLoggingInOnline):
                    UpdateIsLoggingInOnline();
                    break;

                case nameof(ViewModel.IsSyncingAccount):
                    UpdateIsSyncingAccount();
                    break;
            }
        }

        private ProgressDialog _progressDialogLoggingIn;
        private void UpdateIsLoggingInOnline()
        {
            if (ViewModel.IsLoggingInOnline)
            {
                if (_progressDialogLoggingIn == null)
                {
                    _progressDialogLoggingIn = new ProgressDialog(Context)
                    {
                        Indeterminate = true
                    };
                    _progressDialogLoggingIn.SetCancelable(false);
                    _progressDialogLoggingIn.SetTitle(PowerPlannerResources.GetString("LoginPage_String_LoggingIn"));
                }

                _progressDialogLoggingIn.Show();
            }

            else
            {
                if (_progressDialogLoggingIn != null)
                {
                    _progressDialogLoggingIn.Hide();
                }
            }
        }

        private ProgressDialog _progressDialogSyncing;
        private void UpdateIsSyncingAccount()
        {
            if (ViewModel.IsSyncingAccount)
            {
                if (_progressDialogSyncing == null)
                {
                    _progressDialogSyncing = new ProgressDialog(Context)
                    {
                        Indeterminate = true
                    };
                    _progressDialogSyncing.SetCancelable(false);
                    _progressDialogSyncing.SetTitle(PowerPlannerResources.GetString("LoginPage_String_SyncingAccount"));
                }

                _progressDialogSyncing.Show();
            }

            else
            {
                if (_progressDialogSyncing != null)
                {
                    _progressDialogSyncing.Hide();
                }
            }
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            var dontWait = ViewModel.LoginAsync();
        }

        private async void ShowMessage(string content, string title)
        {
            try
            {
                await new PortableMessageDialog(content, title).ShowAsync();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void AlertUserUpgradeAccountNeeded(string error)
        {
            ShowMessage(error, "Account upgrade needed");
            //CustomMessageBox mb = new CustomMessageBox(error, "Upgrade account", "cancel", "upgrade");
            //mb.Response += (s, r) =>
            //{
            //    if (r.Response == 1)
            //    {
            //        var dontWait = Launcher.LaunchUriAsync(new Uri("http://powerplanner.net/web/upgrade"));
            //    }
            //};
            //mb.Show();
        }
    }
}