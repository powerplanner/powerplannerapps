using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using Microsoft.AppCenter.Analytics;
using System.Runtime.CompilerServices;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;

namespace PowerPlanneriOS.Extensions
{
    public class iOSTelemetryExtension : TelemetryExtension
    {
        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            try
            {
                if (properties != null)
                {
                    Analytics.TrackEvent(eventName, properties);
                }
                else
                {
                    Analytics.TrackEvent(eventName);
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

        private static string TrimString(string str, int length)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > length)
            {
                return str.Substring(0, length);
            }

            return str;
        }

        public override void TrackPageView(string pageName, DateTime timeVisited, TimeSpan duration)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Page visited: {pageName} for {duration.TotalSeconds.ToString("0.0")} seconds on {timeVisited.ToString("t")}");
#endif
            try
            {
                Analytics.TrackEvent("PageView_" + pageName, new Dictionary<string, string>()
                {
                    { "Duration", duration.TotalSeconds.ToString("0") }
                });
            }
            catch { }
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            try
            {
                if (account != null)
                {
                    AppCenter.SetUserId(account.GetTelemetryUserId());
                }
            }
            catch { }
        }
    }
}