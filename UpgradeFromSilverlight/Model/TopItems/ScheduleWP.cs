using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class ScheduleWP : BaseItemWithImagesWP, IComparable
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Schedule; }
        }
        
        [DataMember]
        public DayOfWeek DayOfWeek { get; set; }
        
        [DataMember]
        public DateTime StartTime { get; set; }
        
        [DataMember]
        public DateTime EndTime { get; set; }
        
        [DataMember]
        public string Room { get; set; }
        
        [DataMember]
        public Schedule.Week ScheduleWeek { get; set; }
        
        [DataMember]
        public Schedule.Type ScheduleType { get; set; }
        
        [DataMember]
        public double LocationLatitude { get; set; }
        
        [DataMember]
        public double LocationLongitude { get; set; }

        protected override PowerPlannerSending.BaseItem serialize(int offset)
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

            base.serialize(into, offset);

            return into;
        }

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

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            Schedule from = item as Schedule;

            if (changedProperties != null)
            {
                if (DayOfWeek != from.DayOfWeek)
                    changedProperties.Add(PropertyNames.DayOfWeek);

                if (StartTime != from.StartTime.ToUniversalTime())
                    changedProperties.Add(PropertyNames.StartTime);

                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(PropertyNames.EndTime);

                if (!Room.Equals(from.Room))
                    changedProperties.Add(PropertyNames.Room);

                if (ScheduleWeek != from.ScheduleWeek)
                    changedProperties.Add(PropertyNames.ScheduleWeek);

                if (ScheduleType != from.ScheduleType)
                    changedProperties.Add(PropertyNames.ScheduleType);

                if (LocationLatitude != from.LocationLatitude)
                    changedProperties.Add(PropertyNames.LocationLatitude);

                if (LocationLongitude != from.LocationLongitude)
                    changedProperties.Add(PropertyNames.LocationLongitude);
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



        public override int CompareTo(object obj)
        {
            if (obj is ScheduleWP)
            {
                ScheduleWP other = obj as ScheduleWP;

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

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return new BaseItemWP[0];
        }

        internal override void Remove(BaseItemWP item)
        {
            //teachers aren't supported yet
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            throw new NotImplementedException();
        }
    }
}
