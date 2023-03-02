using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ScheduleCreator
    {
        public static bool SharesSameEditingValues(ViewItemSchedule one, ViewItemSchedule two)
        {
            return one.StartTimeInSchoolTime.TimeOfDay == two.StartTimeInSchoolTime.TimeOfDay
                && one.EndTimeInSchoolTime.TimeOfDay == two.EndTimeInSchoolTime.TimeOfDay
                && one.Room.Equals(two.Room)
                && one.ScheduleWeek == two.ScheduleWeek;
        }
    }
}
