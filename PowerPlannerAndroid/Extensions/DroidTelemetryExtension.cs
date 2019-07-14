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
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using System.Runtime.CompilerServices;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidTelemetryExtension : TelemetryExtension
    {
        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            try
            {
                if (properties == null)
                {
                    Analytics.TrackEvent(eventName);
                }

                else
                {
                    Analytics.TrackEvent(eventName, properties);
                }
            }
            catch { }
        }

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null)
        {
            try
            {
                Crashes.TrackError(ex);
            }
            catch { }
        }

        public override void TrackPageView(string pageName, DateTime timeVisited, TimeSpan duration)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Page visited: {pageName} for {duration.TotalSeconds.ToString("0.0")} seconds on {timeVisited.ToString("t")}");
#endif
            try
            {
                // Have to track username since it doesn't automatically get logged for events/page views
                Analytics.TrackEvent("PageView_" + pageName, new Dictionary<string, string>()
                {
                    { "Duration", Math.Ceiling(duration.TotalSeconds).ToString("0") }
                });
            }
            catch { }
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            try
            {
                if (account == null)
                {
                    return;
                }

                AppCenter.SetUserId(account.Username);
            }
            catch { }
        }
    }
}