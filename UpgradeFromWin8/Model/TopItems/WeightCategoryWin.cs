using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class WeightCategoryWin : BaseItemWithImagesWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.WeightCategory; }
        }

        public WeightCategoryWin()
        {
            //PercentSummed = Grade.UNGRADED;
            //PercentAveraged = Grade.UNGRADED;

            WeightAchievedAveraged = Grade.UNGRADED;
            WeightAchievedSummed = Grade.UNGRADED;

            Grades = new MyObservableList<GradeWin>();
        }

        private double _weightAchievedSummed;
        [DataMember]
        public double WeightAchievedSummed
        {
            get { return _weightAchievedSummed; }
            set { SetProperty(ref _weightAchievedSummed, value, "WeightAchievedSummed", "PercentSummed"); }
        }

        private double _weightAchievedAveraged;
        [DataMember]
        public double WeightAchievedAveraged
        {
            get { return _weightAchievedAveraged; }
            set { SetProperty(ref _weightAchievedAveraged, value, "WeightAchievedAveraged", "PercentAveraged"); }
        }

        private double _weightValue;
        [DataMember]
        public double WeightValue
        {
            get { return _weightValue; }
            set { SetProperty(ref _weightValue, value, "WeightValue"); }
        }

        //private double _percentSummed;
        //[DataMember]
        //public double PercentSummed
        //{
        //    get { return _percentSummed; }
        //    set { SetProperty(ref _percentSummed, value, "PercentSummed", "PercentForDisplay", "WeightAchievedSummed"); }
        //}

        //private double _percentAveraged;
        //[DataMember]
        //public double PercentAveraged
        //{
        //    get { return _percentAveraged; }
        //    set { SetProperty(ref _percentAveraged, value, "PercentAveraged", "PercentForDisplay", "WeightAchievedAveraged"); }
        //}

        //public string PercentForDisplay
        //{
        //    get
        //    {
        //        Parent.PropertyChanged += Parent_PropertyChanged;
        //        if ((Parent as ClassWin).ShouldAverageGradeTotals)
        //        {
        //            if (PercentAveraged == Grade.UNGRADED)
        //                return "UNGRADED";

        //            return PercentAveraged.ToString("0.##%");
        //        }

        //        else
        //        {
        //            if (PercentSummed == Grade.UNGRADED)
        //                return "UNGRADED";

        //            return PercentSummed.ToString("0.##%");
        //        }
        //    }
        //}

        //void Parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case "ShouldAverageGradeTotals":
        //            OnPropertyChanged("PercentForDisplay");
        //            break;
        //    }
        //}

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.WeightValue.ToString()] = WeightValue;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            WeightCategory into = new WeightCategory()
            {
                WeightValue = WeightValue
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.WeightValue:
                    return WeightValue;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<BaseItemWin.PropertyNames> changedProperties)
        {
            WeightCategory from = item as WeightCategory;

            if (changedProperties != null)
            {
                if (WeightValue != from.WeightValue)
                    changedProperties.Add(BaseItemWin.PropertyNames.WeightValue);
            }

            WeightValue = from.WeightValue;

            base.deserialize(from, changedProperties);
        }



        #region DisplayLists

        [DataMember]
        public MyObservableList<GradeWin> Grades { get; set; }

        #endregion



        public override void Calculate()
        {
            WeightAchievedSummed = calcWeightAchievedSummed();
            WeightAchievedAveraged = calcWeightAchievedAveraged();
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

            foreach (GradeWin g in gradesThatCount)
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

        public WeightCategoryWin CopyForWhatIf(ClassWin newParent)
        {
            WeightCategoryWin weight = new WeightCategoryWin()
            {
                Name = Name,
                WeightValue = WeightValue,
                Parent = newParent
            };


            weight.Grades = new MyObservableList<GradeWin>();
            for (int i = 0; i < Grades.Count; i++)
            {
                weight.Grades.Add(Grades[i].CopyForWhatIf(weight));
            }

            return weight;
        }

        public bool Recalculate()
        {
            //if (_needsRecalc)
            //{
            //    _needsRecalc = false;

            Calculate();
            return true;
            //}

            //return false;
        }

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return Grades;
        }

        internal override void Delete(bool permanent)
        {
            base.Delete(permanent);
        }

        internal override void Add(BaseItemWin item)
        {
            Grades.InsertSorted((GradeWin)item);
        }

        internal override void Remove(BaseItemWin item)
        {
            Grades.Remove(item as GradeWin);
        }
    }
}
