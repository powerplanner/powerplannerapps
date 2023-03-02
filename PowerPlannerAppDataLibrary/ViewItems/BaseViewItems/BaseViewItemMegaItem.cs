using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemMegaItem : BaseViewItemWithImages, IComparable<BaseViewItemMegaItem>
    {
        public BaseViewItemMegaItem(BaseDataItemHomeworkExamGrade dataItem) : base(dataItem) { }

        public ViewItemWeightCategory _weightCategory;
        public ViewItemWeightCategory WeightCategory
        {
            get { return _weightCategory; }
            set
            {
                SetProperty(ref _weightCategory, value, "WeightCategory");
            }
        }

        private DateTime _date;
        /// <summary>
        /// This will be in the user's local time zone relative to the school's time zone, so if the assignment was due 9pm, this would possibly be 6pm.
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, "Date", "IsComplete", "PercentComplete", "ListItemTertiaryText"); }
        }

        private DateTime _dateInSchoolTime;
        /// <summary>
        /// This doesn't get converted to local, this is always "9pm" regardless of where the user currently is.
        /// </summary>
        public DateTime DateInSchoolTime
        {
            get => _dateInSchoolTime;
            set => SetProperty(ref _dateInSchoolTime, value, nameof(DateInSchoolTime));
        }

        private DateTime _effectiveDateForDisplayInDateBasedGroups;
        /// <summary>
        /// This is the date that should be used when deciding what date items should be displayed on in the calendar or day views or within groupings inside Agenda "tomorrow, in two days" groups. This was added for the following scenario: User adds a task that's due Tuesday end of day. However, user is in a time zone one or two hours ahead. So, in their local time, it's actually due Wednesday at 1am. However, if the calendar shows that task as Wednesday, they might postpone completing it till Wednesday, when realistically they had to complete it Tuesday. Therefore, this date will take into account those differences and adjust the date to be back on Tuesday as long as the item is due less than noon.
        /// </summary>
        public DateTime EffectiveDateForDisplayInDateBasedGroups
        {
            get => _effectiveDateForDisplayInDateBasedGroups;
            set => SetProperty(ref _effectiveDateForDisplayInDateBasedGroups, value, nameof(EffectiveDateForDisplayInDateBasedGroups));
        }

        private double _gradeReceived;
        public double GradeReceived
        {
            get { return _gradeReceived; }
            set { SetProperty(ref _gradeReceived, value, "GradeReceived", "IsGraded", "DoesCount", "GradePercent", nameof(GradeSubtitle)); }
        }

        private double _gradeTotal;
        public double GradeTotal
        {
            get { return _gradeTotal; }
            set { SetProperty(ref _gradeTotal, value, "GradeTotal", "IsExtraCredit", "GradePercent", nameof(GradeSubtitle)); }
        }

        /// <summary>
        /// Between 0.0 and 1.0 (other than UNGRADED)
        /// </summary>
        public double GradePercent
        {
            get
            {
                if (!IsGraded)
                    return PowerPlannerSending.Grade.UNGRADED;

                return GradeReceived / GradeTotal;
            }
        }

        private bool _isDropped;
        public bool IsDropped
        {
            get { return _isDropped; }
            set { SetProperty(ref _isDropped, value, "IsDropped", "DoesCount", nameof(GradeSubtitle)); }
        }

        private double _individualWeight;
        public double IndividualWeight
        {
            get { return _individualWeight; }
            private set { SetProperty(ref _individualWeight, value, "IndividualWeight"); }
        }

        private bool wasChanged;
        /// <summary>
        /// Used for What If mode
        /// </summary>
        public bool WasChanged
        {
            get { return wasChanged; }
            set { SetProperty(ref wasChanged, value, "WasChanged", "ColorWhenInWhatIfMode"); }
        }

        private bool _canBeUsedForAchievingDesiredGrade;
        public bool CanBeUsedForAchievingDesiredGrade
        {
            get { return _canBeUsedForAchievingDesiredGrade; }
            set { SetProperty(ref _canBeUsedForAchievingDesiredGrade, value, "CanBeUsedForAchievingDesiredGrade"); }
        }

        /// <summary>
        /// Returns true if the item is graded.
        /// </summary>
        public bool IsGraded
        {
            get { return GradeReceived != PowerPlannerSending.Grade.UNGRADED; }
        }

        public string GradeSubtitle
        {
            get
            {
                string answer = "";

                if (this.IsDropped)
                    answer = "DROPPED - ";

                if (this.GradeReceived == PowerPlannerSending.Grade.UNGRADED)
                    return answer + "----  -  --/" + this.GradeTotal;

                if (this.GradeTotal == 0)
                    return answer + "Extra Credit - " + this.GradeReceived;

                return answer + this.GradePercent.ToString("0.##%") + "  -  " + this.GradeReceived + "/" + this.GradeTotal;
            }
        }

        /// <summary>
        /// Returns true if the item IsGraded AND if the item isn't dropped
        /// </summary>
        public bool DoesCount
        {
            get { return IsGraded && !IsDropped; }
        }

        public bool IsExtraCredit
        {
            get { return GradeTotal == 0; }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemHomeworkExamGrade i = dataItem as BaseDataItemHomeworkExamGrade;

            if (this is ViewItemHoliday)
            {
                // Holidays don't get localized since they're just raw dates
                Date = ToViewItemSchoolTime(i.Date);
            }
            else
            {
                Date = ToViewItemTime(i.Date);
            }

            DateInSchoolTime = ToViewItemSchoolTime(i.Date);

            // If, due to user being in different time zone, the task got moved to a later date, and it didn't end up being due at noon or later, we'll adjust the effective date back to the original date
            if (Date.Date > DateInSchoolTime.Date && Date.Hour < 12)
            {
                EffectiveDateForDisplayInDateBasedGroups = new DateTime(DateInSchoolTime.Year, DateInSchoolTime.Month, DateInSchoolTime.Day, 23, 59, 59, DateTimeKind.Local);
            }
            else
            {
                EffectiveDateForDisplayInDateBasedGroups = Date;
            }

            GradeReceived = i.GradeReceived;
            GradeTotal = i.GradeTotal;
            IsDropped = i.IsDropped;
            IndividualWeight = i.IndividualWeight;
        }

        protected static int compareViaClass(
            ViewItemClass class1, DateTime dateCreated1, DateTime actualDate1,
            ViewItemClass class2, DateTime dateCreated2, DateTime actualDate2)
        {
            if (class1 == null || class2 == null || class1 == class2)
                return dateCreated1.CompareTo(dateCreated2);

            ViewItemSchedule schedule1 = findSchedule(class1, actualDate1);
            ViewItemSchedule schedule2 = findSchedule(class2, actualDate2);

            //if both schedules missing, group by class
            if (schedule1 == null && schedule2 == null)
            {
                return class1.CompareTo(class2);
            }

            //if the other schedule is missing, this item should go first
            if (schedule2 == null)
                return -1;

            //if this schedule is missing, the other item should go first
            if (schedule1 == null)
                return 1;

            //get the comparison of their start times
            int comp = schedule1.CompareTo(schedule2);

            //if they started at the same time, use the updated time
            if (comp == 0)
                return dateCreated1.CompareTo(dateCreated2);

            return comp;
        }

        protected static ViewItemSchedule findSchedule(ViewItemClass c, DateTime date)
        {
            if (c == null || c.IsNoClassClass || c.Schedules == null)
            {
                return null;
            }

            for (int i = 0; i < c.Schedules.Count; i++)
                if (c.Schedules[i].DayOfWeek == date.DayOfWeek)
                    return c.Schedules[i];

            return null;
        }

        public BaseDataItemHomeworkExamGrade CreateBlankDataItem()
        {
            if (this is ViewItemTaskOrEvent)
            {
                return (this as ViewItemTaskOrEvent).CreateBlankDataItem();
            }
            if (this is ViewItemGrade)
            {
                return new DataItemGrade()
                {
                    Identifier = Identifier
                };
            }
            throw new NotImplementedException("Unknown type: " + this.GetType());
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is BaseViewItemMegaItem)
            {
                return CompareTo(other as BaseViewItemMegaItem);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(BaseViewItemMegaItem other)
        {
            if (this.Date < other.Date)
                return -1;

            else if (this.Date > other.Date)
                return 1;

            return base.CompareTo(other);
        }
    }
}
