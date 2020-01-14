using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.UI;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class ClassWin : BaseItemWithGPACreditsWin, IGPACredits, IComparable<ClassWin>
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Class; }
        }

        public ClassWin()
        {
            Homework = new MyObservableList<HomeworkWin>();
            Exams = new MyObservableList<ExamWin>();
            Schedules = new MyObservableList<ScheduleWin>();
        }

        private string _courseNumber;
        [DataMember]
        public string CourseNumber
        {
            get { return _courseNumber; }
            set { SetProperty(ref _courseNumber, value, "CourseNumber"); }
        }

        private double _grade;
        [DataMember]
        public double Grade
        {
            get { return _grade; }
            set { SetProperty(ref _grade, value, "Grade"); }
        }

        private string _gradeLetter;
        [DataMember]
        public string GradeLetter
        {
            get { return _gradeLetter; }
            set { SetProperty(ref _gradeLetter, value, "GradeLetter"); }
        }

        private bool _shouldAverageGradeTotals;
        [DataMember]
        public bool ShouldAverageGradeTotals
        {
            get { return _shouldAverageGradeTotals; }
            set { SetProperty(ref _shouldAverageGradeTotals, value, "ShouldAverageGradeTotals"); }
        }

        private bool _doesRoundGradesUp;
        [DataMember]
        public bool DoesRoundGradesUp
        {
            get { return _doesRoundGradesUp; }
            set { SetProperty(ref _doesRoundGradesUp, value, "DoesRoundGradesUp"); }
        }

        private byte[] _rawColor;
        [DataMember]
        public byte[] RawColor
        {
            get { return _rawColor; }
            set { SetProperty(ref _rawColor, value, "RawColor"); }
        }

        private byte _position;
        [DataMember]
        public byte Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value, "Position"); }
        }

        private bool _doesCountTowardGPA;
        [DataMember]
        public bool DoesCountTowardGPA
        {
            get { return _doesCountTowardGPA; }
            set { SetProperty(ref _doesCountTowardGPA, value, "DoesCountTowardGPA"); }
        }

        private GradeScale[] _gradeScales = new GradeScale[0];
        [DataMember]
        public GradeScale[] GradeScales
        {
            get
            {
                return _gradeScales;
            }

            set
            {
                SetProperty(ref _gradeScales, value, "GradeScales");
            }
        }


        #region DisplayLists

        [DataMember]
        public MyObservableList<HomeworkWin> Homework { get; set; }

        [DataMember]
        public MyObservableList<ExamWin> Exams { get; set; }

        private MyObservableList<BaseHomeworkExamWin> _combined;
        public MyObservableList<BaseHomeworkExamWin> Combined
        {
            get
            {
                if (_combined == null)
                {
                    _combined = new MyObservableList<BaseHomeworkExamWin>();
                    _combined.InsertSorted(Homework);
                    _combined.InsertSorted(Exams);
                }

                return _combined;
            }
        }


        /// <summary>
        /// Used for What If mode
        /// </summary>
        private MyObservableList<WeightCategoryWin> _weightCategories = new MyObservableList<WeightCategoryWin>();
        [DataMember]
        public MyObservableList<WeightCategoryWin> WeightCategories
        {
            get
            {
                return _weightCategories;
            }

            set { _weightCategories = value; }
        }

        [DataMember]
        public MyObservableList<ScheduleWin> Schedules { get; set; }

        public void NotifySchedulesChanged()
        {
            OnPropertyChanged("Schedules");
        }

        private class AllGradesComparer : IComparer<GradeWin>
        {
            private readonly ClassWin classItem;
            public AllGradesComparer(ClassWin c) { classItem = c; }

            public int Compare(GradeWin x, GradeWin y)
            {
                //if in same weight, sort by date
                if (x.Parent == y.Parent)
                    return x.CompareTo(y);

                //else find where the weight should be
                int xIndex = classItem.WeightCategories.IndexOf(x.Parent as WeightCategoryWin);
                int yIndex = classItem.WeightCategories.IndexOf(y.Parent as WeightCategoryWin);

                //if x's weight category was earlier, it'll return negative
                //if x's weight category was later, it'll return positive
                return xIndex - yIndex;
            }
        }

        private MyObservableList<GradeWin> _grades;
        public MyObservableList<GradeWin> Grades
        {
            get
            {
                if (_grades == null)
                {
                    _grades = new MyObservableList<GradeWin>
                    {
                        Comparer = new AllGradesComparer(this)
                    };

#pragma warning disable CS0618 // Type or member is obsolete
                    _grades.InsertSorted(WeightCategories, "Grades");
#pragma warning restore CS0618 // Type or member is obsolete
                }

                return _grades;
            }
        }

        public bool IsGradesLoaded { get { return _grades != null; } }

        #endregion


        #region Methods


        public ScheduleWin GrabEarliestScheduleOnDate(DateTime date, Schedule.Week week)
        {
            for (int i = 0; i < Schedules.Count; i++)
                if (Schedules[i].DayOfWeek == date.DayOfWeek && (Schedules[i].ScheduleWeek & week) == week)
                    return Schedules[i];

            return null;
        }

        public override void Calculate()
        {
            CalculateInternal();
        }

        private void CalculateInternal()
        {
            double totalGrade = 0, totalWeight = 0;

            // GRADE CALCULATION:
            //
            // WEIGHT VALUE  * PERCENT        = FINAL PERCENT (totalGrade is the sum of these percents)
            // 15            * 97%            = 14.55%
            // 30            * 90%            = 27%
            // 55            * 93%            = 51.15%
            // -----------------------------------------------
            // GRADE = (14.55% + 27% + 51.15%) / WEIGHT VALUE TOTAL = 92.7%
            //
            //
            // ALTERNATIVELY:
            //
            // GRADE = SUM(WEIGHT PERCENT * WEIGHT AMOUNT) / WEIGHT AMOUNT TOTAL
            //       = totalGrade                          / totalWeight;

            foreach (WeightCategoryWin w in WeightCategories)
            {
                if (this.ShouldAverageGradeTotals)
                {
                    if (w.WeightAchievedAveraged != PowerPlannerSending.Grade.UNGRADED)
                    {
                        totalGrade += w.WeightAchievedAveraged;
                        totalWeight += w.WeightValue;
                    }
                }

                else
                {
                    if (w.WeightAchievedSummed != PowerPlannerSending.Grade.UNGRADED)
                    {
                        totalGrade += w.WeightAchievedSummed;
                        totalWeight += w.WeightValue;
                    }
                }
            }

            //if there was actually a grade
            if (totalWeight != 0)
                this.Grade = totalGrade / totalWeight;
            else
                this.Grade = 1;



            //if they have a custom grade scale
            if (this.GradeScales != null && this.GradeScales.Length != 0)
            {
                //if the grade percents are rounded up
                if (this.DoesRoundGradesUp)
                {
                    int roundedGrade = (int)MyMath.Round(Grade * 100);

                    for (int i = 0; i < GradeScales.Length; i++)
                    {
                        if (roundedGrade >= (GradeScales[i]).StartGrade)
                        {
                            //GradeLetter = (GradeScales[i]).LetterGrade;
                            GPA = (GradeScales[i]).GPA;
                            return;
                        }
                    }
                }

                else
                {
                    for (int i = 0; i < GradeScales.Length; i++)
                    {
                        if (Grade * 100 >= (GradeScales[i]).StartGrade)
                        {
                            //GradeLetter = (GradeScales[i]).LetterGrade;
                            GPA = (GradeScales[i]).GPA;
                            return;
                        }
                    }
                }

                //GradeLetter = GradeScales.Last().LetterGrade;
                GPA = GradeScales.Last().GPA;
            }

            //use default grade scale
            else
            {
                int roundedGrade = MyMath.Round(Grade * 100);

                if (roundedGrade >= 97)
                    GradeLetter = "A+";
                else if (roundedGrade >= 93)
                    GradeLetter = "A";
                else if (roundedGrade >= 90)
                    GradeLetter = "A-";
                else if (roundedGrade >= 87)
                    GradeLetter = "B+";
                else if (roundedGrade >= 83)
                    GradeLetter = "B";
                else if (roundedGrade >= 80)
                    GradeLetter = "B-";
                else if (roundedGrade >= 77)
                    GradeLetter = "C+";
                else if (roundedGrade >= 73)
                    GradeLetter = "C";
                else if (roundedGrade >= 70)
                    GradeLetter = "C-";
                else if (roundedGrade >= 67)
                    GradeLetter = "D+";
                else if (roundedGrade >= 63)
                    GradeLetter = "D";
                else if (roundedGrade >= 60)
                    GradeLetter = "D-";
                else
                    GradeLetter = "F";

                if (roundedGrade >= 90)
                    GPA = 4;
                else if (roundedGrade >= 80)
                    GPA = 3;
                else if (roundedGrade >= 70)
                    GPA = 2;
                else if (roundedGrade >= 60)
                    GPA = 1;
                else
                    GPA = 0;
            }
        }

        #endregion

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.CourseNumber.ToString()] = CourseNumber;
            into[PropertyNames.GradeScales.ToString()] = GradeScales;
            into[PropertyNames.ShouldAverageGradeTotals.ToString()] = ShouldAverageGradeTotals;
            into[PropertyNames.Credits.ToString()] = Credits;
            into[PropertyNames.Position.ToString()] = Position;
            into[PropertyNames.Color.ToString()] = Color.FromArgb(255, RawColor[0], RawColor[1], RawColor[2]).ToString();
            into[PropertyNames.DoesRoundGradesUp.ToString()] = DoesRoundGradesUp;
            into[PropertyNames.DoesCountTowardGPA.ToString()] = DoesCountTowardGPA;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            Class into = new Class()
            {
                CourseNumber = CourseNumber,
                GradeScales = GradeScales?.ToArray(),
                ShouldAverageGradeTotals = ShouldAverageGradeTotals,
                Credits = Credits,
                Position = Position,
                Color = Color.FromArgb(255, RawColor[0], RawColor[1], RawColor[2]).ToString(),
                DoesRoundGradesUp = DoesRoundGradesUp
            };

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Class c = item as Class;
            Color color = ColorTranslator.FromHtml(c.Color);
            byte[] rawColor = new byte[]
            {
                color.R, color.G, color.B
            };

            if (changedProperties != null)
            {
                if (CourseNumber != c.CourseNumber)
                    changedProperties.Add(PropertyNames.CourseNumber);

                if ((GradeScales == null && c.GradeScales != null) ||
                    (GradeScales != null && c.GradeScales == null) ||
                    (GradeScales != null && c.GradeScales != null && !GradeScales.SequenceEqual(c.GradeScales)))
                    changedProperties.Add(PropertyNames.GradeScales);

                if (ShouldAverageGradeTotals != c.ShouldAverageGradeTotals)
                    changedProperties.Add(PropertyNames.ShouldAverageGradeTotals);

                if (Credits != c.Credits)
                    changedProperties.Add(PropertyNames.Credits);

                if (Position != c.Position)
                    changedProperties.Add(PropertyNames.Position);

                if (!RawColor.SequenceEqual(rawColor))
                    changedProperties.Add(PropertyNames.Color);

                if (DoesRoundGradesUp != c.DoesRoundGradesUp)
                    changedProperties.Add(PropertyNames.DoesRoundGradesUp);
            }

            CourseNumber = c.CourseNumber;
            GradeScales = c.GradeScales;
            ShouldAverageGradeTotals = c.ShouldAverageGradeTotals;
            Credits = c.Credits;
            Position = c.Position;

            RawColor = rawColor;

            DoesRoundGradesUp = c.DoesRoundGradesUp;

            base.deserialize(c, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.CourseNumber:
                    return CourseNumber;

                case PropertyNames.GradeScales:
                    return GradeScales;

                case PropertyNames.ShouldAverageGradeTotals:
                    return ShouldAverageGradeTotals;

                case PropertyNames.Credits:
                    return Credits;

                case PropertyNames.Position:
                    return Position;

                case PropertyNames.Color:
                    return Color.FromArgb(255, RawColor[0], RawColor[1], RawColor[2]).ToString();

                case PropertyNames.DoesRoundGradesUp:
                    return DoesRoundGradesUp;

                case PropertyNames.DoesCountTowardGPA:
                    return DoesCountTowardGPA;
            }

            return base.GetPropertyValue(p);
        }

        protected override IEnumerable getGradedChildren()
        {
            throw new NotImplementedException();
        }
        
        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return new IEnumerableLinker<BaseItemWin>(Homework, Exams, Schedules, WeightCategories);
        }

        internal override void Delete(bool permanent)
        {
            base.Delete(permanent);
        }

        internal override void Add(BaseItemWin item)
        {
            if (item is HomeworkWin)
                Homework.InsertSorted(item as HomeworkWin);

            else if (item is ExamWin)
                Exams.InsertSorted(item as ExamWin);

            else if (item is ScheduleWin)
                Schedules.InsertSorted(item as ScheduleWin);

            else if (item is WeightCategoryWin)
                WeightCategories.InsertSorted(item as WeightCategoryWin);

            else
                throw new NotImplementedException("Item to be added to Class wasn't any of the supported types.");
        }

        internal override void Remove(BaseItemWin item)
        {
            if (item is HomeworkWin)
                Homework.Remove(item as HomeworkWin);

            else if (item is ExamWin)
                Exams.Remove(item as ExamWin);

            else if (item is ScheduleWin)
                Schedules.Remove(item as ScheduleWin);

            else if (item is WeightCategoryWin)
                WeightCategories.Remove(item as WeightCategoryWin);

            else
                throw new NotImplementedException("Item to be removed from Class wasn't any of the supported types.");
        }

        public override int CompareTo(BaseItemWin other)
        {
            if (other is ClassWin)
                return CompareTo(other as ClassWin);

            return base.CompareTo(other);
        }

        public int CompareTo(ClassWin other)
        {
            int comp = Name.CompareTo(other.Name);

            if (comp == 0)
                return DateCreated.CompareTo(other.DateCreated);

            return comp;
        }
    }
}
