using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ToolsPortable;
using UpgradeFromSilverlight.Sections;
using Windows.UI;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class ClassWP : BaseItemWithGPACreditsWP, IGPACredits
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Class; }
        }

        public ClassWP()
        {
            Homework = new FakeList<HomeworkWP>();
            Exams = new FakeList<ExamWP>();
            Schedules = new FakeList<ScheduleWP>();
        }
        
        [DataMember]
        public string CourseNumber { get; set; }
        
        [DataMember]
        public double Grade { get; set; }
        
        [DataMember]
        public bool ShouldAverageGradeTotals { get; set; }
        
        [DataMember]
        public bool DoesRoundGradesUp { get; set; }
        
        [DataMember]
        public byte[] RawColor { get; set; }
        
        [DataMember]
        public byte Position { get; set; }
        
        [DataMember]
        public bool DoesCountTowardGPA { get; set; }

        private GradesSection _gradesSection;
        private GradesSection GradesSection
        {
            get
            {
                LoadGradesSection();

                return _gradesSection;
            }
        }

        public void LoadGradesSection()
        {
            if (_gradesSection == null)
                _gradesSection = AccountSection.GetGradesSection(this);
        }

        /// <summary>
        /// Setting automatically marks the Grades section changed
        /// </summary>
        public GradeScale[] GradeScales
        {
            get
            {
                return GradesSection.Value.GradeScales;
            }

            set
            {
                GradesSection.Value.GradeScales = value;
            }
        }


        #region DisplayLists

        [DataMember]
        public FakeList<HomeworkWP> Homework { get; set; }

        [DataMember]
        public FakeList<ExamWP> Exams { get; set; }


        /// <summary>
        /// Used for What If mode
        /// </summary>
        public List<WeightCategoryWP> WeightCategories
        {
            get
            {
                return GradesSection.Value.WeightCategories;
            }
        }

        [DataMember]
        public FakeList<ScheduleWP> Schedules { get; set; }
        

        private class AllGradesComparer : IComparer<GradeWP>
        {
            private ClassWP classItem;
            public AllGradesComparer(ClassWP c) { classItem = c; }

            public int Compare(GradeWP x, GradeWP y)
            {
                //if in same weight, sort by date
                if (x.Parent == y.Parent)
                    return x.CompareTo(y);

                //else find where the weight should be
                int xIndex = classItem.WeightCategories.IndexOf(x.Parent as WeightCategoryWP);
                int yIndex = classItem.WeightCategories.IndexOf(y.Parent as WeightCategoryWP);

                //if x's weight category was earlier, it'll return negative
                //if x's weight category was later, it'll return positive
                return xIndex - yIndex;
            }
        }

        #endregion

        

        protected override BaseItem serialize(int offset)
        {
            Class into = new Class()
            {
                CourseNumber = CourseNumber,
                GradeScales = GradeScales == null ? null : GradeScales.ToArray(),
                ShouldAverageGradeTotals = ShouldAverageGradeTotals,
                Credits = Credits,
                Position = Position,
                Color = Color.FromArgb(255, RawColor[0], RawColor[1], RawColor[2]).ToString(),
                DoesRoundGradesUp = DoesRoundGradesUp
            };

            base.serialize(into, offset);

            return into;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.CourseNumber.ToString()] = CourseNumber;
            into[PropertyNames.GradeScales.ToString()] = GradeScales;
            into[PropertyNames.ShouldAverageGradeTotals.ToString()] = ShouldAverageGradeTotals;
            into[PropertyNames.Credits.ToString()] = Credits;
            into[PropertyNames.Position.ToString()] = Position;
            into[PropertyNames.Color.ToString()] = Color.FromArgb(255, RawColor[0], RawColor[1], RawColor[2]).ToString();
            into[PropertyNames.DoesCountTowardGPA.ToString()] = DoesCountTowardGPA;

            base.serialize(into);
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

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return new IEnumerableLinker<BaseItemWP>(Homework, Exams, Schedules, WeightCategories);
        }

        internal override void Delete(bool permanent)
        {
            if (permanent)
                PermanentDelete();

            base.Delete(permanent);
        }

        internal void PermanentDelete()
        {
        }

        internal override void Add(BaseItemWP item)
        {
        }

        internal override void Remove(BaseItemWP item)
        {
        }

        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            foreach (HomeworkWP h in Homework)
                if (h.Identifier.Equals(identifier))
                    return h;

            foreach (ExamWP e in Exams)
                if (e.Identifier.Equals(identifier))
                    return e;

            foreach (ScheduleWP s in Schedules)
                if (s.Identifier.Equals(s))
                    return s;

            return null;
        }

        public override int CompareTo(BaseItemWP other)
        {
            if (other is ClassWP)
                return CompareTo(other as ClassWP);

            return base.CompareTo(other);
        }

        public int CompareTo(ClassWP other)
        {
            int compare = Name.CompareTo(other.Name);

            if (compare == 0)
                return DateCreated.CompareTo(other.DateCreated);

            return compare;
        }
    }
}
