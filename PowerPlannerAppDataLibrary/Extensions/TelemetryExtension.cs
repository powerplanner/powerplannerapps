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

        public abstract void TrackException(Exception ex, [CallerMemberName]string exceptionName = null, IDictionary<string, string> properties = null);

        public abstract void TrackEvent(string eventName, IDictionary<string, string> properties = null);

        protected static string EventToString(string eventName, IDictionary<string, string> properties = null)
        {
            string s = $"Event: {eventName}";

            if (properties != null && properties.Count > 0)
            {
                s += ", " + string.Join(", ", properties.Select(i => i.Key + ": " + i.Value));
            }

            return s;
        }

        protected static string ExceptionToString(Exception ex, [CallerMemberName] string exceptionName = null, IDictionary<string, string> properties = null)
        {
            string s = $"Exception: {ex.ToString()}";

            if (exceptionName != null)
            {
                s += ", " + exceptionName;
            }

            if (properties != null && properties.Count > 0)
            {
                s += ", " + string.Join(", ", properties.Select(i => i.Key + ": " + i.Value));
            }

            return s;
        }

        public abstract string GetDeveloperLogs();

        private string _lastPageName;
        private object _pageViewLock = new object();
        private TelemetryPageViewBundler _pageViewBundler;
        public virtual void TrackPageVisited(string pageName)
        {
            try
            {
                _lastPageName = pageName;

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
                    if (_pageViewBundler != null)
                    {
                        FinishPageViewBundler();
                    }
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
            string e = EventToString(eventName, properties);

            System.Diagnostics.Debug.WriteLine(e);
            _developerLogs.Add(e);
        }

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null, IDictionary<string, string> properties = null)
        {
            string e = ExceptionToString(ex, exceptionName, properties);

            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            _developerLogs.Add(e);
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

        private List<string> _developerLogs = new List<string>();
        public override string GetDeveloperLogs()
        {
            return string.Join("\n", _developerLogs);
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
