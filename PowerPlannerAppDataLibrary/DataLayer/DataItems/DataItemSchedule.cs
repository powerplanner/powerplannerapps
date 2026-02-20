using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemSchedule : BaseDataItemWithImages, IComparable
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Schedule; }
        }

        public static readonly DataItemProperty DayOfWeekProperty = DataItemProperty.Register(SyncPropertyNames.DayOfWeek);

        [Column("DayOfWeek")]
        public DayOfWeek DayOfWeek
        {
            get { return GetValue<DayOfWeek>(DayOfWeekProperty); }
            set { SetValue(DayOfWeekProperty, value); }
        }

        public static readonly DataItemProperty StartTimeProperty = DataItemProperty.Register(SyncPropertyNames.StartTime);

        [Column("StartTime")]
        public DateTime StartTime
        {
            get { return GetValue<DateTime>(StartTimeProperty, SqlDate.MinValue); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DataItemProperty EndTimeProperty = DataItemProperty.Register(SyncPropertyNames.EndTime);

        [Column("EndTime")]
        public DateTime EndTime
        {
            get { return GetValue<DateTime>(EndTimeProperty, SqlDate.MinValue); }
            set { SetValue(EndTimeProperty, value); }
        }

        /// <summary>
        /// Returns true if EndTime is greater than StartTime
        /// </summary>
        /// <returns></returns>
        public bool AreTimesValid()
        {
            return EndTime.TimeOfDay > StartTime.TimeOfDay;
        }

        public static readonly DataItemProperty RoomProperty = DataItemProperty.Register(SyncPropertyNames.Room);

        [Column("Room")]
        public string Room
        {
            get { return GetValue<string>(RoomProperty, ""); }
            set { SetValue(RoomProperty, value); }
        }

        public static readonly DataItemProperty ScheduleWeekProperty = DataItemProperty.Register(SyncPropertyNames.ScheduleWeek);

        [Column("ScheduleWeek")]
        public Schedule.Week ScheduleWeek
        {
            get { return GetValue<Schedule.Week>(ScheduleWeekProperty); }
            set { SetValue(ScheduleWeekProperty, value); }
        }

        public static readonly DataItemProperty ScheduleTypeProperty = DataItemProperty.Register(SyncPropertyNames.ScheduleType);

        [Column("ScheduleType")]
        public Schedule.Type ScheduleType
        {
            get { return GetValue<Schedule.Type>(ScheduleTypeProperty); }
            set { SetValue(ScheduleTypeProperty, value); }
        }

        public static readonly DataItemProperty LocationLatitudeProperty = DataItemProperty.Register(SyncPropertyNames.LocationLatitude);

        [Column("LocationLatitude")]
        public double LocationLatitude
        {
            get { return GetValue<double>(LocationLatitudeProperty); }
            set { SetValue(LocationLatitudeProperty, value); }
        }

        public static readonly DataItemProperty LocationLongitudeProperty = DataItemProperty.Register(SyncPropertyNames.LocationLongitude);

        [Column("LocationLongitude")]
        public double LocationLongitude
        {
            get { return GetValue<double>(LocationLongitudeProperty); }
            set { SetValue(LocationLongitudeProperty, value); }
        }

        #region AppointmentLocalId

        [Column("AppointmentLocalId")]
        public string AppointmentLocalId
        {
            get;
            set;
        }

        #endregion


        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.DayOfWeek.ToString()] = DayOfWeek;
            into[SyncPropertyNames.StartTime.ToString()] = StartTime;
            into[SyncPropertyNames.EndTime.ToString()] = EndTime;
            into[SyncPropertyNames.Room.ToString()] = Room;
            into[SyncPropertyNames.ScheduleWeek.ToString()] = ScheduleWeek;
            into[SyncPropertyNames.ScheduleType.ToString()] = ScheduleType;
            into[SyncPropertyNames.LocationLatitude.ToString()] = LocationLatitude;
            into[SyncPropertyNames.LocationLongitude.ToString()] = LocationLongitude;

            base.serialize(into);
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            Schedule into = new Schedule()
            {
                DayOfWeek = DayOfWeek,
                StartTime = StartTime,
                EndTime = EndTime,
                Room = Room,
                ScheduleWeek = ScheduleWeek,
                ScheduleType = ScheduleType,
                LocationLatitude = LocationLatitude,
                LocationLongitude = LocationLongitude
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.DayOfWeek:
                    return DayOfWeek;

                case SyncPropertyNames.StartTime:
                    return StartTime;

                case SyncPropertyNames.EndTime:
                    return EndTime;

                case SyncPropertyNames.Room:
                    return Room;

                case SyncPropertyNames.ScheduleWeek:
                    return ScheduleWeek;

                case SyncPropertyNames.ScheduleType:
                    return ScheduleType;

                case SyncPropertyNames.LocationLatitude:
                    return LocationLatitude;

                case SyncPropertyNames.LocationLongitude:
                    return LocationLongitude;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Schedule from = item as Schedule;

            if (changedProperties != null)
            {
                if (DayOfWeek != from.DayOfWeek)
                    changedProperties.Add(SyncPropertyNames.DayOfWeek);

                if (StartTime != from.StartTime.ToUniversalTime())
                    changedProperties.Add(SyncPropertyNames.StartTime);

                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(SyncPropertyNames.EndTime);

                if (!Room.Equals(from.Room))
                    changedProperties.Add(SyncPropertyNames.Room);

                if (ScheduleWeek != from.ScheduleWeek)
                    changedProperties.Add(SyncPropertyNames.ScheduleWeek);

                if (ScheduleType != from.ScheduleType)
                    changedProperties.Add(SyncPropertyNames.ScheduleType);

                if (LocationLatitude != from.LocationLatitude)
                    changedProperties.Add(SyncPropertyNames.LocationLatitude);

                if (LocationLongitude != from.LocationLongitude)
                    changedProperties.Add(SyncPropertyNames.LocationLongitude);
            }

            DayOfWeek = from.DayOfWeek;
            StartTime = from.StartTime.ToUniversalTime();
            EndTime = from.EndTime.ToUniversalTime();
            Room = from.Room;
            ScheduleWeek = from.ScheduleWeek;
            ScheduleType = from.ScheduleType;
            LocationLatitude = from.LocationLatitude;
            LocationLongitude = from.LocationLongitude;

            base.deserialize(from, changedProperties);
        }


        public DateTime GetLocalStartDateAndTime(AccountDataItem account, DataItemSemester semester, DataItemClass c)
        {
            DateTime startDate;

            if (!DateValues.IsUnassigned(semester.Start))
                startDate = DateHelpers.ToViewItemTime(account, semester.Start);
            else
                startDate = DateTime.Today.AddYears(-1);

            // If the class has a start date, we use that rather than the semester start date
            if (!DateValues.IsUnassigned(c.StartDate))
            {
                startDate = DateHelpers.ToViewItemTime(account, c.StartDate);
            }

            var currentWeek = account.GetWeekOnDifferentDate(startDate);

            // If the schedule doesn't occur each week
            if (this.ScheduleWeek != Schedule.Week.BothWeeks)
            {
                // If it's on the wrong week, we'll move it forward 7 days
                if (currentWeek != this.ScheduleWeek)
                {
                    startDate = startDate.AddDays(7);
                }
            }

            // Get the date the schedule actually starts on
            startDate = DateTools.Next(this.DayOfWeek, startDate);

            return startDate.Add(this.StartTime.TimeOfDay);
        }
    }
}
