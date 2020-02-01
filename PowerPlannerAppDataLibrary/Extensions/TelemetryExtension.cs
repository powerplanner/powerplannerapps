using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class TelemetryExtension
    {
        public const string EVENT_VIEW_CALENDAR = "ViewCalendar";
        public const string EVENT_VIEW_DAY = "ViewDay";
        public const string EVENT_VIEW_AGENDA = "ViewAgenda";
        public const string EVENT_VIEW_SCHEDULE = "ViewSchedule";
        public const string EVENT_VIEW_CLASS = "ViewClass";
        public const string EVENT_VIEW_YEARS = "ViewYears";
        public const string EVENT_VIEW_SETTINGS = "ViewSettings";
        public const string EVENT_VIEW_HOMEWORK = "ViewHomework";
        public const string EVENT_VIEW_EXAM = "ViewExam";
        public const string EVENT_VIEW_ADD_HOMEWORK = "ViewAddHomework";
        public const string EVENT_VIEW_EDIT_HOMEWORK = "ViewEditHomework";
        public const string EVENT_VIEW_ADD_EXAM = "ViewAddExam";
        public const string EVENT_VIEW_EDIT_EXAM = "ViewEditExam";
        public const string EVENT_VIEW_ADD_CLASS = "ViewAddClass";
        public const string EVENT_VIEW_EDIT_CLASS = "ViewEditClass";
        public const string EVENT_VIEW_ADD_YEAR = "ViewAddYear";
        public const string EVENT_VIEW_EDIT_YEAR = "ViewEditYear";
        public const string EVENT_VIEW_ADD_SEMESTER = "ViewAddSemester";
        public const string EVENT_VIEW_EDIT_SEMESTER = "ViewEditSemester";
        public const string EVENT_VIEW_ADD_CLASS_TIMES = "ViewAddClassTimes";
        public const string EVENT_VIEW_EDIT_CLASS_TIMES = "ViewEditClassTimes";
        public const string EVENT_VIEW_EDIT_CLASS_DETAILS = "ViewEditClassDetails";
        public const string EVENT_VIEW_EDIT_CLASS_GRADES = "ViewEditClassGrades";

#if DEBUG
        private static DebugTelemetryExtension _debugTelemetryExtension = new DebugTelemetryExtension();
        public static TelemetryExtension Current
        {
            get => _debugTelemetryExtension;
            set { /* no-op */ }
        }
#else
        public static TelemetryExtension Current { get; set; }
#endif

        public string UserId { get; private set; }

        public abstract void TrackException(Exception ex, [CallerMemberName]string exceptionName = null);

        public abstract void TrackEvent(string eventName, IDictionary<string, string> properties = null);

        private string _lastPageName;
        private object _pageViewLock = new object();
        private TelemetryPageViewBundler _pageViewBundler;
        public virtual void TrackPageVisited(string pageName)
        {
            try
            {
                if (pageName.EndsWith("View"))
                {
                    pageName = pageName.Substring(0, pageName.Length - 4);
                }

                _lastPageName = pageName;

                // App Center analytics allows up to 20 properties per event, and each property value can only be 125 chars long...

                // Events are usually part of the same date, so no reason to log date info, just time... But we can't infer date from the time the event was logged,
                // since it'd be difficult to tell when it's just about to change date and we should log

                // Functions we have in log query are todatetime("2015-12-24") or todatetime("2018-06-30 20:12:42.9") (ISO8601 format), and totimespan("0.00:01:00") (1 minute)
                // totimespan("1:05") == 1 hr, 5 min
                // totimespan("0:1:05") == 1 min, 5 sec
                // totimespan("5") == 5 hr
                // totimespan("1:01:00") == 1 hour, 1 min
                // totimespan("0:0:20") == 20 sec

                /*
                 * Name: PageViews
                 * Properties:
                 *  AccountId: 39179
                 *  Date: 1/31/2019
                 *  Pages: Calendar,0:0:16;Agenda,0:0:4;ViewTask,0:0:12
                 * */

                DateTime utcTimeVisited = DateTime.UtcNow;
                string userId = UserId;

                lock (_pageViewLock)
                {
                    if (_pageViewBundler == null)
                    {
                        _pageViewBundler = new TelemetryPageViewBundler(userId);
                    }

                    // If able to add the page to the bundler
                    if (_pageViewBundler.TryAddPage(userId, pageName, utcTimeVisited))
                    {
                        return;
                    }

                    // Otherwise, need to log bundler and create new
                    FinishPageViewBundler();

                    // And then add to new one
                    _pageViewBundler = new TelemetryPageViewBundler(userId);
                    _pageViewBundler.TryAddPage(userId, pageName, utcTimeVisited);
                }
            }
            catch (Exception ex)
            {
                TrackException(ex);
            }
        }

        public void LeavingApp()
        {
            try
            {
                lock (_pageViewLock)
                {
                    FinishPageViewBundler();
                }
            }
            catch (Exception ex) { TrackException(ex); }
        }

        public void ReturnedToApp()
        {
            TrackPageVisited(_lastPageName);
        }

        private void FinishPageViewBundler()
        {
            TrackEvent("PageViews", _pageViewBundler.GenerateProperties());

            _pageViewBundler = null;
        }

        public virtual void UpdateCurrentUser(AccountDataItem account)
        {
            if (account != null)
            {
                UserId = account.GetTelemetryUserId();
            }
            else
            {
                UserId = null;
            }

            if (account == null || !account.IsOnlineAccount)
            {
                CurrentAccountId = 0;
            }
            else
            {
                CurrentAccountId = account.AccountId;
            }
        }

        public long CurrentAccountId { get; private set; }

        static TelemetryExtension()
        {
            // For exceptions handled within portable libraries
            ExceptionHelper.OnHandledExceptionOccurred = OnHandledExceptionOccurred;
        }

        private static void OnHandledExceptionOccurred(Exception ex)
        {
            // For exceptions handled within portable libraries
            Current?.TrackException(ex);
        }
    }

#if DEBUG
    public class DebugTelemetryExtension : TelemetryExtension
    {
        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            System.Diagnostics.Debug.WriteLine($"Event: {eventName}");
        }

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
        }

        public override void TrackPageVisited(string pageName)
        {
            System.Diagnostics.Debug.WriteLine($"PageView: {pageName}");
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);

            System.Diagnostics.Debug.WriteLine($"CurrentUser: {UserId}");
        }
    }
#endif

    namespace Telemetry
    {
        public enum SeverityLevel
        {
            //
            // Summary:
            //     Verbose severity level.
            Verbose = 0,
            //
            // Summary:
            //     Information severity level.
            Information = 1,
            //
            // Summary:
            //     Warning severity level.
            Warning = 2,
            //
            // Summary:
            //     Error severity level.
            Error = 3,
            //
            // Summary:
            //     Critical severity level.
            Critical = 4
        }

        public enum ExceptionHandledAt
        {
            //
            // Summary:
            //     Exception was not handled. Application crashed.
            Unhandled = 0,
            //
            // Summary:
            //     Exception was handled in user code.
            UserCode = 1,
            //
            // Summary:
            //     Exception was handled by some platform handlers.
            Platform = 2
        }
    }
}
