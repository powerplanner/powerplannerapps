using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemYear : BaseViewItems.BaseViewItemWithOverriddenGPACredits, IComparable<ViewItemYear>
    {
        public ViewItemYear(
            DataItemYear dataItem,
            Func<DataItemSemester, ViewItemSemester> createSemesterMethod) : base(dataItem)
        {
            if (createSemesterMethod != null)
            {
                Semesters = new MyObservableList<ViewItemSemester>();

                AddChildrenHelper(new ViewItemChildrenHelper<DataItemSemester, ViewItemSemester>(
                    isChild: IsChild,
                    addMethod: Add,
                    removeMethod: Remove,
                    createChildMethod: createSemesterMethod,
                    children: Semesters));
            }
        }

        public DateTime Start
        {
            get
            {
                var semesterWithStartDate = Semesters.FirstOrDefault(i => !PowerPlannerSending.DateValues.IsUnassigned(i.Start));
                if (semesterWithStartDate != null)
                {
                    return semesterWithStartDate.Start;
                }

                return PowerPlannerSending.DateValues.UNASSIGNED;
            }
        }

        internal void Remove(ViewItemSemester viewItemSemester)
        {
            if (Semesters != null)
                Semesters.Remove(viewItemSemester);
        }

        public MyObservableList<ViewItemSemester> Semesters { get; private set; }

        internal void Add(ViewItemSemester semester)
        {
            semester.Year = this;

            if (Semesters != null)
                Semesters.InsertSorted(semester);
        }

        public bool IsChild(DataItemSemester dataSemester)
        {
            return dataSemester.UpperIdentifier == Identifier;
        }

        public void CalculateEverything()
        {
            for (int i = 0; i < Semesters.Count; i++)
                Semesters[i].CalculateEverything();

            Calculate();
        }

        private void Calculate()
        {
            GPACalculator.Answer answer = GPACalculator.Calculate(Semesters);

            CalculatedCreditsEarned = answer.CreditsEarned;
            CalculatedCreditsAffectingGpa = answer.CreditsAffectingGpa;
            CalculatedGPA = answer.GPA;
            HasGrades = answer.HasGrades;
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemYear)
            {
                return CompareTo(other as ViewItemYear);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(ViewItemYear other)
        {
            // If both or either unassigned, use normal order
            // This ensures that if all of your years don't have dates and then you add a date to one year,
            // that doesn't cause the year to move where it was from. It will stay in place.
            if (PowerPlannerSending.DateValues.IsUnassigned(this.Start) || PowerPlannerSending.DateValues.IsUnassigned(other.Start))
                return base.CompareTo(other);

            // Otherwise both are assigned, compare by that
            int comp = this.Start.CompareTo(other.Start);
            if (comp == 0)
                return base.CompareTo(other);

            return comp;
        }
    }
}
