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
        private string _userId;

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            try
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, string>();
                }

                if (_userId != null)
                {
                    // Custom events don't include the custom assigned UserId, so include manually
                    properties["AccountId"] = _userId;
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

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null)
        {
            try
            {
                Crashes.TrackError(ex, exceptionName == null ? null : new Dictionary<string, string>()
                {
                    { "ExceptionName", exceptionName }
                });
            }
            catch { }
        }

        public override void TrackPageView(string pageName, DateTime timeVisited, TimeSpan duration)
        {
            try
            {
                Analytics.TrackEvent("PageView_" + pageName, new Dictionary<string, string>()
                {
                    { "AccountId", _userId }
                });
            }
            catch { }
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);

            try
            {
                if (account != null)
                {
                    _userId = account.GetTelemetryUserId();

                    AppCenter.SetUserId(_userId);
                }
                else
                {
                    _userId = null;
                }
            }
            catch { }
        }
    }
}