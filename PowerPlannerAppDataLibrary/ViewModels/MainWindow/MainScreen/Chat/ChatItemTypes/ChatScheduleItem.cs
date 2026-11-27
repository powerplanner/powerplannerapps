using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatScheduleItem : BaseChatItem
    {
        [Description("The day of week the schedule occurs on.")]
        public DayOfWeek DayOfWeek { get; set; }

        [Description("The time of day that the schedule starts.")]
        public TimeOnly StartTime { get; set; }

        [Description("The time of day that the schedule ends.")]
        public TimeOnly EndTime { get; set; }

        [Description("Whether the schedule is weekly, or bi-weekly on week one/A or week two/B.")]
        public PowerPlannerSending.Schedule.Week WeekType { get; set; }

        [Description("Optional room of the schedule (or a URL if it's an online class).")]
        public string Room { get; set; }

        public ChatScheduleItem(ViewItemSchedule schedule) : base(schedule)
        {
            DayOfWeek = schedule.DayOfWeek;
            StartTime = TimeOnly.FromDateTime(schedule.StartTimeInSchoolTime);
            EndTime = TimeOnly.FromDateTime(schedule.EndTimeInSchoolTime);
            WeekType = schedule.ScheduleWeek;
            Room = schedule.Room;
        }
    }
}
