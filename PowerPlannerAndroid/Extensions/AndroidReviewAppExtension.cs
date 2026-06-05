using Android.App;
using Android.Content;
using Android.Widget;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAndroid.Extensions
{
    internal class AndroidReviewAppExtension : ReviewAppExtension
    {
        public override Task ReviewAppAsync()
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse("market://details?id=" + Application.Context.PackageName));

            if (!TryStartActivity(intent))
            {
                // Google Play app not installed, try open web browser
                intent.SetData(Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=" + Application.Context.PackageName));

                if (!TryStartActivity(intent))
                {
                    Toast.MakeText(Application.Context, "Google Play not available", ToastLength.Short).Show();
                }
            }

            return Task.CompletedTask;
        }

        private bool TryStartActivity(Intent intent)
        {
            try
            {
                Application.Context.StartActivity(intent);
                return true;
            }
            catch (ActivityNotFoundException)
            {
                return false;
            }
        }
    }
}
