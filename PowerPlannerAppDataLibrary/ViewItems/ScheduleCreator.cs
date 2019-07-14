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
    public class ScheduleCreator : BindableBase, IEquatable<ViewItemSchedule>
    {
        private static TimeSpan PREVIOUS_START = new TimeSpan(8, 0, 0);
        private static TimeSpan PREVIOUS_END = new TimeSpan(8, 50, 0);

        public ScheduleCreator()
        {
            DayOfWeeks = new List<DayOfWeek>() { DayOfWeek.Monday };
            startTime = PREVIOUS_START;
            endTime = PREVIOUS_END;
            week = Schedule.Week.BothWeeks;
        }

        /// <summary>
        /// Copies the data from the schedule and stores it in the existing schedules
        /// </summary>
        /// <param name="s"></param>
        public ScheduleCreator(ViewItemSchedule s)
        {
            DayOfWeeks = new List<DayOfWeek>() { s.DayOfWeek };
            startTime = s.StartTime.TimeOfDay;
            endTime = s.EndTime.TimeOfDay;
            room = s.Room;
            week = s.ScheduleWeek;

            existingSchedules.Add(s);
        }

        /// <summary>
        /// If the schedule has the same start time, end time, room, and week, the schedule's day will be added and it'll return true. Else returns false.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Add(ViewItemSchedule s)
        {
            if (this.Equals(s))
            {
                if (!DayOfWeeks.Contains(s.DayOfWeek))
                    DayOfWeeks.Add(s.DayOfWeek);

                existingSchedules.Add(s);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Call this AFTER GetUpdatedSchedules(). Sets DayOfWeek, StartTime, EndTime, Room, and Week. Does not set Teachers or UpperPerm/TempIndex.
        /// </summary>
        /// <returns></returns>
        public List<DataItemSchedule> GetNewSchedules()
        {
            PREVIOUS_START = StartTime;
            PREVIOUS_END = EndTime;

            List<DataItemSchedule> schedules = new List<DataItemSchedule>();

            for (int i = 0; i < DayOfWeeks.Count; i++)
            {
                schedules.Add(new DataItemSchedule()
                {
                    Identifier = Guid.NewGuid(),
                    DayOfWeek = DayOfWeeks[i],
                    StartTime = asUtc(StartTime),
                    EndTime = asUtc(EndTime),
                    Room = Room,
                    ScheduleWeek = Week
                });
            }

            return schedules;
        }

        /// <summary>
        /// Call this AFTER GetUpdatedSchedules().
        /// </summary>
        /// <returns></returns>
        public List<ViewItemSchedule> GetDeletedSchedules()
        {
            return existingSchedules;
        }

        /// <summary>
        /// Call this BEFORE GetDeletedSchedules() and GetUpdatedSchedules(). This will mutate the existingSchedules list, so that it only contains the deleted schedules after it's done. 
        /// It also mutates DayOfWeeks, removing each DayOfWeek as it's found.
        /// </summary>
        /// <returns></returns>
        public List<DataItemSchedule> GetUpdatedSchedules()
        {
            List<DataItemSchedule> answer = new List<DataItemSchedule>();

            for (int i = 0; i < DayOfWeeks.Count && existingSchedules.Count > 0; i++)
                for (int x = 0; x < existingSchedules.Count; x++)
                    if (existingSchedules[x].DayOfWeek == DayOfWeeks[i])
                    {
                        //if it was actually changed
                        if (existingSchedules[x].StartTime.TimeOfDay != StartTime ||
                            existingSchedules[x].EndTime.TimeOfDay != EndTime ||
                            !existingSchedules[x].Room.Equals(Room) ||
                            existingSchedules[x].ScheduleWeek != Week)
                        {
                            DataItemSchedule schedule = new DataItemSchedule()
                            {
                                Identifier = existingSchedules[x].Identifier
                            };

                            //copy in the new info
                            InitializeInfo(schedule);

                            answer.Add(schedule);
                        }

                        existingSchedules.RemoveAt(x);
                        DayOfWeeks.RemoveAt(i);
                        i--;
                        break;
                    }

            return answer;
        }

        private void InitializeInfo(DataItemSchedule dataSchedule)
        {
            dataSchedule.StartTime = asUtc(StartTime);
            dataSchedule.EndTime = asUtc(EndTime);
            dataSchedule.Room = Room;
            dataSchedule.ScheduleWeek = Week;
        }

        /// <summary>
        /// Returns a list of the schedules as it stands right now.
        /// </summary>
        /// <returns></returns>
        public List<ViewItemSchedule> PeekCurrentSchedules()
        {
            List<ViewItemSchedule> answer = new List<ViewItemSchedule>();

            foreach (var dayOfWeek in DayOfWeeks)
            {
                DataItemSchedule dataSchedule = new DataItemSchedule();
                InitializeInfo(dataSchedule);

                answer.Add(new ViewItemSchedule(dataSchedule));
            }

            return answer;
        }

        private DateTime asUtc(TimeSpan time)
        {
            return DateTime.SpecifyKind(DateTime.Today.Add(time), DateTimeKind.Utc);
        }

        private List<ViewItemSchedule> existingSchedules = new List<ViewItemSchedule>();
        public List<ViewItemSchedule> ExistingSchedules { get { return existingSchedules; } }

        private List<DayOfWeek> _dayOfWeeks;
        public List<DayOfWeek> DayOfWeeks
        {
            get { return _dayOfWeeks; }
            set { SetProperty(ref _dayOfWeeks, value, "DayOfWeeks"); }
        }

        private TimeSpan startTime;
        public TimeSpan StartTime
        {
            get { return startTime; }
            set
            {
                TimeSpan newEndTime = value.Add(endTime - startTime);
                startTime = value;

                EndTime = newEndTime;

                OnPropertyChanged("StartTime");
            }
        }

        private TimeSpan endTime;
        public TimeSpan EndTime
        {
            get { return endTime; }

            set
            {
                //if end time went below start time
                if (startTime > value)
                {
                    startTime = value.Subtract(EndTime - startTime);

                    if (startTime > value)
                        startTime = new TimeSpan();

                    OnPropertyChanged("StartTime");
                }

                endTime = value;
                OnPropertyChanged("EndTime");
            }
        }

        private string room = "";
        public string Room
        {
            get { return room; }
            set { SetProperty(ref room, value, "Room"); }
        }

        private Schedule.Week week;
        public Schedule.Week Week
        {
            get { return week; }
            set { SetProperty(ref week, value, "Week"); }
        }

        public bool Equals(ViewItemSchedule other)
        {
            return StartTime == other.StartTime.TimeOfDay && EndTime == other.EndTime.TimeOfDay && Room.Equals(other.Room) && Week == other.ScheduleWeek;
        }

        public static bool SharesSameEditingValues(ViewItemSchedule one, ViewItemSchedule two)
        {
            return one.StartTime.TimeOfDay == two.StartTime.TimeOfDay
                && one.EndTime.TimeOfDay == two.EndTime.TimeOfDay
                && one.Room.Equals(two.Room)
                && one.ScheduleWeek == two.ScheduleWeek;
        }
    }
}
