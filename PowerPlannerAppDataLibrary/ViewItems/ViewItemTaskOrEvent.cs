using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Extensions;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public enum TaskOrEventType
    {
        Task,
        Event
    }

    public class ViewItemTaskOrEvent : BaseViewItemMegaItem, IComparable<ViewItemTaskOrEvent>
    {
        private TaskOrEventType _type;
        public TaskOrEventType Type
        {
            get => _type;
            set => SetProperty(ref _type, value, nameof(Type), nameof(Subtitle), nameof(SubtitleDueDate), nameof(SubtitleDueTime), nameof(PercentComplete), nameof(IsComplete));
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

        private ViewItemClass _class;
        private PropertyChangedEventHandler _classPropertyChangedHandler;
        public ViewItemClass Class
        {
            get { return _class; }
            set
            {
                if (_class != null && _classPropertyChangedHandler != null)
                {
                    _class.PropertyChanged -= _classPropertyChangedHandler;
                }

                SetProperty(ref _class, value, nameof(Class));

                HandleUpdatingWeightCategory();

                if (_class != null && !_class.IsNoClassClass)
                {
                    _classPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(_class_PropertyChanged).Handler;
                    _class.PropertyChanged += _classPropertyChangedHandler;
                }

                NotifySubtitleChanged();
            }
        }

        private void HandleUpdatingWeightCategory()
        {
            if (WeightCategoryIdentifier == PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED)
            {
                WeightCategory = ViewItemWeightCategory.UNASSIGNED;
            }
            else if (WeightCategoryIdentifier == PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED)
            {
                WeightCategory = ViewItemWeightCategory.EXCLUDED;
            }
            else
            {
                if (Class?.WeightCategories == null)
                {
                    // WeightCategories should only be null in the scenario where the view group was loaded disregarding
                    // the weight categories (like from the live tile task)
                    WeightCategory = null;
                    return;
                }

                var weight = Class.WeightCategories.FirstOrDefault(i => i.Identifier == WeightCategoryIdentifier);
                if (weight != null)
                {
                    WeightCategory = weight;
                }
                else
                {
                    WeightCategory = ViewItemWeightCategory.UNASSIGNED;
                }
            }
        }

        private double _percentComplete;
        public double PercentComplete
        {
            get
            {
                if (Type == TaskOrEventType.Event)
                {
                    return IsComplete ? 1 : 0;
                }

                return _percentComplete;
            }

            private set { SetProperty(ref _percentComplete, value, nameof(PercentComplete), nameof(IsComplete)); }
        }

        public bool IsComplete
        {
            get
            {
                if (Type == TaskOrEventType.Task)
                {
                    return PercentComplete >= 1;
                }
                else
                {
                    if (Date < DateTime.Today)
                    {
                        return true;
                    }

                    DateTime endTime;
                    if (TryGetEndDateWithTime(out endTime))
                    {
                        return endTime < DateTime.Now;
                    }

                    return false;
                }
            }
        }

        public bool IsTask
        {
            get => Type == TaskOrEventType.Task;
        }
        
        public bool IsEvent
        {
            get => Type == TaskOrEventType.Event;
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemMegaItem i = dataItem as DataItemMegaItem;

            switch (i.MegaItemType)
            {
                case PowerPlannerSending.MegaItemType.Task:
                case PowerPlannerSending.MegaItemType.Homework:
                    Type = TaskOrEventType.Task;
                    break;

                case PowerPlannerSending.MegaItemType.Event:
                case PowerPlannerSending.MegaItemType.Exam:
                    Type = TaskOrEventType.Event;
                    break;

                default:
                    throw new NotImplementedException("This code shouldn't be hit.");
            }

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

            PercentComplete = i.PercentComplete;
        }

        private void _class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Class.Name):
                    NotifySubtitleChanged();
                    break;
            }
        }

        public string Subtitle
        {
            get
            {
                if (Class == null || Class.IsNoClassClass)
                {
                    return SubtitleDueDate;
                }
                return Class.Name + SubtitleDueDate;
            }
        }

        /// <summary>
        /// Includes both date and time
        /// </summary>
        public string SubtitleDueDate
        {
            get
            {
                string answer = DateHelpers.ToFriendlyShortDate(Date);

                string timeString = GetTimeString();

                answer += timeString;

                if (Class == null || Class.IsNoClassClass)
                {
                    return string.Format(PowerPlannerResources.GetString(Type == TaskOrEventType.Task ? "String_DueX" : "String_OnX"), answer);
                }
                else
                {
                    // Need to make the rest lowercase
                    return $" - " + PowerPlannerResources.GetStringAsLowercaseWithParameters(Type == TaskOrEventType.Task ? "String_DueX" : "String_OnX", answer);
                }
            }
        }

        /// <summary>
        /// Only includes the time, not date
        /// </summary>
        public string SubtitleDueTime
        {
            get
            {
                var answer = GetTimeString().TrimStart(',', ' ');

                if (Class == null || Class.IsNoClassClass)
                {
                    if (Type == TaskOrEventType.Task)
                    {
                        return string.Format(PowerPlannerResources.GetString("String_DueX"), answer);
                    }
                    else
                    {
                        return answer.Substring(0, 1).ToUpper() + answer.Substring(1);
                    }
                }
                else
                {
                    if (Type == TaskOrEventType.Task)
                    {
                        // Need to make the rest lowercase
                        return $" - " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_DueX", answer);
                    }
                    else
                    {
                        return $" - {answer}";
                    }
                }
            }
        }

        private string GetTimeString()
        {
            switch (GetActualTimeOption())
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    {
                        if (Type == TaskOrEventType.Task && Account.IsInDifferentTimeZone)
                        {
                            return ", " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_AtX", DateTimeFormatterExtension.Current.FormatAsShortTime(Date));
                        }

                        return ", " + PowerPlannerResources.GetString(Type == TaskOrEventType.Task ? "TimeOption_EndOfDay" : "TimeOption_AllDay").ToLower();
                    }

                case DataItemMegaItem.TimeOptions.BeforeClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_BeforeClass").ToLower();

                case DataItemMegaItem.TimeOptions.Custom:
                    if (Type == TaskOrEventType.Task)
                    {
                        return " " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_AtX", DateTimeFormatterExtension.Current.FormatAsShortTime(Date));
                    }
                    else
                    {
                        return $", " + string.Format(PowerPlannerResources.GetString("String_TimeToTime"), DateTimeFormatterExtension.Current.FormatAsShortTimeWithoutAmPm(Date), DateTimeFormatterExtension.Current.FormatAsShortTime(EndTime));
                    }

                case DataItemMegaItem.TimeOptions.DuringClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_DuringClass").ToLower();

                case DataItemMegaItem.TimeOptions.EndOfClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_EndOfClass").ToLower();

                case DataItemMegaItem.TimeOptions.StartOfClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_StartOfClass").ToLower();
            }

            // Shouldn't get hit
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            return "";
        }

        private void NotifySubtitleChanged()
        {
            OnPropertyChanged(nameof(Subtitle));
        }

        internal ViewItemTaskOrEvent(DataItemMegaItem dataItem) : base(dataItem)
        {
            base.PropertyChanged += ViewItemTaskOrEvent_PropertyChanged;
        }

        private void ViewItemTaskOrEvent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Date):
                    NotifySubtitleChanged();
                    break;
            }
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemTaskOrEvent otherTaskOrEvent)
            {
                return CompareTo(otherTaskOrEvent);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(ViewItemTaskOrEvent other)
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
            var class1 = Class;
            var class2 = other.Class;

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


        /// <summary>
        /// Used by grades view model, so that it knows which type of item to draw, since it's all contained in the same list
        /// </summary>
        public bool IsUnassignedItem { get; set; }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            private set { SetProperty(ref _endTime, value, "EndTime"); }
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
                    var c = Class;
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

            var c = Class;
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

        public new DataItemMegaItem CreateBlankDataItem()
        {
            if (Type == TaskOrEventType.Event)
            {
                return new DataItemMegaItem()
                {
                    Identifier = Identifier,
                    MegaItemType = this.Class.IsNoClassClass ? PowerPlannerSending.MegaItemType.Event : PowerPlannerSending.MegaItemType.Exam
                };
            }
            if (Type == TaskOrEventType.Task)
            {
                return new DataItemMegaItem()
                {
                    Identifier = Identifier,
                    MegaItemType = this.Class.IsNoClassClass ? PowerPlannerSending.MegaItemType.Task : PowerPlannerSending.MegaItemType.Homework
                };
            }
            throw new NotImplementedException("Unknown type: " + Type);
        }

        private int TryCompareByType(ViewItemTaskOrEvent other)
        {
            if (this.Type == other.Type)
            {
                return 0;
            }

            if (this.Type == TaskOrEventType.Event)
            {
                // Events go ahead of everything
                return -1;
            }
            else if (other.Type == TaskOrEventType.Event)
            {
                return 1;
            }

            if (this.Type == TaskOrEventType.Task)
            {
                return -1;
            }
            else if (other.Type == TaskOrEventType.Task)
            {
                return 1;
            }

            // Only remaining type is Event. Both items, by definition, must be events.
            // Therefore this code shouldn't even get hit.
            return 0;
        }

        private int TryCompareByCompletionStatus(ViewItemTaskOrEvent other)
        {
            var thisIsComplete = this.IsComplete;
            var otherIsComplete = other.IsComplete;

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

        private int TryCompareWithDateTimeOtherwiseUseDateCreated(ViewItemTaskOrEvent other)
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
            if (this.Type == TaskOrEventType.Task)
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
            if (this.Type == TaskOrEventType.Task)
            {
                return Date.Date.AddDays(1).AddSeconds(-1);
            }
            else
            {
                return Date.Date;
            }
        }

        public override bool DateTimeIsDependentOnSchedule
        {
            get
            {
                switch (TimeOption)
                {
                    case DataItemMegaItem.TimeOptions.AllDay:
                    case DataItemMegaItem.TimeOptions.Custom:
                        return false;

                    default:
                        return true;
                }
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
                        answer = schedule.StartTimeInLocalTime(Date).AddMinutes(-1);
                        return true;

                    case DataItemMegaItem.TimeOptions.StartOfClass:
                        answer = schedule.StartTimeInLocalTime(Date);
                        return true;

                    case DataItemMegaItem.TimeOptions.DuringClass:
                        if (this.Type == TaskOrEventType.Task)
                        {
                            answer = schedule.StartTimeInLocalTime(Date).AddTicks(schedule.Duration.Ticks / 2);
                            return true;
                        }
                        else
                        {
                            answer = schedule.StartTimeInLocalTime(Date);
                            return true;
                        }

                    case DataItemMegaItem.TimeOptions.EndOfClass:
                        answer = schedule.EndTimeInLocalTime(Date);
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
            if (this.Type == TaskOrEventType.Task)
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
                        dateTime = schedule.EndTimeInLocalTime(Date);
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
            var c = Class;

            return findSchedule(c, Date.Date);
        }

        /// <summary>
        /// Returns true if the item is not past-completed.
        /// </summary>
        /// <returns></returns>
        public bool IsActive(DateTime today)
        {
            if (Type == TaskOrEventType.Task)
            {
                return !IsComplete || Date >= today;
            }
            else
            {
                return !IsComplete;
            }
        }

        public override string ToString()
        {
            return Name + " - " + Class?.Name;
        }
    }
}
