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
                _developerLogs.Add(EventToString(eventName, properties));

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
                _developerLogs.Add(ExceptionToString(ex, exceptionName, properties));

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

        private List<string> _developerLogs = new List<string>();
        public override string GetDeveloperLogs()
        {
            return string.Join("\n", _developerLogs);
        }
    }
}