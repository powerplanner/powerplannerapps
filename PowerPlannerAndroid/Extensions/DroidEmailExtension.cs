using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Droid;
using static Android.OS.Build;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidEmailExtension : EmailExtension
    {
        public override Task ComposeNewMailAsync(string to, string subject)
        {
            var context = VxDroidExtensions.ApplicationContext;

            Intent emailIntent = new Intent(Intent.ActionSendto);
            emailIntent.SetData(Android.Net.Uri.Parse("mailto:")); // Sendto and mailto ensures only email apps are shown
            emailIntent.PutExtra(Intent.ExtraEmail, new string[] { to });
            emailIntent.PutExtra(Intent.ExtraSubject, subject);

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

            return Task.CompletedTask;
        }
    }
}