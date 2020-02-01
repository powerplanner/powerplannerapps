using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
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

        public abstract void TrackException(Exception ex, [CallerMemberName]string exceptionName = null);

        public abstract void TrackEvent(string eventName, IDictionary<string, string> properties = null);

        public abstract void TrackPageView(string pageName, DateTime timeVisited, TimeSpan duration);

        public virtual void UpdateCurrentUser(AccountDataItem account)
        {
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
        private string _userId;

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            System.Diagnostics.Debug.WriteLine($"Event: {eventName}");
        }

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
        }

        public override void TrackPageView(string pageName, DateTime timeVisited, TimeSpan duration)
        {
            System.Diagnostics.Debug.WriteLine($"PageView: {pageName}");
        }

        public override void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);
            _userId = account?.GetTelemetryUserId();
            System.Diagnostics.Debug.WriteLine($"CurrentUser: {account?.GetTelemetryUserId()}");
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
