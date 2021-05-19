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

namespace PowerPlannerAndroid.Extensions
{
    public class DroidBrowserExtension : BrowserExtension
    {
        public override Task OpenUrlAsync(Uri uri)
        {
            var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri.ToString()));

            VxDroidExtensions.ApplicationContext.StartActivity(browserIntent);

            return Task.CompletedTask;
        }
    }
}