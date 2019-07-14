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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using PowerPlannerAppDataLibrary;
using ToolsPortable;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class CreateAccountView : PopupViewHost<CreateAccountViewModel>
    {
        public CreateAccountView(ViewGroup root) : base(Resource.Layout.CreateAccount, root)
        {
            Title = PowerPlannerResources.GetString("CreateAccountPage.Title");

            AutofillHelper.EnableForAll(this);
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonCreateAccount).Click += ButtonCreateAccount_Click;
            FindViewById<Button>(Resource.Id.ButtonCreateOfflineAccount).Click += ButtonCreateOfflineAccount_Click;

            ViewModel.AlertConfirmationPasswordDidNotMatch = AlertConfirmationPasswordDidNotMatch;
            ViewModel.AlertNoEmail = AlertNoEmail;
            ViewModel.AlertNoUsername = AlertNoUsername;
            ViewModel.AlertPasswordTooShort = AlertPasswordTooShort;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextUsername));
        }

        private void ButtonCreateOfflineAccount_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Context);

            builder
                .SetTitle(PowerPlannerResources.GetString("CreateAccountPage_String_WarningOfflineAccount"))
                .SetMessage(PowerPlannerResources.GetString("CreateAccountPage_String_WarningOfflineAccountExplanation"))
                .SetPositiveButton(PowerPlannerResources.GetString("String_Create"), (s, clickArgs) => { ViewModel.CreateLocalAccount(); })
                .SetNegativeButton(PowerPlannerResources.GetString("String_GoBack"), delegate { });

            var dialog = builder.Create();
            dialog.Show();
        }

        private void AlertConfirmationPasswordDidNotMatch()
        {
            ShowMessage(PowerPlannerResources.GetString("String_InvalidConfirmationPasswordExplanation"), PowerPlannerResources.GetString("String_InvalidConfirmationPassword"));
        }

        private void AlertNoEmail()
        {
            ShowMessage(PowerPlannerResources.GetString("String_NoEmailBody"), PowerPlannerResources.GetString("String_NoEmailHeader"));
        }

        private void AlertNoUsername()
        {
            ShowMessage(PowerPlannerResources.GetString("String_NoUsernameBody"), PowerPlannerResources.GetString("String_NoUsernameHeader"));
        }

        private void AlertPasswordTooShort()
        {
            ShowMessage(PowerPlannerResources.GetString("String_InvalidPasswordTooShortExplanation"), PowerPlannerResources.GetString("String_InvalidPassword"));
        }

        private void ShowMessage(string message, string title)
        {
            var dontWait = new PortableMessageDialog(message, title).ShowAsync();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsCreatingOnlineAccount):
                    UpdateIsCreatingOnlineAccount();
                    break;
            }
        }

        private ProgressDialog _progressDialogCreatingOnlineAccount;
        private void UpdateIsCreatingOnlineAccount()
        {
            if (ViewModel.IsCreatingOnlineAccount)
            {
                if (_progressDialogCreatingOnlineAccount == null)
                {
                    _progressDialogCreatingOnlineAccount = new ProgressDialog(Context)
                    {
                        Indeterminate = true
                    };
                    _progressDialogCreatingOnlineAccount.SetCancelable(false);
                    _progressDialogCreatingOnlineAccount.SetTitle("Creating account");
                }

                _progressDialogCreatingOnlineAccount.Show();
            }

            else
            {
                if (_progressDialogCreatingOnlineAccount != null)
                {
                    _progressDialogCreatingOnlineAccount.Hide();
                }
            }
        }

        private void ButtonCreateAccount_Click(object sender, EventArgs e)
        {
            ViewModel.CreateAccount();
        }
    }
}