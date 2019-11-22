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
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAndroid.Views
{
    public class AboutView : PopupViewHost<AboutViewModel>
    {
        private string _version = "";

        public AboutView(ViewGroup root) : base(Resource.Layout.About, root)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_AboutItem.Title");

            try
            {
                _version = PowerPlannerAppDataLibrary.Variables.VERSION.ToString();
                FindViewById<TextView>(Resource.Id.TextViewVersionNumber).Text = _version;
            }

            catch { }

            FindViewById<Button>(Resource.Id.ButtonEmailDeveloper).Click += ButtonEmailDeveloper_Click;
        }

        private void ButtonEmailDeveloper_Click(object sender, EventArgs e)
        {
            EmailDeveloper(Context, base.ViewModel);
        }

        public static void EmailDeveloper(Context context, BaseViewModel current)
        {
            string accountInfo = "";
            var mainScreen = current is MainScreenViewModel ? current as MainScreenViewModel : current.FindAncestor<MainScreenViewModel>();
            if (mainScreen != null && mainScreen.CurrentAccount != null)
            {
                accountInfo = " - " + mainScreen.CurrentAccount.GetTelemetryUserId() + " - " + mainScreen.CurrentAccount.DeviceId;
            }

            var _version = PowerPlannerAppDataLibrary.Variables.VERSION.ToString();
            Intent emailIntent = new Intent(Intent.ActionSendto);
            emailIntent.SetData(Android.Net.Uri.Parse("mailto:")); // Sendto and mailto ensures only email apps are shown
            emailIntent.PutExtra(Intent.ExtraEmail, new string[] { "support@powerplanner.net" });
            emailIntent.PutExtra(Intent.ExtraSubject, "Power Planner Droid - Contact Developer - " + _version + accountInfo);

            // Note that we're not including body text, since when sending from Outlook, it trims the leading newlines leaving no space for user to write.
            //emailIntent.PutExtra(Intent.ExtraText, "\n\nPower Planner Droid - Version " + _version + accountInfo);

            try
            {
                context.StartActivity(Intent.CreateChooser(emailIntent, "Send email"));
            }

            catch
            {
                Toast.MakeText(context, "You need to set up your email.", ToastLength.Short);
            }
        }
    }
}
