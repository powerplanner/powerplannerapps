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
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAndroid.Views
{
    public class UpdateCredentialsView : PopupViewHost<UpdateCredentialsViewModel>
    {
        private Button _buttonLogIn;

        public UpdateCredentialsView(ViewGroup root) : base(Resource.Layout.UpdateCredentials, root)
        {
            Title = PowerPlannerResources.GetString("Settings_UpdateCredentialsPage.Title");

            _buttonLogIn = FindViewById<Button>(Resource.Id.ButtonLogIn);

            _buttonLogIn.Click += delegate { ViewModel.LogIn(); };
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            var explanation = FindViewById<TextView>(Resource.Id.TextViewUpdateCredentialsExplanation);

            if (ViewModel.UpdateType == UpdateCredentialsViewModel.UpdateTypes.NoDevice)
            {
                explanation.Text = "Your device has been removed from your online account. Please log back in to continue using your online account.";
            }

            else
            {
                explanation.Text = PowerPlannerResources.GetString("Settings_UpdateCredentialsPage_Description.Text");
            }

            FindViewById<Button>(Resource.Id.ButtonForgotUsername).Click += delegate { ViewModel.ForgotUsername(); };
            FindViewById<Button>(Resource.Id.ButtonForgotPassword).Click += delegate { ViewModel.ForgotPassword(); };
        }

        private void UpdateState()
        {
            if (ViewModel.IsLoggingIn)
            {
                Enabled = false;
                _buttonLogIn.Text = PowerPlannerResources.GetString("LoginPage_String_LoggingIn") + "...";
            }

            else
            {
                Enabled = true;
                _buttonLogIn.Text = PowerPlannerResources.GetString("Button_LogIn.Content");
            }
        }
    }
}