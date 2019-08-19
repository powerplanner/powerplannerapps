using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Runtime.CompilerServices;
using Microsoft.HockeyApp;
using System.Diagnostics;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPTelemetryExtension : TelemetryExtension
    {
        private string _userId = "";

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null)
        {
            try
            {
                HockeyClient.Current.TrackException(ex, new Dictionary<string, string>()
                {
                    { "ExceptionName", exceptionName },
                    { "UserId", _userId }
                });
            }
            catch { }
        }

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
#if DEBUG
            return;
#else
            try
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, string>();
                }
                if (properties.Count < 5)
                {
                    properties["UserId"] = _userId;
                }

                HockeyClient.Current.TrackEvent(eventName, properties);
            }
            catch { }
#endif
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            try
            {
                if (account != null)
                {
                    _userId = account.GetTelemetryUserId();
                    if (account.IsDefaultOfflineAccount)
                    {
                        HockeyClient.Current.UpdateContactInfo(account.GetTelemetryUserId(), "");
                    }
                    else
                    {
                        HockeyClient.Current.UpdateContactInfo(account.GetTelemetryUserId(), account.Username);
                    }
                }
                else
                {
                    _userId = "";
                }
            }
            catch { }
        }

        public override void TrackPageView(string pageName, DateTime timeVisited, TimeSpan duration)
        {
            try
            {
#if DEBUG
                Debug.WriteLine($"Page visited: {pageName} for {duration.TotalSeconds.ToString("0.#")} seconds on {timeVisited.ToString("t")}");
#endif
                HockeyClient.Current.TrackPageView(new Microsoft.HockeyApp.DataContracts.PageViewTelemetry(pageName)
                {
                    Duration = duration,
                    Timestamp = timeVisited,
                    Properties =
                    {
                        { "UserId", _userId }
                    }
                });
            }
            catch { }
        }
    }
}
