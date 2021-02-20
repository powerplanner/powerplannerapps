﻿using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using ToolsPortable;

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

        /// <summary>
        /// Only used by UWP for displaying the preview of schedules on the tile. 
        /// </summary>
        public DayOfWeek DayOfWeekInLocalTime
        {
            get
            {
                var date = DateTools.Next(DayOfWeek, DateTime.Today);
                if (OccursOnDate(date))
                {
                    return date.DayOfWeek;
                }

                // Otherwise, check the next day and the previous day, has to be one of those...
                date = date.AddDays(1);
                if (OccursOnDate(date))
                {
                    return date.DayOfWeek;
                }

                date = date.AddDays(-2);
                if (OccursOnDate(date))
                {
                    return date.DayOfWeek;
                }

                return DayOfWeek;
            }
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value, "StartTime"); }
        }

        private DateTime _startTimeInSchoolTime;
        public DateTime StartTimeInSchoolTime
        {
            get => _startTimeInSchoolTime;
            set => SetProperty(ref _startTimeInSchoolTime, value, nameof(StartTimeInSchoolTime));
        }

        /// <summary>
        /// If the schedule instance occurs on the given (local) day, returns true. Accomodates for time zones including when time zone causes schedule that occurs on Wednesdays to actually be on Tuesday. Does NOT take into consideration week settings.
        /// </summary>
        /// <param name="date">Date to check in local time</param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public bool OccursOnDate(DateTime date)
        {
            // The start date time in local time
            var startDateTime = ToViewItemTime(DateTools.Next(DayOfWeek, DateTime.Today).Add(StartTimeInSchoolTime.TimeOfDay));

            if (date.DayOfWeek == startDateTime.DayOfWeek)
            {
                return true;
            }

            return false;
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value, "EndTime"); }
        }

        private DateTime _endTimeInSchoolTime;
        public DateTime EndTimeInSchoolTime
        {
            get => _endTimeInSchoolTime;
            set => SetProperty(ref _endTimeInSchoolTime, value, nameof(EndTimeInSchoolTime));
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
            StartTimeInSchoolTime = ToViewItemSchoolTime(schedule.StartTime);

            EndTime = ToViewItemTime(schedule.EndTime);
            EndTimeInSchoolTime = ToViewItemSchoolTime(schedule.EndTime);

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
