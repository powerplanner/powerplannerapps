using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class PageViewTelemetryHelper
    {
        private static string _currPageName;
        private static DateTime _timePageVisitedOn = DateTime.MinValue;

        public static void TrackPageVisited(string pageName)
        {
            // Report the current page view
            ReportCurrentPageView();

            // And then update info to the new info
            _currPageName = pageName;
            _timePageVisitedOn = DateTime.Now;
        }

        public static void TrackLeavingApp()
        {
            // Report the current page view, but leave the info in place
            ReportCurrentPageView();
            _timePageVisitedOn = DateTime.MinValue;
        }

        public static void TrackReturningToApp()
        {
            // Update the page visit time so that we can re-track this page view
            // Note that this doesn't get called if Power Planner decides to re-launch the app since user
            // hadn't had it open for a day. In that case _timePageVisitedOn would remain MinValue until the next page visited gets
            // called, so we correctly ignore 
            _timePageVisitedOn = DateTime.Now;
        }

        private static void ReportCurrentPageView()
        {
            if (_currPageName != null && _timePageVisitedOn != DateTime.MinValue)
            {
                TimeSpan duration;
                var now = DateTime.Now;
                if (now > _timePageVisitedOn)
                {
                    duration = now - _timePageVisitedOn;
                }
                else
                {
                    return;
                }

                if (duration.TotalSeconds > 0.1)
                {
                    TelemetryExtension.Current?.TrackPageView(_currPageName, _timePageVisitedOn, duration);
                }
            }
        }
    }
}
