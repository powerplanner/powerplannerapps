using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public class TimeTracker
    {
        private DateTime StartTime { get; set; } = DateTime.UtcNow;

        private TimeTracker()
        {

        }

        public static TimeTracker Start()
        {
            return new TimeTracker();
        }

        public void End(double secondsTooLong, Func<string> actionGenerateMessage)
        {
            double totalSeconds = (DateTime.UtcNow - StartTime).TotalSeconds;
            if (totalSeconds >= secondsTooLong)
            {
                TelemetryExtension.Current?.TrackEvent("OperationTooLong", new Dictionary<string, string>()
                {
                    { "Message", actionGenerateMessage() },
                    { "Duration", totalSeconds.ToString("0") }
                });
            }
        }

        public void End(double secondsTooLong, string message)
        {
            End(secondsTooLong, delegate { return message; });
        }
    }
}
