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
                    properties = new Dictionary<string, string>();
                }

                if (UserId != null)
                {
                    // Custom events don't include the custom assigned UserId, so include manually
                    if (!properties.ContainsKey("AccountId"))
                    {
                        properties["AccountId"] = UserId;
                    }
                }

                if (properties.Count > 0)
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

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null, IDictionary<string, string> properties = null)
        {
            try
            {
                Dictionary<string, string> finalProps = new Dictionary<string, string>();

                if (exceptionName != null)
                {
                    finalProps["ExceptionName"] = exceptionName;
                }

                if (properties != null)
                {
                    foreach (var p in properties.Take(4))
                    {
                        finalProps[p.Key] = p.Value;
                    }
                }

                Crashes.TrackError(ex, finalProps.Count == 0 ? null : finalProps);
            }
            catch { }
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);

            if (UserId != null)
            {
                AppCenter.SetUserId(UserId);
            }
        }
    }
}