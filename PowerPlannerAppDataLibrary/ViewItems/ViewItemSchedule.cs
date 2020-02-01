using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemSchedule : BaseViewItemWithImages, IComparable<ViewItemSchedule>, IComparable
    {
        public ViewItemSchedule(DataItemSchedule dataItem) : base(dataItem) { }

        public new DataItemSchedule DataItem
        {
            get { return base.DataItem as DataItemSchedule; }
        }

        public ViewItemClass Class { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Room { get; set; }

        public Schedule.Week ScheduleWeek { get; set; }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemSchedule schedule = (DataItemSchedule)dataItem;

            DayOfWeek = schedule.DayOfWeek;
            StartTime = DateTime.SpecifyKind(schedule.StartTime, DateTimeKind.Local);
            EndTime = DateTime.SpecifyKind(schedule.EndTime, DateTimeKind.Local);
            Room = schedule.Room;
            ScheduleWeek = schedule.ScheduleWeek;
        }

        public override int CompareTo(object obj)
        {
            if (obj is ViewItemSchedule)
                return CompareTo(obj as ViewItemSchedule);

            return base.CompareTo(obj);
        }

        public int CompareTo(ViewItemSchedule other)
        {
            if (DayOfWeek < other.DayOfWeek)
                return -1;

            if (DayOfWeek == other.DayOfWeek)
            {
                if (StartTime.TimeOfDay < other.StartTime.TimeOfDay)
                    return -1;
                if (StartTime.TimeOfDay == other.StartTime.TimeOfDay)
                {
                    if (EndTime.TimeOfDay <= other.EndTime.TimeOfDay)
                        return -1;
                    return 1;
                }
                return 1;
            }

            return 1;
        }
    }
}
