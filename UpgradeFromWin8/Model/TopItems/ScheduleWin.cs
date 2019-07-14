using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class ScheduleWin : BaseItemWithImagesWin, IComparable
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Schedule; }
        }

        private DayOfWeek _dayOfWeek;
        [DataMember]
        public DayOfWeek DayOfWeek
        {
            get { return _dayOfWeek; }
            set { SetProperty(ref _dayOfWeek, value, "DayOfWeek"); }
        }

        private DateTime _startTime;
        [DataMember]
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, DateTime.SpecifyKind(value, DateTimeKind.Utc), "StartTime"); }
        }

        private DateTime _endTime;
        [DataMember]
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, DateTime.SpecifyKind(value, DateTimeKind.Utc), "EndTime"); }
        }

        private string _room;
        [DataMember]
        public string Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value, "Room"); }
        }

        private Schedule.Week _scheduleWeek;
        [DataMember]
        public Schedule.Week ScheduleWeek
        {
            get { return _scheduleWeek; }
            set { SetProperty(ref _scheduleWeek, value, "ScheduleWeek"); }
        }

        private Schedule.Type _scheduleType;
        [DataMember]
        public Schedule.Type ScheduleType
        {
            get { return _scheduleType; }
            set { SetProperty(ref _scheduleType, value, "ScheduleType"); }
        }

        private double _locationLatitude;
        [DataMember]
        public double LocationLatitude
        {
            get { return _locationLatitude; }
            set { SetProperty(ref _locationLatitude, value, "LocationLatitude"); }
        }

        private double _locationLongitude;
        [DataMember]
        public double LocationLongitude
        {
            get { return _locationLongitude; }
            set { SetProperty(ref _locationLongitude, value, "LocationLongitude"); }
        }

        //private ObservableEntityList<TeacherUnderScheduleWin, BaseItemWin> _teacherUnderSchedules;
        //private MyObservableList<TeacherUnderScheduleWin> _teacherUnderSchedules;
        //public MyObservableList<TeacherUnderScheduleWin> TeacherUnderSchedules
        //{
        //    get
        //    {
        //        if (_teacherUnderSchedules == null)
        //            _teacherUnderSchedules = GetChildren<TeacherUnderScheduleWin>(false);

        //        return _teacherUnderSchedules;
        //    }
        //}

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.DayOfWeek.ToString()] = DayOfWeek;
            into[PropertyNames.StartTime.ToString()] = StartTime;
            into[PropertyNames.EndTime.ToString()] = EndTime;
            into[PropertyNames.Room.ToString()] = Room;
            into[PropertyNames.ScheduleWeek.ToString()] = ScheduleWeek;
            into[PropertyNames.ScheduleType.ToString()] = ScheduleType;
            into[PropertyNames.LocationLatitude.ToString()] = LocationLatitude;
            into[PropertyNames.LocationLongitude.ToString()] = LocationLongitude;

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

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.DayOfWeek:
                    return DayOfWeek;

                case PropertyNames.StartTime:
                    return StartTime;

                case PropertyNames.EndTime:
                    return EndTime;

                case PropertyNames.Room:
                    return Room;

                case PropertyNames.ScheduleWeek:
                    return ScheduleWeek;

                case PropertyNames.ScheduleType:
                    return ScheduleType;

                case PropertyNames.LocationLatitude:
                    return LocationLatitude;

                case PropertyNames.LocationLongitude:
                    return LocationLongitude;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<BaseItemWin.PropertyNames> changedProperties)
        {
            Schedule from = item as Schedule;

            if (changedProperties != null)
            {
                if (DayOfWeek != from.DayOfWeek)
                    changedProperties.Add(BaseItemWin.PropertyNames.DayOfWeek);

                if (StartTime != from.StartTime.ToUniversalTime())
                    changedProperties.Add(BaseItemWin.PropertyNames.StartTime);

                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(BaseItemWin.PropertyNames.EndTime);

                if (!Room.Equals(from.Room))
                    changedProperties.Add(BaseItemWin.PropertyNames.Room);

                if (ScheduleWeek != from.ScheduleWeek)
                    changedProperties.Add(BaseItemWin.PropertyNames.ScheduleWeek);

                if (ScheduleType != from.ScheduleType)
                    changedProperties.Add(BaseItemWin.PropertyNames.ScheduleType);

                if (LocationLatitude != from.LocationLatitude)
                    changedProperties.Add(BaseItemWin.PropertyNames.LocationLatitude);

                if (LocationLongitude != from.LocationLongitude)
                    changedProperties.Add(BaseItemWin.PropertyNames.LocationLongitude);
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



        public override int CompareTo(object obj)
        {
            if (obj is ScheduleWin)
            {
                ScheduleWin other = obj as ScheduleWin;

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

            return 0;
        }

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return new BaseItemWin[0];
        }

        internal override void Remove(BaseItemWin item)
        {
            //teachers aren't supported yet
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWin item)
        {
            throw new NotImplementedException();
        }
    }
}
