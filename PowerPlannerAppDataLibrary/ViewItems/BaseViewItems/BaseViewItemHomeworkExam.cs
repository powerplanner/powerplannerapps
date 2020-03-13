using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemHomeworkExam : BaseViewItemHomeworkExamGrade
    {
        /// <summary>
        /// Used by grades view model, so that it knows which type of item to draw, since it's all contained in the same list
        /// </summary>
        public bool IsUnassignedItem { get; set; }

        public BaseViewItemHomeworkExam(DataItemMegaItem dataItem) : base(dataItem) { }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            private set { SetProperty(ref _endTime, value, "EndTime"); }
        }

        private DateTime _endTimeInSchoolTime;
        public DateTime EndTimeInSchoolTime
        {
            get => _endTimeInSchoolTime;
            private set => SetProperty(ref _endTimeInSchoolTime, value, nameof(EndTimeInSchoolTime));
        }

        private DateTime _reminder;
        public DateTime Reminder
        {
            get { return _reminder; }
            private set { SetProperty(ref _reminder, value, "Reminder"); }
        }

        /// <summary>
        /// For internal uses only. The actual WeightCategory object will be assigned by <see cref="ViewItemClass"/>.
        /// </summary>
        internal Guid WeightCategoryIdentifier { get; set; }

        public byte[] ColorWhenInWhatIfMode
        {
            get
            {
                if (WasChanged)
                {
                    var c = GetClassOrNull();
                    if (c != null)
                        return c.Color;
                }

                return new byte[] { 190, 190, 190 };
            }
        }

#if ANDROID
        public bool HasSentReminder { get; private set; }
#endif

        public string ListItemTertiaryText
        {
            get
            {
                string answer = Date.ToString("d");

                if (!string.IsNullOrWhiteSpace(Details))
                    answer += " - " + Details.Trim().Replace("\n", "  ");

                return answer;
            }
        }

        public DataItemMegaItem.TimeOptions TimeOption
        {
            get
            {
                return (DataItem as DataItemMegaItem).TimeOption;
            }
        }

        /// <summary>
        /// Gets the time option with the class schedule factored in
        /// </summary>
        /// <returns></returns>
        public DataItemMegaItem.TimeOptions GetActualTimeOption()
        {
            switch (TimeOption)
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    return DataItemMegaItem.TimeOptions.AllDay;

                case DataItemMegaItem.TimeOptions.Custom:
                    return DataItemMegaItem.TimeOptions.Custom;
            }

            var c = GetClassOrNull();
            if (c == null || c.IsNoClassClass || !PowerPlannerApp.DoesClassOccurOnDate(Account, Date, c))
            {
                return DataItemMegaItem.TimeOptions.AllDay;
            }

            return TimeOption;
        }

        /// <summary>
        /// Returns true if the task occurs at a specific time during the day, rather than at the end of the day
        /// </summary>
        /// <returns></returns>
        public bool IsDuringDay()
        {
            switch (GetActualTimeOption())
            {
                case DataItemMegaItem.TimeOptions.BeforeClass:
                case DataItemMegaItem.TimeOptions.Custom:
                case DataItemMegaItem.TimeOptions.DuringClass:
                case DataItemMegaItem.TimeOptions.EndOfClass:
                case DataItemMegaItem.TimeOptions.StartOfClass:
                    return true;

                default:
                    return false;
            }
        }

        public bool IsComplete()
        {
            if (this is BaseViewItemHomework)
            {
                return (this as BaseViewItemHomework).IsComplete;
            }
            else if (this is ViewItemExam)
            {
                return (this as ViewItemExam).IsComplete;
            }
            return false;
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemMegaItem i = dataItem as DataItemMegaItem;

            if (i.EndTime == PowerPlannerSending.DateValues.UNASSIGNED)
            {
                EndTime = i.EndTime;
                EndTimeInSchoolTime = i.EndTime;
            }
            else
            {
                EndTime = ToViewItemTime(i.EndTime);
                EndTimeInSchoolTime = ToViewItemSchoolTime(i.EndTime);
            }

            // TODO: I probably need to do a similar local conversion for Reminder too when I start using it
            Reminder = i.Reminder;
            WeightCategoryIdentifier = i.WeightCategoryIdentifier;

#if ANDROID
            HasSentReminder = i.HasSentReminder;
#endif
        }

        public ViewItemClass GetClassOrNull()
        {
            if (this is ViewItemHomework)
                return (this as ViewItemHomework).Class;

            if (this is ViewItemExam)
                return (this as ViewItemExam).Class;

            return null;
        }

        public new DataItemMegaItem CreateBlankDataItem()
        {
            if (this is ViewItemExam)
            {
                return new DataItemMegaItem()
                {
                    Identifier = Identifier,
                    MegaItemType = this.GetClassOrNull().IsNoClassClass ? PowerPlannerSending.MegaItemType.Event : PowerPlannerSending.MegaItemType.Exam
                };
            }
            if (this is ViewItemHomework)
            {
                return new DataItemMegaItem()
                {
                    Identifier = Identifier,
                    MegaItemType = this.GetClassOrNull().IsNoClassClass ? PowerPlannerSending.MegaItemType.Task : PowerPlannerSending.MegaItemType.Homework
                };
            }
            throw new NotImplementedException("Unknown type: " + this.GetType());
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is BaseViewItemHomeworkExam)
            {
                return CompareTo(other as BaseViewItemHomeworkExam);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(BaseViewItemHomeworkExam other)
        {
            // Desired sort order:
            //
            // - Anything on a different day obviously goes earlier/later
            //
            // - Within the same day, items that are completed drop to the bottom
            //
            // - Items that have times (or class times) should go first and appear in chronological order
            //
            // - After the items with times, everything else should appear afterwards
            //
            // - Anything that collides at the same time (or at the bottom, or don't have times) are sorted in the following order
            //   - Exam
            //   - Homework
            //   - Task
            //   - Event

            // If earlier date, goes first
            if (this.Date.Date < other.Date.Date)
            {
                return -1;
            }

            // If later date, goes last
            if (this.Date.Date > other.Date.Date)
            {
                return 1;
            }

            // Otherwise, same date, so need to sort based on specific behaviors...

            // Within same day, items that are complete drop to bottom
            int comp = this.TryCompareByCompletionStatus(other);
            if (comp != 0)
            {
                return comp;
            }

            // Place items in chronological order
            comp = this.GetDueDateWithTime().CompareTo(other.GetDueDateWithTime());
            if (comp != 0)
            {
                return comp;
            }

            // Otherwise, items collide at same time (or didn't have time)

            // Compare by type
            comp = this.TryCompareByType(other);
            if (comp != 0)
            {
                return comp;
            }

            // Otherwise, compare by class if present
            var class1 = GetClassOrNull();
            var class2 = GetClassOrNull();

            if (class1 != null && class2 != null)
            {
                comp = class1.CompareTo(class2);
                if (comp != 0)
                {
                    return comp;
                }
            }

            // Otherwise, compare by date created
            return this.DateCreated.CompareTo(other.DateCreated);
        }

        private int TryCompareByType(BaseViewItemHomeworkExam other)
        {
            if (this.GetType() == other.GetType())
            {
                return 0;
            }

            if (this is ViewItemExam)
            {
                // Exams go ahead of everything
                return -1;
            }
            else if (other is ViewItemExam)
            {
                return 1;
            }

            if (this is ViewItemHomework)
            {
                return -1;
            }
            else if (other is ViewItemHomework)
            {
                return 1;
            }

            // Only remaining type is Event. Both items, by definition, must be events.
            // Therefore this code shouldn't even get hit.
            return 0;
        }

        private int TryCompareByCompletionStatus(BaseViewItemHomeworkExam other)
        {
            var thisIsComplete = this.IsComplete();
            var otherIsComplete = other.IsComplete();

            // If other is complete and this isn't, we send this to front
            if (otherIsComplete && !thisIsComplete)
            {
                return -1;
            }

            // If this is complete and other isn't, we send this to back
            if (thisIsComplete && !otherIsComplete)
            {
                return 1;
            }

            // Otherwise, continue to use normal sorting behavior
            return 0;
        }

        private int TryCompareWithDateTimeOtherwiseUseDateCreated(BaseViewItemHomeworkExam other)
        {
            int compare = this.Date.CompareTo(other.Date);

            if (compare != 0)
            {
                return compare;
            }

            return this.DateCreated.CompareTo(other.DateCreated);
        }

        public virtual DateTime GetDayOfReminderTime(out bool hadSpecificTime)
        {
            DateTime answer;
            if (TryGetDueDateWithTime(out answer))
            {
                hadSpecificTime = true;
                return answer.AddHours(-1);
            }

            // Act like it's an all-day item
            if (this is ViewItemHomework)
            {
                hadSpecificTime = false;
                return Date.Date.AddHours(18); // 6:00 PM
            }
            else
            {
                hadSpecificTime = false;
                return Date.Date.AddHours(9); // 9 AM
            }
        }

        /// <summary>
        /// Returns the due (or start) date.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetDueDateWithTime()
        {
            DateTime answer;
            if (TryGetDueDateWithTime(out answer))
            {
                return answer;
            }

            // Act like it's an all-day item
            if (this is ViewItemHomework)
            {
                return Date.Date.AddDays(1).AddSeconds(-1);
            }
            else
            {
                return Date.Date;
            }
        }

        /// <summary>
        /// Returns false if it's an all-day item
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        public virtual bool TryGetDueDateWithTime(out DateTime answer)
        {
            switch (TimeOption)
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    answer = DateTime.MinValue;
                    return false;

                case DataItemMegaItem.TimeOptions.Custom:
                    answer = StripSeconds(Date);
                    return true;
            }

            // Otherwise we need to obtain the class schedule
            var schedule = FindSchedule();

            if (schedule != null)
            {
                switch (TimeOption)
                {
                    case DataItemMegaItem.TimeOptions.BeforeClass:
                        answer = Date.Date.Add(schedule.StartTime.TimeOfDay).AddMinutes(-1);
                        return true;

                    case DataItemMegaItem.TimeOptions.StartOfClass:
                        answer = Date.Date.Add(schedule.StartTime.TimeOfDay);
                        return true;

                    case DataItemMegaItem.TimeOptions.DuringClass:
                        if (this is ViewItemHomework)
                        {
                            answer = Date.Date.Add(schedule.StartTime.TimeOfDay).AddTicks((schedule.EndTime.TimeOfDay.Ticks - schedule.StartTime.TimeOfDay.Ticks) / 2);
                            return true;
                        }
                        else
                        {
                            answer = Date.Date.Add(schedule.StartTime.TimeOfDay);
                            return true;
                        }

                    case DataItemMegaItem.TimeOptions.EndOfClass:
                        answer = Date.Date.Add(schedule.EndTime.TimeOfDay);
                        return true;
                }
            }
            else
            {
                answer = DateTime.MinValue;
                return false;
            }

            // Shouldn't be hit
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            answer = DateTime.MinValue;
            return false;
        }

        public bool TryGetEndDateWithTime(out DateTime dateTime)
        {
            if (this is ViewItemHomework)
            {
                dateTime = DateTime.MinValue;
                return false;
            }

            switch (TimeOption)
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    dateTime = Date.Date.AddDays(1).AddSeconds(-1);
                    return true;

                case DataItemMegaItem.TimeOptions.Custom:
                    dateTime = Date.Date.Add(EndTime.TimeOfDay);
                    return true;
            }

            // Otherwise we need to obtain the class schedule
            var schedule = FindSchedule();

            if (schedule != null)
            {
                switch (TimeOption)
                {
                    case DataItemMegaItem.TimeOptions.DuringClass:
                        dateTime = Date.Date.Add(schedule.EndTime.TimeOfDay);
                        return true;
                }
            }
            else
            {
                // Act like it's an all-day item
                dateTime = Date.Date.AddDays(1).AddSeconds(-1);
                return true;
            }

            dateTime = DateTime.MinValue;
            return false;
        }

        private static DateTime StripSeconds(DateTime date)
        {
            return date.AddSeconds(date.Second * -1);
        }

        private ViewItemSchedule FindSchedule()
        {
            var c = GetClassOrNull();

            return findSchedule(c, Date.Date);
        }

        public string GetSubtitleOrNull()
        {
            if (this is ViewItemHomework)
            {
                return (this as ViewItemHomework).Subtitle;
            }
            else if (this is ViewItemExam)
            {
                return (this as ViewItemExam).Subtitle;
            }
            return null;
        }
    }
}
