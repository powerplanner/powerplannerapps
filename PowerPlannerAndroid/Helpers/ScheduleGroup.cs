using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Extensions;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public class ScheduleGroup
    {
        public ViewItemSchedule[] Schedules { get; private set; }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public string Room { get; private set; }
        public DayOfWeek[] DayOfWeeks { get; private set; }

        public string DaysText { get; private set; }

        public string TimeText { get; private set; }

        public ScheduleGroup(ViewItemSchedule[] schedules)
        {
            var s = schedules[0];

            // Used for editing, so using school time
            StartTime = s.StartTimeInSchoolTime.TimeOfDay;
            EndTime = s.EndTimeInSchoolTime.TimeOfDay;
            Room = s.Room;
            DayOfWeeks = schedules.Select(i => i.DayOfWeek).Distinct().OrderBy(i => i).ToArray();

            DaysText = string.Join(", ", DayOfWeeks.Select(i => DateTools.ToLocalizedString(i)));
            TimeText = PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(s.StartTimeInSchoolTime), DateTimeFormatterExtension.Current.FormatAsShortTime(s.EndTimeInSchoolTime));
        }
    }
}