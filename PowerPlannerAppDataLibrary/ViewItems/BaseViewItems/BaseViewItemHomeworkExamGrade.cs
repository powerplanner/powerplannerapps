using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PropertyChanged;
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

        [DependsOn("IsComplete", "PercentComplete", "ListItemTertiaryText")]
        public DateTime Date { get; set; }

        [DependsOn(nameof(IsGraded), nameof(DoesCount), nameof(GradePercent), nameof(GradeSubtitle))]
        public double GradeReceived { get; set; }

        [DependsOn(nameof(IsExtraCredit), nameof(GradePercent), nameof(GradeSubtitle))]
        public double GradeTotal { get; set; }

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

        [DependsOn(nameof(DoesCount), nameof(GradeSubtitle))]
        public bool IsDropped { get; set; }

        private double _individualWeight;
        public double IndividualWeight
        {
            get { return _individualWeight; }
            private set { SetProperty(ref _individualWeight, value, "IndividualWeight"); }
        }

        /// <summary>
        /// Used for What If mode
        /// </summary>
        [DependsOn("ColorWhenInWhatIfMode")]
        public bool WasChanged { get; set; }

        public bool CanBeUsedForAchievingDesiredGrade { get; set; }

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

            Date = DateTime.SpecifyKind(i.Date, DateTimeKind.Local);
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
