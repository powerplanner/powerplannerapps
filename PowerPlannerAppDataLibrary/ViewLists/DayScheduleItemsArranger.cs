using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewLists
{
    public class DayScheduleItemsArranger : BindableBase
    {
        public double HeightOfHour { get; private set; }

        public DateTime Date { get; private set; }

        /// <summary>
        /// The overall start time
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The overall end time
        /// </summary>
        public TimeSpan EndTime { get; set; }

        public TimeSpan MinDuration { get; private set; }

        public List<ViewItemTaskOrEvent> AllDayItems { get; private set; }

        public List<ViewItemHoliday> Holidays { get; private set; }

        private List<object> _cachedHolidayAndAllDayItems;
        public List<object> HolidayAndAllDayItems
        {
            get
            {
                if (_cachedHolidayAndAllDayItems == null)
                {
                    var answer = new List<object>();
                    answer.AddRange(Holidays);
                    answer.AddRange(AllDayItems);
                    _cachedHolidayAndAllDayItems = answer;
                }
                return _cachedHolidayAndAllDayItems;
            }
        }

        private bool _hasHolidays;
        public bool HasHolidays
        {
            get { return _hasHolidays; }
            private set { SetProperty(ref _hasHolidays, value, nameof(HasHolidays)); }
        }

        public bool IsDifferentSemester { get; private set; }

        public event EventHandler OnItemsChanged;

        public class BaseScheduleItem
        {
            public double TopOffset { get; private set; }
            public double LeftOffset { get; set; }
            public double Height { get; private set; }

            public int Column { get; set; }

            public int NumOfColumns { get; set; } = 1;

            public DayScheduleItemsArranger Arranger { get; private set; }

            public BaseScheduleItem(DayScheduleItemsArranger arranger)
            {
                Arranger = arranger;
            }

            /// <summary>
            /// Note that the StartTime is for rendering layout purposes only, and might not correspond to the actual start time of the event.
            /// </summary>
            public TimeSpan StartTime { get; set; }
            /// <summary>
            /// Note that the EndTime is for rendering layout purposes only, and might not correspond to the actual end time of the event.
            /// </summary>
            public TimeSpan EndTime { get; set; }

            public void CalculateOffsets()
            {
                TopOffset = Arranger.HeightOfHour * (StartTime - Arranger.StartTime).TotalHours;
                Height = Arranger.HeightOfHour * (EndTime - StartTime).TotalHours;
            }

            public bool CollidesWith(BaseScheduleItem other)
            {
                var paddedStartTime = StartTime;
                var paddedEndTime = EndTime;

                //if (this.GetType() != other.GetType())
                //{
                //    paddedStartTime = StartTime.Subtract(TimeSpan.FromMinutes(10));
                //    paddedEndTime = EndTime.Add(TimeSpan.FromMinutes(10));
                //}

                // AAAA
                //      BBBB

                // AAAAAAA
                //      BBBB

                // AAAAAAAAAAAAA
                //      BBBB

                //       AA
                //      BBBB

                // Easier to determine if it does NOT collide
                bool doesNotCollide = paddedEndTime <= other.StartTime || paddedStartTime >= other.EndTime;

                return !doesNotCollide;
            }
        }

        public class ScheduleItem : BaseScheduleItem
        {
            public ViewItemSchedule Item { get; private set; }

            public ScheduleItem(DayScheduleItemsArranger arranger, ViewItemSchedule viewItem) : base(arranger)
            {
                Item = viewItem;

                StartTime = viewItem.StartTime.TimeOfDay;
                EndTime = viewItem.EndTime.TimeOfDay;
            }
        }

        public class EventItem : BaseScheduleItem
        {
            public bool IsCollapsedMode { get; set; }

            public ViewItemTaskOrEvent Item { get; private set; }

            /// <summary>
            /// In the case where IsCollapsedMode is true and there's additional items, this will be initialized with the additional items
            /// </summary>
            public List<ViewItemTaskOrEvent> AdditionalItems { get; private set; }

            public EventItem(DayScheduleItemsArranger arranger, ViewItemTaskOrEvent item) : base(arranger)
            {
                Item = item;

                StartTime = item.GetDueDateWithTime().TimeOfDay;
                DateTime endTime;
                if (item.TryGetEndDateWithTime(out endTime))
                {
                    EndTime = endTime.TimeOfDay;
                }

                if (EndTime - arranger.MinDuration < StartTime)
                {
                    EndTime = StartTime + arranger.MinDuration;

                    // If that pushes it past 24 hours
                    if (EndTime >= new TimeSpan(24, 0, 0))
                    {
                        // Move the start time up to accomodate the min duration
                        EndTime = new TimeSpan(23, 59, 0);
                        StartTime = EndTime - arranger.MinDuration;
                    }
                }
            }

            public void AddAdditionalItem(EventItem item)
            {
                if (AdditionalItems == null)
                {
                    AdditionalItems = new List<ViewItemTaskOrEvent>();
                }

                AdditionalItems.Add(item.Item);

                if (item.EndTime > EndTime)
                {
                    EndTime = item.EndTime;
                }
            }

            public bool CanExpand()
            {
                return IsCollapsedMode || (AdditionalItems != null && AdditionalItems.Count > 0);
            }
        }

        public ScheduleItem[] ScheduleItems { get; private set; }

        public EventItem[] EventItems { get; private set; }

        public AccountDataItem Account { get; private set; }

        private SemesterItemsViewGroup _semesterItems;
        private SchedulesOnDay _schedules;
        private MyObservableList<BaseViewItemMegaItem> _events;
        private MyObservableList<ViewItemHoliday> _holidays;
        private double _spacingWhenNoAdditionalItems;
        private double _spacingWithAdditionalItems;
        private double _widthOfCollapsed;
        private TimeZoneInfo _schoolTimeZone;

        private DayScheduleItemsArranger(AccountDataItem account, SemesterItemsViewGroup semesterItems, ScheduleViewItemsGroup scheduleGroup, DateTime date, double heightOfHour, double spacingWhenNoAdditionalItems, double spacingWithAdditionalItems, double widthOfCollapsed, bool includeTasksAndEventsAndHolidays)
        {
            if (semesterItems.Semester == null)
            {
                throw new NullReferenceException("Semester was null");
            }

            _semesterItems = semesterItems;
            semesterItems.OnItemsChanged += new WeakEventHandler<EventArgs>(SemesterGroup_OnItemsChanged).Handler;
            scheduleGroup.OnChangesOccurred += new WeakEventHandler<DataChangedEvent>(ScheduleViewItemsGroup_OnChangesOccurred).Handler;

            Date = date;
            _spacingWhenNoAdditionalItems = spacingWhenNoAdditionalItems;
            _spacingWithAdditionalItems = spacingWithAdditionalItems;
            _widthOfCollapsed = widthOfCollapsed;
            Account = account;
            _schoolTimeZone = account.SchoolTimeZone;
            HeightOfHour = heightOfHour;
            MinDuration = TimeSpan.FromHours(widthOfCollapsed / HeightOfHour);

            IsDifferentSemester = !semesterItems.Semester.IsDateDuringThisSemester(date);

            SchedulesOnDay schedules = null;

            if (!IsDifferentSemester)
            {
                schedules = SchedulesOnDay.Get(semesterItems.Classes, date, account.GetWeekOnDifferentDate(date), trackChanges: true);
                schedules.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Schedules_CollectionChanged).Handler;
            }

            _schedules = schedules;

            if (includeTasksAndEventsAndHolidays)
            {
                // For schedules, if tasks have a specific due time, we don't adjust the dates regardless of time zones causing them to switch to a different day, since they're displayed on the visual schedule. Therefore we set useEffectiveDatesEvenWhenItemsHaveTimes to false.
                _events = TasksOrEventsOnDay.Get(account, semesterItems.Items, date, today: null, activeOnly: false, useEffectiveDateEvenWhenItemsHaveTimes: false);
                _events.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Events_CollectionChanged).Handler;
            }
            else
            {
                _events = new MyObservableList<BaseViewItemMegaItem>();
            }

            if (includeTasksAndEventsAndHolidays)
            {
                _holidays = HolidaysOnDay.Create(semesterItems.Items, date);
                _holidays.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(_holidays_CollectionChanged).Handler;
            }
            else
            {
                _holidays = new MyObservableList<ViewItemHoliday>();
            }

            Initialize(schedules, _events, _holidays);
        }

        private void ScheduleViewItemsGroup_OnChangesOccurred(object sender, DataChangedEvent e)
        {
            EnsureChangesPropagated();
        }

        private void SemesterGroup_OnItemsChanged(object sender, EventArgs e)
        {
            EnsureChangesPropagated();
        }

        private void _holidays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _hadChanges = true;
        }

        ~DayScheduleItemsArranger()
        {
            System.Diagnostics.Debug.WriteLine("DayScheduleItemsArranger disposed");
        }

        private void Initialize(SchedulesOnDay schedules, MyObservableList<BaseViewItemMegaItem> events, MyObservableList<ViewItemHoliday> holidays)
        {
            List<ScheduleItem> schedulesCopied;
            if (schedules == null)
            {
                // Different semester case, so no schedules
                schedulesCopied = new List<ScheduleItem>();
            }
            else
            {
                schedulesCopied = schedules.Where(i => i.EndTime.TimeOfDay > i.StartTime.TimeOfDay).Select(i => new ScheduleItem(this, i)).ToList();
            }

            List<EventItem> eventsCopied = new List<EventItem>();
            List<ViewItemTaskOrEvent> allDayEvents = new List<ViewItemTaskOrEvent>();
            foreach (var e in events.OfType<ViewItemTaskOrEvent>())
            {
                if (e.IsDuringDay())
                {
                    eventsCopied.Add(new EventItem(this, e));
                }
                else
                {
                    allDayEvents.Add(e);
                }
            }
            AllDayItems = allDayEvents;

            Holidays = holidays.ToList();
            HasHolidays = Holidays.Any();
            _cachedHolidayAndAllDayItems = null;

            var schedulesFinal = schedulesCopied.ToArray();
            var eventsFinal = eventsCopied.ToList();

            ScheduleItems = schedulesFinal;

            // Handle schedule collisions
            while (schedulesCopied.Count > 0)
            {
                var collidingSchedules = new List<ScheduleItem>() { schedulesCopied[0] };
                schedulesCopied.RemoveAt(0);
                AddColliding(schedulesCopied, collidingSchedules);

                if (collidingSchedules.Count > 1)
                {
                    for (int i = 0; i < collidingSchedules.Count; i++)
                    {
                        collidingSchedules[i].Column = i;
                        collidingSchedules[i].NumOfColumns = collidingSchedules.Count;
                    }
                }
            }

            // Handle event collisions
            while (eventsCopied.Count > 0)
            {
                var collidingEvents = new List<EventItem>() { eventsCopied[0] };
                eventsCopied.RemoveAt(0);
                AddColliding(eventsCopied, collidingEvents);

                List<ScheduleItem> scheduleCollisionsWithEvent = new List<ScheduleItem>();

                // If there's a colliding schedule, we collapse
                bool doesCollideWithSchedule = false;
                foreach (var e in collidingEvents)
                {
                    foreach (var s in schedulesFinal)
                    {
                        if (s.CollidesWith(e))
                        {
                            doesCollideWithSchedule = true;
                            scheduleCollisionsWithEvent.Add(s);
                        }
                        else
                        {
                            AccomodateTouchingItemsIfNeeded(s, e);
                        }
                    }
                }

                if (doesCollideWithSchedule)
                {
                    var firstEvent = collidingEvents[0];
                    firstEvent.IsCollapsedMode = true;
                    foreach (var e in collidingEvents.Skip(1))
                    {
                        firstEvent.AddAdditionalItem(e);
                        eventsFinal.Remove(e);
                    }

                    foreach (var s in scheduleCollisionsWithEvent)
                    {
                        if (firstEvent.AdditionalItems != null)
                        {
                            s.LeftOffset = _spacingWithAdditionalItems;
                        }
                        else
                        {
                            // LeftOffset might have been previously assigned, so make sure we're assigning higher value
                            if (_spacingWhenNoAdditionalItems > s.LeftOffset)
                            {
                                s.LeftOffset = _spacingWhenNoAdditionalItems;
                            }
                        }
                    }
                }
                else if (collidingEvents.Count == 1)
                {
                    // Nothing
                }
                else if (collidingEvents.Count == 2)
                {
                    // Exactly two items
                    collidingEvents[0].NumOfColumns = 2;
                    collidingEvents[1].NumOfColumns = 2;
                    collidingEvents[1].Column = 1;
                }
                else
                {
                    // More than two items
                    EventItem prev = null;
                    bool isLeftSide = true;

                    while (collidingEvents.Count > 0)
                    {
                        var curr = collidingEvents[0];
                        curr.NumOfColumns = 2;
                        collidingEvents.RemoveAt(0);

                        if (prev != null)
                        {
                            if (!isLeftSide)
                            {
                                curr.Column = 1;
                            }

                            // Find out if any items collide with the prev item, and therefore need to become mini-items with the curr item
                            while (collidingEvents.Count > 0)
                            {
                                var next = collidingEvents[0];
                                if (prev.CollidesWith(next))
                                {
                                    collidingEvents.RemoveAt(0);
                                    curr.AddAdditionalItem(next);
                                    eventsFinal.Remove(next);
                                }
                                else
                                {
                                    AccomodateTouchingItemsIfNeeded(prev, next);
                                    break;
                                }
                            }
                        }

                        // Prev becomes curr
                        prev = curr;

                        // And we switch the side
                        isLeftSide = !isLeftSide;
                    }
                }
            }

            EventItems = eventsFinal.ToArray();

            if (ScheduleItems.Any() || EventItems.Any())
            {
                var min = ScheduleItems.OfType<BaseScheduleItem>().Concat(EventItems.OfType<BaseScheduleItem>()).Min(i => i.StartTime);
                if (min.Minutes == 59 && min.Hours != 23)
                {
                    // So that a task that's due before class (1 min before) doesn't cause an entire hour to be rendered, we adjust 0:59 to the next hour
                    // Note that we exclude incrementing 23:59 since that would make it the next day
                    min = min.Add(TimeSpan.FromMinutes(1));
                }
                StartTime = new TimeSpan(min.Hours, 0, 0);
                EndTime = new TimeSpan(ScheduleItems.OfType<BaseScheduleItem>().Concat(EventItems.OfType<BaseScheduleItem>()).Max(i => i.EndTime).Hours, 0, 0);
                if (EndTime < StartTime)
                {
                    EndTime = StartTime.Add(TimeSpan.FromHours(1));
                }
            }

            CalculateOffsets();
        }

        public void CalculateOffsets()
        {
            foreach (var s in ScheduleItems)
            {
                s.CalculateOffsets();
            }
            foreach (var e in EventItems)
            {
                e.CalculateOffsets();
            }
        }

        private bool _hadChanges;
        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _hadChanges = true;
        }

        private void Schedules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _hadChanges = true;
        }

        private static void AddColliding<T>(List<T> copiedList, List<T> into) where T : BaseScheduleItem
        {
            for (int i = 0; i < copiedList.Count; i++)
            {
                if (AddColliding(copiedList[i], into))
                {
                    copiedList.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// If the item is colliding and can't be resolved (like same start time as an end time), adds into the into list and returns true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="incoming"></param>
        /// <param name="into"></param>
        /// <returns></returns>
        private static bool AddColliding<T>(T incoming, List<T> into) where T : BaseScheduleItem
        {
            foreach (var existing in into)
            {
                if (existing.CollidesWith(incoming))
                {
                    into.Add(incoming);
                    return true;
                }

                AccomodateTouchingItemsIfNeeded(incoming, existing);
            }

            return false;
        }

        private static void AccomodateTouchingItemsIfNeeded(BaseScheduleItem first, BaseScheduleItem second)
        {
            const int collisionPadding = 8;

            if (first.StartTime == second.EndTime)
            {
                second.EndTime = second.EndTime.Subtract(TimeSpan.FromMinutes(collisionPadding / 2));
                first.StartTime = first.StartTime.Add(TimeSpan.FromMinutes(collisionPadding / 2));
            }

            else if (first.EndTime == second.StartTime)
            {
                first.EndTime = first.EndTime.Subtract(TimeSpan.FromMinutes(collisionPadding / 2));
                second.StartTime = second.StartTime.Add(TimeSpan.FromMinutes(collisionPadding / 2));
            }
        }

        private static WeakReferenceList<DayScheduleItemsArranger> _cached = new WeakReferenceList<DayScheduleItemsArranger>();

        public static void ClearCached()
        {
            _cached.Clear();
        }

        public static DayScheduleItemsArranger Create(AccountDataItem account, SemesterItemsViewGroup semesterItems, ScheduleViewItemsGroup scheduleGroup, DateTime date, double heightOfHour, double spacingWhenNoAdditionalItems, double spacingWithAdditionalItems, double widthOfCollapsed, bool includeTasksAndEventsAndHolidays)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (semesterItems == null)
            {
                throw new ArgumentNullException(nameof(semesterItems));
            }

            if (scheduleGroup == null)
            {
                throw new ArgumentNullException(nameof(scheduleGroup));
            }

            date = date.Date;

            foreach (var cachedAnswer in _cached)
            {
                if (cachedAnswer.Date == date.Date
                    && cachedAnswer._semesterItems == semesterItems
                    && cachedAnswer.HeightOfHour == heightOfHour
                    && cachedAnswer._spacingWhenNoAdditionalItems == spacingWhenNoAdditionalItems
                    && cachedAnswer._spacingWithAdditionalItems == spacingWithAdditionalItems
                    && cachedAnswer._widthOfCollapsed == widthOfCollapsed
                    && object.Equals(account.SchoolTimeZone, cachedAnswer._schoolTimeZone))
                {
                    return cachedAnswer;
                }
            }

            DayScheduleItemsArranger answer = new DayScheduleItemsArranger(account, semesterItems, scheduleGroup, date, heightOfHour, spacingWhenNoAdditionalItems, spacingWithAdditionalItems, widthOfCollapsed, includeTasksAndEventsAndHolidays);
            _cached.Add(answer);
            return answer;
        }

        /// <summary>
        /// Returns true if the schedule portion is valid and has any items
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return EndTime >= StartTime && (ScheduleItems.Length > 0 || EventItems.Length > 0);
        }

        /// <summary>
        /// Returns true if has any items
        /// </summary>
        /// <returns></returns>
        public bool HasItems()
        {
            return ScheduleItems.Length > 0 || EventItems.Length > 0 || HasHolidays || AllDayItems.Count > 0;
        }

        /// <summary>
        /// Assumes this is being called from UI thread
        /// </summary>
        public void EnsureChangesPropagated()
        {
            try
            {
                // Hopefully this doesn't cause any bugs... the dispatcher has already been called for the Events and Schedules collections,
                // so this dispatcher is being called second, and as long as the order is preserved everything should be good
                if (_hadChanges)
                {
                    Initialize(_schedules, _events, _holidays);
                    _hadChanges = false;
                    OnItemsChanged?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
