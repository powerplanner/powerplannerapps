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

namespace PowerPlannerAndroid.Views
{
    public class AboutView : InterfacesDroid.Views.PopupViewHost<AboutViewModel>
    {
        private string _version = "";

        public AboutView(ViewGroup root) : base(Resource.Layout.About, root)
        {
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
            EmailDeveloper(Context);
        }

        public static void EmailDeveloper(Context context)
        {
            var _version = PowerPlannerAppDataLibrary.Variables.VERSION.ToString();
            Intent emailIntent = new Intent(Intent.ActionSend);
            emailIntent.SetType("message/rfc822");
            emailIntent.PutExtra(Intent.ExtraEmail, new string[] { "barebonesdev@live.com" });
            emailIntent.PutExtra(Intent.ExtraSubject, "Power Planner Droid - Contact Developer - " + _version);
            emailIntent.PutExtra(Intent.ExtraText, "\n\nPower Planner Droid - Version " + _version);

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