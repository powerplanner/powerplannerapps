using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.Extensions;
using Android.Gms.Common;
using Firebase.Iid;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidPushExtension : PushExtension
    {
        public override Task<string> GetPushChannelUri()
        {
            try
            {
                // If Google Play services available
                if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == ConnectionResult.Success)
                {
                    string token = FirebaseInstanceId.Instance.Token;
                    System.Diagnostics.Debug.WriteLine("FirebaseToken: " + token);
                    return Task.FromResult(token);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return Task.FromResult<string>(null);
        }
    }
}