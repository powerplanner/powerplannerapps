using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemSemester : BaseViewItems.BaseViewItemWithOverriddenGPACredits, IComparable<ViewItemSemester>
    {
        public new DataItemSemester DataItem
        {
            get { return base.DataItem as DataItemSemester; }
        }

        private ViewItemYear _year;
        /// <summary>
        /// The parent automatically assigns this when Add is called on the parent
        /// </summary>
        public ViewItemYear Year
        {
            get { return _year; }
            internal set
            {
                SetProperty(ref _year, value, "Year");
            }
        }


        private DateTime _start;
        public DateTime Start
        {
            get { return _start; }
            set { SetProperty(ref _start, value, "Start"); }
        }

        private DateTime _end;
        public DateTime End
        {
            get { return _end; }
            set { SetProperty(ref _end, value, "End"); }
        }

        private ViewItemClass _noClassClass;
        public ViewItemClass NoClassClass
        {
            get
            {
                if (_noClassClass == null)
                {
                    _noClassClass = new ViewItemClass(new NoClassDataItemClass()
                    {
                        Name = PowerPlannerResources.GetString("String_NoClass"),
                        RawColor = new byte[] { 84, 107, 199 },
                        UpperIdentifier = Identifier,
                        Identifier = Identifier
                    })
                    {
                        Semester = this
                    };
                }

                return _noClassClass;
            }
        }

        private class NoClassDataItemClass : DataItemClass
        {
            public override string LastTaskTimeOption
            {
                get => Helpers.Settings.LastSelectedTimeOptionForTaskWithoutClass;
                set => Helpers.Settings.LastSelectedTimeOptionForTaskWithoutClass = value;
            }

            public override TimeSpan? LastTaskDueTime
            {
                get => Helpers.Settings.LastSelectedDueTimeForTaskWithoutClass;
                set => Helpers.Settings.LastSelectedDueTimeForTaskWithoutClass = value;
            }

            public override string LastEventTimeOption
            {
                get => Helpers.Settings.LastSelectedTimeOptionForEventWithoutClass;
                set => Helpers.Settings.LastSelectedTimeOptionForEventWithoutClass = value;
            }

            public override TimeSpan? LastEventStartTime
            {
                get => Helpers.Settings.LastSelectedStartTimeForEventWithoutClass;
                set => Helpers.Settings.LastSelectedStartTimeForEventWithoutClass = value;
            }

            public override TimeSpan? LastEventDuration
            {
                get => Helpers.Settings.LastSelectedDurationForEventWithoutClass;
                set => Helpers.Settings.LastSelectedDurationForEventWithoutClass = value;
            }
        }

        public ViewItemSemester(
            Guid identifier,
            Func<DataItemClass, ViewItemClass> createClassMethod = null) : base(identifier)
        {
            Initialize(createClassMethod);
        }

        public class SemesterViewItemChildrenHelper : IViewItemChildrenHelper
        {
            private ViewItemSemester _semester;
            public SemesterViewItemChildrenHelper(ViewItemSemester semester)
            {
                _semester = semester;
            }

            public bool FilterAndAddChildren(IEnumerable<BaseDataItem> potentialChildren)
            {
                bool changed = false;

                // add new classes under the semester
                foreach (var c in potentialChildren.OfType<DataItemClass>().Where(i => _semester.IsChild(i)))
                {
                    _semester.Add(c);
                    changed = true;
                }

                return changed;
            }

            public IEnumerable<BaseViewItem> GetChildren()
            {
                return _semester.Classes;
            }

            public Type GetDataItemType()
            {
                return typeof(DataItemClass);
            }

            public Type GetViewItemType()
            {
                return typeof(ViewItemClass);
            }

            public bool HandleChanges(DataChangedEvent e)
            {
                bool changed = false;

                // Remove any classes that were deleted
                if (_semester.Classes.RemoveWhere(i => e.DeletedItems.Contains(i.Identifier)))
                    changed = true;

                // add new classes under the semester
                foreach (var c in e.NewItems.OfType<DataItemClass>().Where(i => _semester.IsChild(i)))
                {
                    var newViewClass = _semester._createClassMethod.Invoke(c);
                    _semester.Classes.InsertSorted(newViewClass);
                    changed = true;
                }

                // remove classes that were edited and are no longer children
                if (_semester.Classes.RemoveWhere(i => e.EditedItems.OfType<DataItemClass>().Any(edited => edited.Identifier == i.Identifier && edited.UpperIdentifier != _semester.Identifier)))
                    changed = true;

                // Apply the updates
                foreach (var edited in e.EditedItems.OfType<DataItemClass>())
                {
                    var matching = _semester.Classes.FirstOrDefault(i => i.Identifier == edited.Identifier);

                    if (matching != null)
                    {
                        changed = true;
                    }
                }

                // And then re-sort (for any edited classes)
                if (changed)
                    _semester.Classes.Sort();

                return changed;
            }
        }

        public ViewItemSemester(
            DataItemSemester dataItem,
            Func<DataItemClass, ViewItemClass> createClassMethod = null) : base(dataItem)
        {
            Initialize(createClassMethod);
        }

        private Func<DataItemClass, ViewItemClass> _createClassMethod;

        private void Initialize(Func<DataItemClass, ViewItemClass> createClassMethod)
        {
            _createClassMethod = createClassMethod;

            if (createClassMethod != null)
            {
                Classes = new MyObservableList<ViewItemClass>();

                AddChildrenHelper(new SemesterViewItemChildrenHelper(this));
            }
        }

        public MyObservableList<ViewItemClass> Classes { get; private set; }

        internal void Remove(ViewItemClass viewClass)
        {
            if (Classes != null)
                Classes.Remove(viewClass);
        }

        private bool IsChild(DataItemClass dataItem)
        {
            return dataItem.UpperIdentifier == Identifier;
        }

        internal void Add(DataItemClass c)
        {
            Add(_createClassMethod.Invoke(c));
        }

        internal void Add(ViewItemClass viewClass)
        {
            viewClass.Semester = this;

            if (Classes != null)
                Classes.InsertSorted(viewClass);
        }

        public void CalculateEverything()
        {
            for (int i = 0; i < Classes.Count; i++)
                Classes[i].CalculateEverything();

            Calculate();
        }

        private void Calculate()
        {
            GPACalculator.Answer answer = GPACalculator.Calculate(Classes);

            CalculatedCreditsEarned = answer.CreditsEarned;
            CalculatedCreditsAffectingGpa = answer.CreditsAffectingGpa;
            CalculatedGPA = answer.GPA;
            HasGrades = answer.HasGrades;
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemSemester i = dataItem as DataItemSemester;

            // We don't localize to time zone since these are just raw dates
            Start = DateTime.SpecifyKind(i.Start, DateTimeKind.Local);
            End = DateTime.SpecifyKind(i.End, DateTimeKind.Local);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date">This should be in local time.</param>
        /// <returns></returns>
        public bool IsDateDuringThisSemester(DateTime date)
        {
            // If the Start is okay
            if (PowerPlannerSending.DateValues.IsUnassigned(Start) || date >= DateTime.SpecifyKind(Start, DateTimeKind.Local))
            {
                // If the End is okay
                return PowerPlannerSending.DateValues.IsUnassigned(End) || date <= DateTime.SpecifyKind(End, DateTimeKind.Local);
            }

            // Otherwise Start wasn't okay
            return false;
        }

        public bool IsMonthDuringThisSemester(DateTime firstDayOfMonth)
        {
            if (!PowerPlannerSending.DateValues.IsUnassigned(this.Start) && DateTools.LastDayOfMonth(firstDayOfMonth.Date) < DateTime.SpecifyKind(this.Start.Date, DateTimeKind.Local))
                return false;

            if (!PowerPlannerSending.DateValues.IsUnassigned(this.End) && firstDayOfMonth.Date > DateTime.SpecifyKind(this.End.Date, DateTimeKind.Local))
                return false;

            return true;
        }

        public override int CompareTo(BaseViewItem other)
        {
            if (other is ViewItemSemester)
                return CompareTo(other as ViewItemSemester);

            return base.CompareTo(other);
        }

        public int CompareTo(ViewItemSemester other)
        {
            // If both or either unassigned, use normal order
            // This ensures that if all of your semesters don't have dates and then you add a date to one semester,
            // that doesn't cause the semester to move where it was from. It will stay in place.
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
