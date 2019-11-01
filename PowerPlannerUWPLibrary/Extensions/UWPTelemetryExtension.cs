using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPTelemetryExtension : TelemetryExtension
    {
        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null)
        {
            try
            {
#if DEBUG
                Debugger.Break();
#endif

                Crashes.TrackError(ex, exceptionName == null ? null : new Dictionary<string, string>()
                {
                    { "ExceptionName", exceptionName }
                });
            }
            catch (Exception ex2)
            {
                TrackEvent("FailedTrackError", new Dictionary<string, string>()
                {
                    { "Message", ex2.Message },
                    { "OriginalMessage", ex.Message }
                });
            }
        }

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
#if DEBUG
            return;
#else
            try
            {
                Analytics.TrackEvent(eventName, properties);
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
                    var userId = account.GetTelemetryUserId();

                    AppCenter.SetUserId(userId);
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
                Analytics.TrackEvent("PageView_" + pageName, new Dictionary<string, string>()
                {
                    { "Duration", Math.Ceiling(duration.TotalSeconds).ToString("0") }
                });
            }
            catch { }
        }
    }
}
