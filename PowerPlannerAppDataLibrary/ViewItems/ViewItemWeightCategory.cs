using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerSending;
using ToolsPortable;
using System.ComponentModel;
using PropertyChanged;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemWeightCategory : BaseViewItemWithImages
    {
        public static ViewItemWeightCategory UNASSIGNED = new ViewItemWeightCategory(new DataItemWeightCategory()
        {
            Name = PowerPlannerResources.GetString("WeightCategory_Unassigned"),
            Identifier = PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED
        });

        public static ViewItemWeightCategory EXCLUDED = new ViewItemWeightCategory(new DataItemWeightCategory()
        {
            Name = PowerPlannerResources.GetString("WeightCategory_Exclude"),
            Identifier = PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED
        });

        private ViewItemClass _class;
        public ViewItemClass Class
        {
            get { return _class; }
            set
            {
                SetProperty(ref _class, value, "Class");
            }
        }

        /// <summary>
        /// Something like "15/20"
        /// </summary>
        public string WeightAchievedAndTotalString
        {
            get
            {
                return (WeightAchieved == Grade.UNGRADED ? "--" : WeightAchieved.ToString("0.##")) + "/" + WeightValue.ToString("0.##");
            }
        }

        private bool _hasWiredClassEvent;
        public double WeightAchieved
        {
            get
            {
                if (Class == null)
                    throw new NullReferenceException("Class was null");

                if (!_hasWiredClassEvent)
                {
                    _hasWiredClassEvent = true;
                    Class.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Class_PropertyChanged).Handler;
                }

                if (Class.ShouldAverageGradeTotals)
                {
                    return WeightAchievedAveraged;
                }
                else
                {
                    return WeightAchievedSummed;
                }
            }
        }

        private void Class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewItemClass.ShouldAverageGradeTotals):
                    OnPropertyChanged(nameof(WeightAchieved), nameof(WeightAchievedAndTotalString));
                    break;
            }
        }

        [DependsOn(nameof(WeightAchieved), nameof(WeightAchievedAndTotalString))]
        public double WeightAchievedSummed { get; set; } = Grade.UNGRADED;

        [DependsOn(nameof(WeightAchieved), nameof(WeightAchievedAndTotalString))]
        public double WeightAchievedAveraged { get; set; } = Grade.UNGRADED;

        public void Remove(BaseViewItemHomeworkExamGrade viewItemGrade)
        {
            if (Grades != null)
                Grades.Remove(viewItemGrade);

            NeedsRecalc = true;
        }

        public bool NeedsRecalc { get; internal set; } = true;

        public ViewItemWeightCategory(
            DataItemWeightCategory dataItem,
            Func<BaseDataItemHomeworkExamGrade, BaseViewItemHomeworkExamGrade> createGradeMethod = null) : base(dataItem)
        {
            if (createGradeMethod != null)
            {
                AddGradesHelper(createGradeMethod);
            }
        }

        public void AddGradesHelper(Func<BaseDataItemHomeworkExamGrade, BaseViewItemHomeworkExamGrade> createGradeMethod)
        {
            Grades = new MyObservableList<BaseViewItemHomeworkExamGrade>();

            AddChildrenHelper(new ViewItemChildrenHelper<BaseDataItemHomeworkExamGrade, BaseViewItemHomeworkExamGrade>(
                isChild: IsChild,
                addMethod: Add,
                removeMethod: Remove,
                createChildMethod: createGradeMethod,
                children: Grades));
        }

        public static BaseViewItemHomeworkExamGrade CreateGradeHelper(BaseDataItemHomeworkExamGrade dataItem)
        {
            if (dataItem is DataItemMegaItem)
            {
                var type = (dataItem as DataItemMegaItem).MegaItemType;
                if (type == MegaItemType.Homework)
                {
                    return new ViewItemHomework(dataItem as DataItemMegaItem);
                }
                else if (type == MegaItemType.Exam)
                {
                    return new ViewItemExam(dataItem as DataItemMegaItem);
                }
            }
            else if (dataItem is DataItemGrade)
            {
                return new ViewItemGrade(dataItem as DataItemGrade);
            }
            throw new NotImplementedException("Unknown type");
        }

        private bool IsChild(BaseDataItemHomeworkExamGrade dataItem)
        {
            if (dataItem is DataItemGrade)
            {
                return dataItem.UpperIdentifier == Identifier;
            }
            else if (dataItem is DataItemMegaItem)
            {
                return (dataItem as DataItemMegaItem).WeightCategoryIdentifier == Identifier;
            }
            else
            {
                return false;
            }
        }

        private MyObservableList<BaseViewItemHomeworkExamGrade> _grades;
        public MyObservableList<BaseViewItemHomeworkExamGrade> Grades
        {
            get { return _grades; }
            private set { SetProperty(ref _grades, value, nameof(Grades)); }
        }

        private double _weightValue;
        public double WeightValue
        {
            get { return _weightValue; }
            private set { SetProperty(ref _weightValue, value, "WeightValue"); }
        }

        public void Add(BaseViewItemHomeworkExamGrade grade)
        {
            grade.WeightCategory = this;

            if (grade is ViewItemHomework)
            {
                (grade as ViewItemHomework).Class = this.Class;
            }
            else if (grade is ViewItemExam)
            {
                (grade as ViewItemExam).Class = this.Class;
            }

            if (Grades != null)
                Grades.InsertSorted(grade);

            NeedsRecalc = true;
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemWeightCategory i = dataItem as DataItemWeightCategory;

            WeightValue = i.WeightValue;

            NeedsRecalc = true;
        }

        public bool Calculate()
        {
            if (!NeedsRecalc)
                return false;

            NeedsRecalc = false;

            WeightAchievedSummed = calcWeightAchievedSummed();
            WeightAchievedAveraged = calcWeightAchievedAveraged();

            return true;
        }

        private double calcWeightAchievedSummed()
        {
            /* EXAMPLES */

            /*
             * 
             * Bonus (0)
             * =========    points = 0
             *    0/0
             * 
             * Bonus (0)
             *  - 2/0       points = 3
             *  - 1/0
             * =========
             *    3/0
             *    
             * Bonus (0)
             *  - 1/1       points = 8
             *  - 4/5
             *  - 2/0
             *  - 1/0
             * =========
             *    8/0
             * 
             * Participation (5)
             * =========    total = 0
             *   --/5
             *   
             * Participation (5)
             *  - 2/0       points = 3
             *  - 1/0       total = 0
             * =========
             *   --/5
             *   
             * Participation (5)
             *  - 1/1       points = 4
             *  - 2/0       total = 1
             *  - 1/0
             * =========
             *   20/5
             *   
             * Participation (5)
             *  - 1/1 (1)   points = 8
             *  - 4/5 (0.8) total = 6
             *  - 2/0 (2)
             *  - 1/0 (1)
             * =========
             *  6.6/5
             *  
             * All Grades (100)
             *  - 1/1       points = 4
             *  - 2/0       total = 1
             *  - 1/0
             * =========
             *  400/100
             *  
             * All Grades (100)
             *  - 1/1       points = 8
             *  - 4/5       total = 6
             *  - 2/0
             *  - 1/0
             * =========
             *  133/100
             * 
             * */

            //non-dropped graded items (including EC)
            var gradesThatCount = Grades.Where(i => i.DoesCount);

            //sum of GradeReceived
            double points = gradesThatCount.Sum(i => i.GradeReceived);

            //if WeightValue is 0, then we just use the total of the points
            if (WeightValue == 0)
                return points;

            //sum of GradeTotal
            double total = gradesThatCount.Sum(i => i.GradeTotal);

            //if there wasn't a non-EC item, then it's ungraded
            if (total == 0)
                return Grade.UNGRADED;

            return points / total * WeightValue;
        }

        private double calcWeightAchievedAveraged()
        {
            /* EXAMPLES */

            /*
             * 
             * Bonus (0)
             * =========    0 + 0
             *    0/0
             * 
             * Bonus (0)
             *  - 2/0       0 + 3
             *  - 1/0
             * =========
             *    3/0
             *    
             * Bonus (0)
             *  - 1/1       1.8 + 3
             *  - 4/5
             *  - 2/0
             *  - 1/0
             * =========
             *  4.8/0
             * 
             * Participation (5)
             * =========    countOfRegulars = 0, countOfExtras = 0
             *   --/5
             *   
             * Participation (5)
             *  - 2/0       countOfRegulars = 0, countOfExtras = 2
             *  - 1/0       extraPercents = 3
             * =========
             *    8/5       1 * 5 + 3
             *   
             * Participation (5)
             *  - 1/1       countOfRegulars = 1, countOfExtras = 2
             *  - 2/0       percentTotal = 1, percent = 1, extraPercents = 3
             *  - 1/0
             * =========
             *    8/5       1 * 5 + 3
             *   
             * Participation (5)
             *  - 1/1 (1)   countOfRegulars = 2, countOfExtras = 2
             *  - 4/5 (0.8) percentTotal = 1.8, percent = 0.9, extraPercents = 3
             *  - 2/0 (2)
             *  - 1/0 (1)
             * =========
             *  7.5/5       0.9 * 5 + 3
             *  
             * All Grades (100)
             *  - 1/1       countOfRegulars = 1, countOfExtras = 2
             *  - 2/0       percentTotal = 1, percent = 1, extraPercents = 3
             *  - 1/0
             * =========
             *  103/100     1 * 100 + 3
             *  
             * All Grades (100)
             *  - 1/1       countOfRegulars = 2, countOfExtras = 2
             *  - 4/5       percentTotal = 1.8, percent = 0.9, extraPercents = 3
             *  - 2/0
             *  - 1/0
             * =========
             *  93/100      0.9 * 100 + 3
             * 
             * */


            //non-dropped graded items (including EC)
            var gradesThatCount = Grades.Where(i => i.DoesCount);

            double extraPercents = 0;
            int countOfExtras = 0;

            double percentTotal = 0;
            int countOfRegulars = 0;

            foreach (var g in gradesThatCount)
            {
                if (g.IsExtraCredit)
                {
                    extraPercents += g.GradeReceived;
                    countOfExtras += 1;
                }

                else
                {
                    countOfRegulars++;
                    percentTotal += g.GradePercent;
                }
            }

            if (WeightValue == 0)
                return percentTotal + extraPercents;

            double percent;

            //if they don't have any regular grades
            if (countOfRegulars == 0)
            {
                //if they didn't have any EC grades either, it's ungraded
                if (countOfExtras == 0)
                    return Grade.UNGRADED;

                //otherwise, we assume their regular grade is 100%.
                percent = 1;
            }

            //otherwise calculate their regular grade
            else
                percent = percentTotal / countOfRegulars;


            return percent * WeightValue + extraPercents;
        }

        internal void PrepareForWhatIf()
        {
            foreach (var grade in Grades)
            {
                // If it's ungraded and not dropped, then we can set its grade to achieve desired grade
                // We need that property since when assigning grades, we'll be overwriting the GradeReceived value, and 
                // thus we would lose the knowledge of whether it was actually an ungraded item
                if (!grade.IsGraded && !grade.IsDropped)
                    grade.CanBeUsedForAchievingDesiredGrade = true;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
