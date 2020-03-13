using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemHomeworkExamGrade : BaseViewItemWithImages, IComparable<BaseViewItemHomeworkExamGrade>
    {
        public BaseViewItemHomeworkExamGrade(BaseDataItemHomeworkExamGrade dataItem) : base(dataItem) { }

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
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, "Date", "IsComplete", "PercentComplete", "ListItemTertiaryText"); }
        }

        private DateTime _dateInSchoolTime;
        public DateTime DateInSchoolTime
        {
            get => _dateInSchoolTime;
            set => SetProperty(ref _dateInSchoolTime, value, nameof(DateInSchoolTime));
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
            int comp = schedule1.StartTime.TimeOfDay.CompareTo(schedule2.StartTime.TimeOfDay);

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
            if (this is BaseViewItemHomeworkExam)
            {
                return (this as BaseViewItemHomeworkExam).CreateBlankDataItem();
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
            if (other is BaseViewItemHomeworkExamGrade)
            {
                return CompareTo(other as BaseViewItemHomeworkExamGrade);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(BaseViewItemHomeworkExamGrade other)
        {
            if (this.Date < other.Date)
                return -1;

            else if (this.Date > other.Date)
                return 1;

            return base.CompareTo(other);
        }
    }
}
