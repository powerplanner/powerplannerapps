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

        private ViewItemClass _class;
        public ViewItemClass Class
        {
            get { return _class; }
            set { SetProperty(ref _class, value, "Class"); }
        }

        private DayOfWeek _dayOfWeek;
        public DayOfWeek DayOfWeek
        {
            get { return _dayOfWeek; }
            set { SetProperty(ref _dayOfWeek, value, "DayOfWeek"); }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value, "StartTime"); }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value, "EndTime"); }
        }

        private string _room;
        public string Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value, "Room"); }
        }

        private Schedule.Week _scheduleWeek;
        public Schedule.Week ScheduleWeek
        {
            get { return _scheduleWeek; }
            set { SetProperty(ref _scheduleWeek, value, "ScheduleWeek"); }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemSchedule schedule = (DataItemSchedule)dataItem;

            DayOfWeek = schedule.DayOfWeek;
            StartTime = ToViewItemTime(schedule.StartTime);
            EndTime = ToViewItemTime(schedule.EndTime);
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
